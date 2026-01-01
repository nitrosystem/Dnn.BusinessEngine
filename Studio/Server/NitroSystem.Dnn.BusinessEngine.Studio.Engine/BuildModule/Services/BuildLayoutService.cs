using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ConditionParser;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.SseNotifier;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services
{
    public class BuildLayoutService : IBuildLayoutService
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly ISseNotifier _notifier;
        private readonly IModuleFieldService _moduleFieldService;

        private ConcurrentDictionary<(string fieldType, string template), string> _fieldTypes = new
                ConcurrentDictionary<(string fieldType, string template), string>();
        private IDictionary<Guid, List<ModuleFieldDto>> _fieldMap;
        private Queue<ModuleFieldDto> _buffer;
        private HtmlDocument _htmlDoc;
        private volatile int _userId;
        private ModuleDto _module;
        private IEngineNotifier _Notifier;

        private readonly string _doubleBracketsPattern = @"\[\[(?<Exp>.[^:\[\[\]\]\?\?]+)(\?\?)?(?<NullValue>.[^\[\[\]\]]*)?\]\]";
        private readonly string _conditionPattern = @"\[\[\s*IF:\s*(?<Condition>.+?)\s*:\s*(?<Exp>.[^\[\[\]\]]+)\s*\]\]";
        private readonly string _fieldLayout =
            @"<div [[IF:ShowConditions != null && ShowConditions != """":b-if=""[[ShowConditions]]""]] class=""[[Settings.CssClass??b-field]]"" [[IF:CanHaveValue == true:b-class=""{'b-field-invalid':[FIELD].isValidated==true && ([FIELD].requiredError==true || [FIELD].patternError==true)}""]]>
                [[IF:GlobalSettings.IsHiddenFieldText == false && FieldText != null:
                    <label class=""[[Settings.FieldTextCssClass??b-field-label]]"">[[FieldText]]</label>
                ]]
                [FIELD-COMPONENT]
                [[IF:CanHaveValue == true && IsRequired == true:
                    <p b-show=""[FIELD].isValidated==true && [FIELD].isValid==false && [FIELD].requiredError==true"" class=""[[Settings.RequiredMessageCssClass??b-field-invalid-message]]"">[[Settings.RequiredMessage]]</p>
                ]]
                [[IF:CanHaveValue == true && GlobalSettings.EnableValidationPattern == true && GlobalSettings.ValidationPattern != null:
                    <p b-show=""[FIELD].isValidated && ![FIELD].isValid && [FIELD].patternError"" class=""[[Settings.ValidationMessageCssClass??b-field-pattern-message]]"">[[Settings.ValidationMessage]]</p>
                ]]
                [[IF:GlobalSettings.Subtext != null:
                    <span class=""[[Settings.SubtextCssClass??b-field-subtext]]"">[[Settings.Subtext]]</span>
                ]]
            </div>";

        public BuildLayoutService(IServiceLocator serviceLocator, ISseNotifier notifier, IModuleFieldService moduleFieldService)
        {
            _serviceLocator = serviceLocator;
            _notifier = notifier;
            _moduleFieldService = moduleFieldService;
        }

        public async Task<string> BuildLayoutAsync(ModuleDto module, int userId)
        {
            _module = module;
            _userId = userId;

            _fieldMap = _module.Fields
                .GroupBy(f => f.ParentId ?? Guid.Empty)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(f => f.ViewOrder).ToList()
                );

            if (!_fieldMap.TryGetValue(Guid.Empty, out var parents))
            {
                throw new Exception("The module does not have any fields!");
            }

            //await _workflow.ExecuteTaskAsync<object>(_module.Id.ToString(), _userId,
            //    "BuildModuleWorkflow", "BuildModule", "BuildLayoutMiddleware", false, true, true,
            //        (Expression<Func<Task>>)(() => LoadTemplates(_module.Fields))
            //    );

            await LoadTemplates(_module.Fields);

            await _notifier.Publish(_module.ScenarioName,
               new
               {
                   Channel = _module.ScenarioName,
                   Type = "ActionCenter",
                   TaskId = $"{_module.Id}-BuildModule",
                   Message = $"Loaded resource content of {_module.ModuleName} module",
                   Percent = 40
               }
           );

            _buffer = new Queue<ModuleFieldDto>();
            foreach (var item in parents)
            {
                _buffer.Enqueue(item);
                CreateBuffer(item);
            }

            _htmlDoc = new HtmlDocument();
            _htmlDoc.LoadHtml(_module.LayoutTemplate);

            await ProcessBuffer(_buffer.Count);

            return _htmlDoc.DocumentNode.OuterHtml;
        }

        #region Private Methods

        private async Task LoadTemplates(IEnumerable<ModuleFieldDto> fields)
        {
            await BatchExecutor.ExecuteInBatchesAsync(
                fields,
                batchSize: 5,
                async batch =>
                {
                    // کلید: TemplatePath (نرمال‌سازی شده)
                    var items = batch
                        .GroupBy(f => f.TemplatePath?.ReplaceFrequentTokens())
                        .ToDictionary(
                            g => g.Key,
                            g => g.First()
                        );

                    await FileUtil.LoadFilesAsync(
                        items.Keys,
                        Constants.MapPath,
                        (itemKey, fileContent) =>
                        {
                            if (items.TryGetValue(itemKey, out var field))
                            {
                                _fieldTypes[(field.FieldType, field.Template)] = fileContent;
                            }
                        });
                });
        }

        private void CreateBuffer(ModuleFieldDto field)
        {
            if (!_fieldMap.TryGetValue(field.Id, out var childs))
                return;

            field.IsParent = childs.Any();

            foreach (var childField in childs)
            {
                _buffer.Enqueue(childField);
                if (field.ParentId.HasValue) CreateBuffer(childField);
            }
        }

        private async Task ProcessBuffer(int index)
        {
            if (index <= 0) return;

            var field = _buffer.Dequeue();

            try
            {
                var node = GetPaneNode(field.PaneName);
                var fieldHtml = await ParseFieldTemplate(field);

                //var fieldHtml = await _workflow.ExecuteTaskAsync<string>(_module.Id.ToString(), _userId,
                //    "BuildModuleWorkflow", "BuildModule", "BuildLayoutMiddleware", false, true, false,
                //   (Expression<Func<Task<string>>>)(() => ParseFieldTemplate(field))
                //);

                var htmlNode = HtmlNode.CreateNode(fieldHtml);
                node.AppendChild(htmlNode);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            await ProcessBuffer(index - 1);
        }

        private async Task<string> ParseFieldTemplate(ModuleFieldDto field)
        {
            var fieldKey = (field.FieldType, field.Template);
            _fieldTypes.TryGetValue(fieldKey, out var fieldTemplate);

            var contentValue = field.Settings.GetValueOrDefault("Content");
            if (contentValue != null)
            {
                return contentValue as string;
            }
            else if (string.IsNullOrEmpty(fieldTemplate))
                return string.Empty;

            try
            {
                if (field.IsParent && field.IsGroupField && !string.IsNullOrEmpty(field.FieldTypeGeneratePanesBusinessControllerClass))
                {
                    var type = await _moduleFieldService.GenerateFieldTypePanesBusinessControllerClassAsync(field.FieldType);
                    if (!string.IsNullOrEmpty(type))
                    {
                        var controller = _serviceLocator.GetInstance<IFieldTypePaneGeneration>(type);
                        var panes = await controller.GeneratePanes(field);
                        fieldTemplate = fieldTemplate.Replace("[FIELDPANES]", panes ?? string.Empty);
                    }
                }

                if (field.GlobalSettings.IsDisabledLayout && !string.IsNullOrEmpty(field.ShowConditions))
                    fieldTemplate = fieldTemplate.Replace("[TOKENS]", @"[[IF:ShowConditions != null && ShowConditions != """":b-if=""[[ShowConditions]]""]][TOKENS]");
                if (!field.GlobalSettings.IsDisabledLayout && field.GlobalSettings.IsCustomFieldLayout)
                    fieldTemplate = (field.GlobalSettings.CustomFieldLayout ?? _fieldLayout).Replace("[FIELD-COMPONENT]", fieldTemplate);
                else if (!field.GlobalSettings.IsDisabledLayout)
                    fieldTemplate = _fieldLayout.Replace("[FIELD-COMPONENT]", fieldTemplate);

                if (!string.IsNullOrWhiteSpace(field.GlobalSettings.CustomStyles))
                    fieldTemplate = fieldTemplate.Replace("[TOKENS]", $@" style=""{field.GlobalSettings.CustomStyles}""[TOKENS]");

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(field);
                JObject jObject = JObject.Parse(json);

                // --------------------- Double Braket Proccess --------------------------
                var matches = Regex.Matches(fieldTemplate, _doubleBracketsPattern);
                foreach (Match match in matches)
                {
                    var value = "";
                    try
                    {
                        var expression = match.Groups["Exp"].Value;
                        var jtoken = jObject.SelectToken(expression);

                        if (jtoken != null)
                            value = jtoken.Value<string>() ?? "";
                        else if (jtoken == null || string.IsNullOrEmpty(value))
                            value = match.Groups["NullValue"].Value;
                    }
                    catch (Exception ex)
                    {
                        //throw ex;
                    }

                    fieldTemplate = fieldTemplate.Replace(match.Value, value ?? "");
                }

                fieldTemplate = fieldTemplate.Replace("[FIELD]", $"field.{field.FieldName}");
                fieldTemplate = fieldTemplate.Replace("[FIELDNAME]", $"{field.FieldName}");
                fieldTemplate = fieldTemplate.Replace("[FIELDID]", $"{field.Id}");

                // --------------------- Condition Expression Proccess --------------------------
                matches = Regex.Matches(fieldTemplate, _conditionPattern, RegexOptions.Singleline);
                foreach (Match match in matches)
                {
                    var conditionResult = false;
                    try
                    {
                        var condition = match.Groups["Condition"].Value;
                        var expressionTree = ConditionParser.Parse(condition);

                        var param = Expression.Parameter(typeof(ModuleFieldDto));
                        var expression = expressionTree.BuildExpression(param);

                        var lambda = Expression.Lambda<Func<ModuleFieldDto, bool>>(expression, param).Compile();
                        conditionResult = lambda(field);
                    }
                    catch (Exception ex)
                    {
                        //throw ex;
                    }

                    fieldTemplate = fieldTemplate.Replace(match.Value, conditionResult
                        ? (match.Groups["Exp"].Value ?? "")
                        : string.Empty);

                    await _notifier.Publish(_module.ScenarioName,
                       new
                       {
                           Channel = _module.ScenarioName,
                           Type = "ActionCenter",
                           TaskId = $"{_module.Id}-BuildModule",
                           Message = $"Render template {field.FieldType} of {_module.ModuleName} module",
                           Percent = 70
                       }
                    );
                }
            }
            catch (Exception exx)
            {
                throw exx;
            }

            return fieldTemplate.Replace("[TOKENS]", $@"data-fi=""{field.Id}""");
        }

        private HtmlNode GetPaneNode(string pane)
        {
            HtmlNode result = null;

            var nodes = _htmlDoc.DocumentNode.SelectNodes("//*[@data-pane]");
            foreach (var item in nodes)
            {
                string paneName = item.GetAttributeValue("data-pane", "");
                if (!string.IsNullOrEmpty(paneName) && paneName == pane)
                {
                    result = item;
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}
