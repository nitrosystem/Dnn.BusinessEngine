using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionService.ConditionParser;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleLayout : IBuildModuleLayout
    {
        private readonly IServiceLocator _serviceLocator;

        public BuildModuleLayout(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        #region Members

        private IDictionary<(string fieldType, string template), string> _fieldTypes;
        private IDictionary<Guid, List<BuildModuleFieldDto>> _fieldMap;
        private Queue<BuildModuleFieldDto> _buffer;
        private HtmlDocument _htmlDoc;

        private readonly string _doubleBracketsPattern = @"\[\[(?<Exp>.[^:\[\[\]\]\?\?]+)(\?\?)?(?<NullValue>.[^\[\[\]\]]*)?\]\]";
        private readonly string _conditionPattern = @"\[\[\s*IF:\s*(?<Condition>.+?)\s*:\s*(?<Exp>.[^\[\[\]\]]+)\s*\]\]";
        private readonly string _fieldLayout =
            @"<div [FIELD-DISPLAY-EXPRESSION] class=""[[Settings.CssClass??b-field]]"" [[IF:CanHaveValue ==true:ng-class=""{'b-invalid':[FIELD].Validated && ([FIELD].RequiredError || ![FIELD].IsValid)}""]]>
                [[IF:FieldText!=null:<label class=""[[Settings.FieldTextCssClass??b-form-label]]"">[[FieldText]]</label>]]
                [FIELD-COMPONENT]
                [[IF:CanHaveValue ==true && IsRequired==true:<p ng-show=""[FIELD].Validated && ([FIELD].Value==null || [FIELD].Value==undefined || [FIELD].Value=='') && [FIELD].RequiredError"" 
                    class=""[[Settings.RequiredMessageCssClass??b-invlid-message]]"">[[Settings.RequiredMessage]]</p>]]
                [[IF:GlobalSettings.EnableValidationPattern==true&&Settings.ValidationPattern!=null:<p ng-show=""[FIELD].IsPatternValidate && ![FIELD].IsValid && [FIELD].Value!==undefined && [FIELD].Value!==null && [FIELD].Value!==''"" 
                    class=""[[Settings.ValidationMessageCssClass??b-pattern-message]]"">[[Settings.ValidationMessage]]</p>]]
                [[IF:GlobalSettings.Subtext!=null:<span class=""[[Settings.SubtextCssClass??b-subtext]]"">[[Settings.Subtext]]</span>]]
            </div>";

        #endregion

        public string BuildLayout(string layoutTemplate, IDictionary<(string fieldType, string template), string> fieldTypes, IEnumerable<BuildModuleFieldDto> fields)
        {
            _fieldTypes = fieldTypes;

            _fieldMap = fields
                .GroupBy(f => f.ParentId ?? Guid.Empty)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(f => f.ViewOrder).ToList()
                );

            var result = new BuildModuleResultDto();

            if (!_fieldMap.TryGetValue(Guid.Empty, out var parents))
            {
                throw new Exception("The module does not have any fields!");
            }

            _buffer = new Queue<BuildModuleFieldDto>();

            foreach (var item in parents)
            {
                _buffer.Enqueue(item);
                CreateBuffer(item);
            }

            _htmlDoc = new HtmlDocument();
            _htmlDoc.LoadHtml(layoutTemplate);

            ProcessBuffer(_buffer.Count);

            return _htmlDoc.DocumentNode.OuterHtml;
        }

        #region Private Methods

        private void CreateBuffer(BuildModuleFieldDto field)
        {
            if (!_fieldMap.TryGetValue(field.Id, out var childs))
                return;

            field.IsParent = childs.Any();

            foreach (var childField in childs)
            {
                _buffer.Enqueue(childField);
                CreateBuffer(childField);
            }
        }

        private void ProcessBuffer(int index)
        {
            if (index <= 0) return;

            var field = _buffer.Dequeue();

            var node = GetPaneNode(field.PaneName);
            var fieldHtml = ParseFieldTemplate(field);
            var htmlNode = HtmlNode.CreateNode(fieldHtml);

            node.AppendChild(htmlNode);

            ProcessBuffer(index - 1);
        }

        private string ParseFieldTemplate(BuildModuleFieldDto field)
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
                //اگر فیلد از نوع گروه بود و برای ایجاد پین های فیلد نیاز به فراخوانی کد اکسیتن لازم می باشد
                if (field.IsParent && field.IsGroupField && !string.IsNullOrEmpty(field.FieldTypeGeneratePanesBusinessControllerClass))
                {
                    var serviceLocator = CreateInstance(field.FieldTypeGeneratePanesBusinessControllerClass);
                    if (serviceLocator != null)
                    {
                        var panes = serviceLocator.GeneratePanes(field.Id);

                        fieldTemplate = fieldTemplate.Replace("[FIELDPANES]", panes ?? string.Empty);
                    }
                }

                fieldTemplate = _fieldLayout.Replace("[FIELD-COMPONENT]", fieldTemplate);

                // --------------------- PARSE Field Show Conditions --------------------------
                if (field.ShowConditions != null && field.ShowConditions.Any())
                {
                    fieldTemplate = fieldTemplate.Replace("[FIELD-DISPLAY-EXPRESSION]", $@"ng-if=""$.field.{field.FieldName}.IsShow""");
                }
                else
                {
                    fieldTemplate = fieldTemplate.Replace("[FIELD-DISPLAY-EXPRESSION]", "");
                }

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
                    }

                    fieldTemplate = fieldTemplate.Replace(match.Value, value ?? "");
                }

                fieldTemplate = fieldTemplate.Replace("[FIELD]", $"$.field.{field.FieldName}");
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

                        var param = Expression.Parameter(typeof(BuildModuleFieldDto));
                        var expression = expressionTree.BuildExpression(param);

                        var lambda = Expression.Lambda<Func<BuildModuleFieldDto, bool>>(expression, param).Compile();
                        conditionResult = lambda(field);
                    }
                    catch (Exception ex)
                    {
                    }

                    fieldTemplate = fieldTemplate.Replace(match.Value, conditionResult
                        ? (match.Groups["Exp"].Value ?? "")
                        : string.Empty);
                }
            }
            catch (Exception exx)
            {
            }

            return fieldTemplate;
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

        private IField CreateInstance(string businessControllerClass)
        {
            if (string.IsNullOrEmpty(businessControllerClass)) return null;

            return _serviceLocator.CreateInstance<IField>(businessControllerClass);
        }

        #endregion
    }
}
