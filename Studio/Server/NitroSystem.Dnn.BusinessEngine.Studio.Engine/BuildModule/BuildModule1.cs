using DotNetNuke.Common.Utilities;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Contract;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using static System.Collections.Specialized.BitVector32;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationEngine.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Reflection;
namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationEngine.BuildModule
{
    public class BuildModule1 : IBuildModule
    {
        private readonly IEnumerable<IModuleField> _fields;

        public BuildModule1(IEnumerable<IModuleField> fields)
        {
            _fields = fields;
        }

        private List<IModuleField> Buffer;
        private HtmlDocument HtmlDoc;

        private readonly string DoubleBracketsPattern = @"\[\[(?<Exp>.[^:\[\[\]\]\?\?]+)(\?\?)?(?<NullValue>.[^\[\[\]\]]+)?\]\]";
        private readonly string ConditionPattern = @"\[\[IF:(?<Condition>.[^:]+):(?<Exp>.[^\[\[\]\]]+)\]\]";
        private readonly string FieldLayout =
            @"<div data-field=""[[FieldName]]"" [FIELD-DISPLAY-EXPRESSION] class=""[[Settings.CssClass??b-field]]"" [[IF:field.IsValuable:ng-class=""{'b-invalid':[FIELD].Validated && ([FIELD].RequiredError || ![FIELD].IsValid)}""]]>
                [[IF:FieldText!=null && IsValuable==true:<label class=""[[Settings.FieldTextCssClass??b-form-label]]"">[[FieldText]]</label>]]
                [FIELD-COMPONENT]
                [[IF:IsValuable==true && IsRequired==true:<p ng-show=""[FIELD].Validated && ([FIELD].Value==null || [FIELD].Value==undefined || [FIELD].Value=='') && [FIELD].RequiredError"" 
                    class=""[[Settings.RequiredMessageCssClass??b-invlid-message]]"">[[Settings.RequiredMessage]]</p>]]
                [[IF:GlobalSettings.EnableValidationPattern==true&&Settings.ValidationPattern!=null:<p ng-show=""[FIELD].IsPatternValidate && ![FIELD].IsValid && [FIELD].Value!==undefined && [FIELD].Value!==null && [FIELD].Value!==''"" 
                    class=""[[Settings.ValidationMessageCssClass??b-pattern-message]]"">[[Settings.ValidationMessage]]</p>]]
                [[IF:GlobalSettings.Subtext!=null:<span class=""[[Settings.SubtextCssClass??b-subtext]]"">[[Settings.Subtext]]</span>]]
            </div>";

        public async Task<string> Build(BuildModuleDto data)
        {
            var fields = _fields;

            this.Buffer = new List<IModuleField>();

            var parents = fields.Where(f => f.ParentID == null).OrderBy(f => f.ViewOrder);
            foreach (var item in parents)
            {
                this.Buffer.Add(item);
                await CreateBuffer(item);
            }

            this.HtmlDoc = new HtmlDocument();
            this.HtmlDoc.LoadHtml(layoutTemplate);

            var result = await ProcessBuffer(Buffer.Count);

            return result;
        }

        private async Task<string> ProcessBuffer(int index)
        {
            if (index <= 0) return this.HtmlDoc.DocumentNode.OuterHtml;

            var field = this.Buffer[0];
            var pane = field.PaneName;

            var node = GetPaneNode(pane);

            var fieldHtml = await GetFieldTemplateAsync(field);

            HtmlNode htmlNode = HtmlNode.CreateNode(fieldHtml);

            node.AppendChild(htmlNode);

            this.Buffer.RemoveAt(0);

            return await ProcessBuffer(index - 1);
        }

        private HtmlNode GetPaneNode(string pane)
        {
            HtmlNode result = null;

            var nodes = this.HtmlDoc.DocumentNode.SelectNodes("//*[@data-pane]");
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

        private async Task<string> GetFieldTemplateAsync(IModuleField field)
        {
            string result = string.Empty;
            try
            {
                var objModuleFieldTypeTemplateInfo = ModuleFieldTypeTemplateRepository.Instance.GetFieldTemplate(field.FieldType, field.Template);
                if (objModuleFieldTypeTemplateInfo != null)
                {
                    result = await FileUtil.GetFileContentAsync(HttpContext.Current.Server.MapPath(objModuleFieldTypeTemplateInfo.TemplatePath.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions")));
                }

                if (field.IsParent)
                {
                    var fieldController = CreateInstance(field.FieldType);
                    if (fieldController != null)
                    {
                        var panes = fieldController.GeneratePanes(field.FieldID);

                        result = result.Replace("[FIELDPANES]", panes);
                    }
                }

                result = FieldLayout.Replace("[FIELD-COMPONENT]", result);

                // --------------------- PARSE Field Show Conditions --------------------------
                if (field.ShowConditions != null && field.ShowConditions.Any())
                {
                    result = result.Replace("[FIELD-DISPLAY-EXPRESSION]", $@"ng-if=""Field.{field.FieldName}.IsShow""");
                }
                else
                {
                    result = result.Replace("[FIELD-DISPLAY-EXPRESSION]", "");
                }

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(field);
                JObject jObject = JObject.Parse(json);

                // --------------------- Double Braket Proccess --------------------------
                var matches = Regex.Matches(result, DoubleBracketsPattern);
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

                    result = result.Replace(match.Value, value ?? "");
                }

                result = result.Replace("[FIELD]", $"Field.{field.FieldName}");
                result = result.Replace("[FIELDNAME]", $"{field.FieldName}");
                result = result.Replace("[FIELDID]", $"{field.FieldID}");

                // --------------------- Condition Expression Proccess --------------------------
                matches = Regex.Matches(result, ConditionPattern);
                foreach (Match match in matches)
                {
                    var conditionResult = false;
                    try
                    {
                        var condition = match.Groups["Condition"].Value;

                        var expressionTree = ConditionParser.Parse(condition);

                        var param = Expression.Parameter(typeof(IModuleField));
                        var expression = expressionTree.BuildExpression(param);
                        var lambda = Expression.Lambda<Func<IModuleField, bool>>(expression, param).Compile();
                        conditionResult = lambda(field);
                    }
                    catch (Exception ex)
                    {
                    }

                    result = result.Replace(match.Value, conditionResult ? (match.Groups["Exp"].Value ?? "") : "");
                }
            }
            catch (Exception exx)
            {
            }

            return result;
        }

        private async Task CreateBuffer(IModuleField field)
        {
            var childs = _fields.Where(f => f.ParentID == field.FieldID).OrderBy(f => f.ViewOrder);

            field.IsParent = childs.Any();

            foreach (var childField in childs)
            {
                this.Buffer.Add(childField);

                await CreateBuffer(childField);
            }
        }

        private IField CreateInstance(string fieldType)
        {
            var objModuleFieldTypeInfo = ModuleFieldTypeRepository.Instance.GetFieldTypeByName(fieldType);

            if (string.IsNullOrEmpty(objModuleFieldTypeInfo.GeneratePanesBusinessControllerClass)) return null;

            return ServiceLocator<IField>.CreateInstance(objModuleFieldTypeInfo.GeneratePanesBusinessControllerClass);
        }
    }
}