using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ConditionParser;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services
{
    public class BuildLayoutService : IBuildLayoutService
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly IModuleFieldService _moduleFieldService;

        private ConcurrentDictionary<(string fieldType, string template), string> _fieldTypes =
            new ConcurrentDictionary<(string fieldType, string template), string>();

        private IDictionary<Guid, List<ModuleFieldDto>> _fieldMap;
        private Queue<ModuleFieldDto> _buffer;

        // جایگزین HtmlDocument
        private Dictionary<string, PaneDefinition> _panes;
        private string _layoutTemplate;

        private volatile int _userId;
        private ModuleDto _module;

        private Action<string, double> _onProgress;
        private int _fieldIndex;
        private double _progressStep;

        private static readonly Regex PaneTagRegex =
            new Regex(
                @"<(?<tag>\w+)(?<attrs>[^>]*?)\sdata-pane\s*=\s*""(?<name>[^""]+)""(?<attrs2>[^>]*?)>",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly string _doubleBracketsPattern =
            @"\[\[(?<Exp>.[^:\[\[\]\]\?\?]+)(\?\?)?(?<NullValue>.[^\[\[\]\]]*)?\]\]";

        private readonly string _conditionPattern =
            @"\[\[\s*IF:\s*(?<Condition>.+?)\s*:\s*(?<Exp>.[^\[\[\]\]]+)\s*\]\]";

        private readonly string _fieldLayout =
            @"<div [[IF:HiddenConditions != null && HiddenConditions != """":b-if=""!([[HiddenConditions]])""]] class=""[[Settings.CssClass??b-field]]"" [[IF:CanHaveValue == true:b-class=""{'b-field-invalid':[FIELD].isValidated==true && ([FIELD].requiredError==true || [FIELD].patternError==true)}""]]>
                [[IF:GlobalSettings.IsHiddenFieldText == false && FieldText != null:
                    <label class=""[[Settings.FieldTextCssClass??b-field-label]]"">[[FieldText]]</label>
                ]]
                [FIELD-COMPONENT]
                [[IF:CanHaveValue == true && IsRequired == true:
                    <p b-show=""[FIELD].isValidated==true && [FIELD].isValid==false && [FIELD].requiredError==true"" class=""[[Settings.RequiredMessageCssClass??b-field-invalid-message]]"" style=""display:none;"">[[Settings.RequiredMessage]]</p>
                ]]
                [[IF:CanHaveValue == true && GlobalSettings.EnableValidationPattern == true && GlobalSettings.ValidationPattern != null:
                    <p b-show=""[FIELD].isValidated && ![FIELD].isValid && [FIELD].patternError"" class=""[[Settings.ValidationMessageCssClass??b-field-pattern-message]]"" style=""display:none;"">[[Settings.ValidationMessage]]</p>
                ]]
                [[IF:GlobalSettings.Subtext != null:
                    <span class=""[[Settings.SubtextCssClass??b-field-subtext]]"">[[Settings.Subtext]]</span>
                ]]
            </div>";

        private class PaneDefinition
        {
            public string Name { get; }
            public string Title { get; }
            public bool Injected { get; set; }
            public StringBuilder Buffer { get; }
            public Func<string, string> Injector { get; set; }

            public PaneDefinition(string name, string title)
            {
                Name = name;
                Title = title;
                Buffer = new StringBuilder();
            }
        }

        public BuildLayoutService(IServiceLocator serviceLocator, IModuleFieldService moduleFieldService)
        {
            _serviceLocator = serviceLocator;
            _moduleFieldService = moduleFieldService;
        }

        public async Task<string> BuildLayoutAsync(ModuleDto module, int userId, Action<string, double> progress)
        {
            _module = module;
            _userId = userId;
            _onProgress = progress;

            _fieldIndex = 0;
            _progressStep = 85 / module.Fields.Count();

            _layoutTemplate = module.LayoutTemplate;
            _panes = new Dictionary<string, PaneDefinition>();

            await LoadTemplates(_module.Fields);

            InitializePanes();

            _onProgress.Invoke($"Loaded resource content of {_module.ModuleName} module", 15);

            _fieldMap = _module.Fields
                .GroupBy(f => f.ParentId ?? Guid.Empty)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(f => f.ViewOrder).ToList()
                );
            _fieldMap.TryGetValue(Guid.Empty, out var roots);
            _buffer = new Queue<ModuleFieldDto>();

            foreach (var item in roots)
            {
                _buffer.Enqueue(item);
                CreateBuffer(item);
            }

            await ProcessBuffer(_buffer.Count);

            InjectAllPanes(); // تزریق نهایی، layout + runtime

            return _layoutTemplate;
        }

        #region Pane Engine

        private void InitializePanes() => RegisterPanesFromHtml(_layoutTemplate);

        private void RegisterPanesFromHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return;

            foreach (Match match in PaneTagRegex.Matches(html))
            {
                var name = match.Groups["name"].Value;
                if (_panes.ContainsKey(name))
                    continue;

                var attrs = match.Groups["attrs"].Value + match.Groups["attrs2"].Value;
                string title = null;
                var titleMatch = Regex.Match(attrs, @"data-pane-title\s*=\s*""(?<title>[^""]+)""", RegexOptions.IgnoreCase);
                if (titleMatch.Success) title = titleMatch.Groups["title"].Value;

                var paneDef = new PaneDefinition(name, title);

                // ← اینجا بهترین جا برای تنظیم Injector
                paneDef.Injector = layout =>
                {
                    var hostPattern = $@"(<[^>]+data-pane\s*=\s*""{paneDef.Name}""[^>]*>)";
                    return Regex.Replace(layout, hostPattern, m => m.Value + paneDef.Buffer);
                };

                _panes[name] = paneDef;
            }
        }

        private StringBuilder GetPaneBuffer(string pane)
        {
            if (!_panes.TryGetValue(pane, out var paneDef))
                throw new InvalidOperationException($"Pane '{pane}' not found.");
            return paneDef.Buffer;
        }

        private void InjectAllPanes()
        {
            foreach (var pane in _panes.Values)
            {
                if (pane.Injected)
                    continue;

                if (pane.Buffer.Length == 0)
                    continue;

                _layoutTemplate = pane.Injector(_layoutTemplate);
                pane.Injected = true;
            }
        }

        #endregion

        #region Buffer Management

        private void CreateBuffer(ModuleFieldDto field)
        {
            if (!_fieldMap.TryGetValue(field.Id, out var childs))
                return;

            field.IsParent = childs.Any();

            foreach (var child in childs)
            {
                _buffer.Enqueue(child);
                CreateBuffer(child);
            }
        }

        private async Task ProcessBuffer(int index)
        {
            if (index <= 0) return;

            var field = _buffer.Dequeue();
            var pane = GetPaneBuffer(field.PaneName);
            var html = await ParseFieldTemplate(field);

            pane.AppendLine(html);

            await ProcessBuffer(index - 1);
        }

        #endregion

        #region Template Parsing

        private async Task LoadTemplates(IEnumerable<ModuleFieldDto> fields)
        {
            await BatchExecutor.ExecuteInBatchesAsync(fields, 5, async batch =>
            {
                var items = batch.GroupBy(f => f.TemplatePath?.ReplaceFrequentTokens())
                                 .ToDictionary(g => g.Key, g => g.First());

                await FileUtil.LoadFilesAsync(items.Keys, Constants.MapPath, (key, content) =>
                {
                    if (items.TryGetValue(key, out var field))
                        _fieldTypes[(field.FieldType, field.Template)] = content;
                });
            });
        }

        private async Task<string> ParseFieldTemplate(ModuleFieldDto field)
        {
            _fieldIndex++;

            var key = (field.FieldType, field.Template);
            _fieldTypes.TryGetValue(key, out var template);

            var contentValue = field.Settings.GetValueOrDefault("Content");
            if (contentValue != null)
                return contentValue as string ?? string.Empty;

            if (string.IsNullOrEmpty(template))
                return string.Empty;

            if (field.IsParent && field.IsGroupField)
            {
                string panesHtml = string.Empty;
                if (!string.IsNullOrEmpty(field.FieldTypeGeneratePanesBusinessControllerClass))
                {
                    var type = await _moduleFieldService.GenerateFieldTypePanesBusinessControllerClassAsync(field.FieldType);
                    if (!string.IsNullOrEmpty(type))
                    {
                        var controller = _serviceLocator.GetInstance<IFieldTypePaneGeneration>(type);
                        panesHtml = await controller.GeneratePanes(field);
                    }
                }
                else panesHtml = template;

                panesHtml = panesHtml.Replace("[FIELD]", $"field.{field.FieldName}")
                                   .Replace("[FIELDNAME]", field.FieldName)
                                   .Replace("[FIELDID]", field.Id.ToString());

                // 🔴 ثبت pane های runtime
                RegisterPanesFromHtml(panesHtml);

                template = template.Replace("[FIELDPANES]", panesHtml ?? string.Empty);
            }

            if (field.GlobalSettings.IsDisabledLayout && !string.IsNullOrEmpty(field.HiddenConditions))
                template = template.Replace("[TOKENS]", @"[[IF:HiddenConditions != null && HiddenConditions != """":b-if=""!([[HiddenConditions]])""]] [TOKENS]");
            if (!field.GlobalSettings.IsDisabledLayout && field.GlobalSettings.IsCustomFieldLayout)
                template = (field.GlobalSettings.CustomFieldLayout ?? _fieldLayout).Replace("[FIELD-COMPONENT]", template);
            else if (!field.GlobalSettings.IsDisabledLayout)
                template = _fieldLayout.Replace("[FIELD-COMPONENT]", template);

            if (!string.IsNullOrWhiteSpace(field.GlobalSettings.CustomStyles))
                template = template.Replace("[TOKENS]", $@" style=""{field.GlobalSettings.CustomStyles}""[TOKENS]");

            // JSON Token Binding
            var json = JsonConvert.SerializeObject(field);
            var jObject = JObject.Parse(json);

            // --------------------- Double Braket Proccess --------------------------
            var matches = Regex.Matches(template, _doubleBracketsPattern);
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

                template = template.Replace(match.Value, value ?? "");
            }

            template = template.Replace("[FIELD]", $"field.{field.FieldName}");
            template = template.Replace("[FIELDNAME]", $"{field.FieldName}");
            template = template.Replace("[FIELDID]", $"{field.Id}");

            // --------------------- Condition Expression Proccess --------------------------
            matches = Regex.Matches(template, _conditionPattern, RegexOptions.Singleline);
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

                template = template.Replace(match.Value, conditionResult
                    ? (match.Groups["Exp"].Value ?? "")
                    : string.Empty);
            }

            _onProgress.Invoke($"Render template {field.FieldType} of {_module.ModuleName} module", 15 + (_fieldIndex * _progressStep));

            return template.Replace("[TOKENS]", $@"__b=""{field.Id}""");
        }

        #endregion
    }
}
