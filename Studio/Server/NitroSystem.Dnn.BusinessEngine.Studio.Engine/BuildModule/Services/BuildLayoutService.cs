using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Models;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine;
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
		private IEngineContext _context;
		private Queue<ModuleFieldDto> _buffer;
		private Dictionary<string, PaneDefinition> _panes;
		private ModuleDto _module;
		private Action<string, double> _onProgress;
		private string _layoutTemplate;
		private int _fieldIndex;
		private double _progressStep;

		private static readonly Regex PaneTagRegex =
			new Regex(
				@"<(?<tag>\w+)(?<attrs>[^>]*?)\sdata-pane\s*=\s*""(?<name>[^""]+)""(?<attrs2>[^>]*?)>",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private readonly string _doubleBracketsPattern =
			@"\[\[(?<Exp>.[^:\[\[\]\]\?\?]+)(\?\?)?(?<NullValue>.[^\[\[\]\]]*)?\]\]";

		private readonly string _ifPattern =
			@"\[\[\s*IF:\s*(?<Condition>.+?)\s*:\s*(?<Exp>.+?)\s*\]\]";

		private readonly string _bConditionPattern = @"<b-condition\b[^>]*\bcon\s*=\s*'([^']*)'[^>]*>([\s\S]*?)</b-condition>";

		private readonly string _fieldLayout =
			@"<div [[IF:Field.HiddenConditions != null and Field.HiddenConditions != """":b-if='!([[HiddenConditions]])']] class=""[[Settings.CssClass]]"" [[IF:Field.CanHaveValue == true and Field.Settings.InvalidCssClass != null and Field.Settings.InvalidCssClass != """":b-class=""{'[[Settings.InvalidCssClass]]':[FIELD].isValidated and ([FIELD].requiredError or [FIELD].patternError)}""]]>
                <b-condition con='Field.Settings.IsDisabledLayout == false and Field.Settings.IsHiddenFieldText == false and Field.FieldText != null'>
                    <label class=""[[Settings.FieldTextCssClass]]"">[[FieldText]]</label>
                </b-condition>

                [FIELD-COMPONENT]

                <b-condition con='Field.CanHaveValue == true and Field.IsRequired == true and Field.FieldValueProperty != null and Field.FieldValueProperty != """"'>
                    <p b-show='[FIELD].isValidated and [FIELD].requiredError and 
                        ([[FieldValueProperty]] === null or [[FieldValueProperty]] === undefined or [[FieldValueProperty]] === """")' 
                        class=""[[Settings.RequiredMessageCssClass]]"" 
                        style=""display:none;"">
                        [[Settings.RequiredMessage]]
                    </p>
                </b-condition>

                <b-condition con='Field.CanHaveValue == true and Field.FieldValueProperty != null and Field.FieldValueProperty != """"
                    and Field.Settings.EnableValidationPattern == true and Field.Settings.ValidationPattern != null and Field.Settings.ValidationPattern != """"'>

                    <p b-show=""[FIELD].isValidated and [[FieldValueProperty]] and ![FIELD].isValid and [FIELD].patternError"" 
                        class=""[[Settings.ValidationMessageCssClass]]"" 
                        style=""display:none;"">
                        [[Settings.ValidationMessage]]
                    </p>
                </b-condition>

                <b-condition con='Field.Settings.Subtext != null'>
                    <span class=""[[Settings.SubtextCssClass]]"">[[Settings.Subtext]]</span>
                </b-condition>
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

		public async Task<string> BuildLayoutAsync(IEngineContext context, ModuleDto module, Action<string, double> progress)
		{
			_module = module;
			_context = context;
			_onProgress = progress;
			_fieldIndex = 0;
			_progressStep = 85 / module.Fields.Count();
			_panes = new Dictionary<string, PaneDefinition>();

			await LoadTemplates(_module.Fields);

			InitializePanes();

			_onProgress.Invoke($"Loaded resource content of {_module.ModuleName} module", 15);

			_fieldMap = _module.Fields
				.GroupBy(f => f.ParentId ?? Guid.Empty)
				.ToDictionary(g => g.Key, g => g.OrderBy(f => f.ViewOrder).ToList());

			_fieldMap.TryGetValue(Guid.Empty, out var roots);
			roots = roots ?? new List<ModuleFieldDto>();

			await ProcessFieldTree(roots);

			InjectAllPanes(); // Final inject، layout + runtime

			if (!string.IsNullOrWhiteSpace(_module.ThemeCssClass))
				_layoutTemplate = _layoutTemplate.Replace("[THEME_CSS_CLASS]", _module.ThemeCssClass);

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

				// ←  Here is best place for set Injector 
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

		private async Task ProcessFieldTree(IEnumerable<ModuleFieldDto> fields)
		{
			foreach (var field in fields)
			{
				if (_fieldMap.TryGetValue(field.Id, out var children) && children.Any())
					field.IsParent = true;

				var pane = GetPaneBuffer(field.PaneName);
				var html = await ParseFieldTemplate(field);
				pane.AppendLine(html);

				if (field.IsParent)
					await ProcessFieldTree(children);
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
			if (!_context.TryGet<string>("OutputDirectory", out var path))
				throw new KeyNotFoundException("'OutputDirectory' key was not found in the context.");
			_layoutTemplate = await FileUtil.GetFileContentAsync($@"{path}\_layout.html");

			await BatchExecutor.ExecuteInBatchesAsync(fields, 5, async batch =>
			{
				var items = batch.GroupBy(f => f.TemplatePath?.ReplaceFrequentTokens())
								 .ToDictionary(g => g.Key, g => g.First());

				await FileUtil.LoadFilesAsync(items.Keys, GlobalHelper.MapPath, (key, content) =>
				{
					if (items.TryGetValue(key, out var field))
						_fieldTypes[(field.FieldType, field.Template)] = content;
				}, true);
			});
		}

		private async Task<string> ParseFieldTemplate(ModuleFieldDto field)
		{
			_fieldIndex++;

			var key = (field.FieldType, field.Template);
			_fieldTypes.TryGetValue(key, out var template);

			if (field.Settings != null && field.Settings.TryGetValue("Content", out var content) && content != null)
			{
				template = $@"
                    <div [TOKENS] >
                        {content}
                    </div>
                ";

                if(!string.IsNullOrEmpty(field.HiddenConditions))
                    template = template.Replace("[TOKENS]", @"[[IF:Field.HiddenConditions != null and Field.HiddenConditions != """":b-if='!([[HiddenConditions]])']] [TOKENS]");

                field.IsContent = true;
			}

			if (string.IsNullOrEmpty(template))
				return string.Empty;

			if (!field.IsContent)
			{
				if (field.IsParent && field.IsGroupField)
				{
					string panesHtml = string.Empty;
					if (!string.IsNullOrEmpty(field.FieldTypeGeneratePanesBusinessControllerClass))
					{
						var type = await _moduleFieldService.GetGeneratePanesBusinessControllerClassAsync(field.FieldType);
						if (!string.IsNullOrEmpty(type))
						{
							var controller = _serviceLocator.GetInstance<IFieldTypePaneGeneration>(type);
							panesHtml = await controller.GeneratePanes(field);
						}
					}
					else panesHtml = template;

					panesHtml = panesHtml
									.Replace("[FIELD]", $"field.{field.FieldName}")
									.Replace("[FIELD_ID]", field.Id.ToString())
									.Replace("[FIELD_NAME]", field.FieldName);

					// Register runtime panes
					RegisterPanesFromHtml(panesHtml);

					template = template.Replace("[FIELDPANES]", panesHtml ?? string.Empty);
				}

				if (field.Settings.TryGetValue("IsDisabledLayout", out var isDisabledLayout) && (bool)isDisabledLayout)
				{
					if (!string.IsNullOrEmpty(field.HiddenConditions))
						template = template.Replace("[TOKENS]", @"[[IF:Field.HiddenConditions != null and Field.HiddenConditions != """":b-if='!([[HiddenConditions]])']] [TOKENS]");

					if (field.CanHaveValue && field.IsRequired && !string.IsNullOrEmpty(field.FieldValueProperty))
						template += @"
                            <b-condition con='Field.CanHaveValue == true and Field.IsRequired == true and Field.FieldValueProperty != null and Field.FieldValueProperty != """"'>
                                <p b-show='[FIELD].isValidated and [FIELD].requiredError and 
                                    ([[FieldValueProperty]] === null or [[FieldValueProperty]] === undefined or [[FieldValueProperty]] === """")' 
                                    class=""[[Settings.RequiredMessageCssClass]]"" 
                                    style=""display:none;"">
                                    [[Settings.RequiredMessage]]
                                </p>
                            </b-condition>";

					if (field.CanHaveValue && field.Settings.TryGetValue("EnableValidationPattern", out var enableValidationPattern) && (bool)enableValidationPattern)
						template += @"
                            <b-condition con='Field.CanHaveValue == true and Field.Settings.EnableValidationPattern == true and Field.Settings.ValidationPattern != null and Field.Settings.ValidationPattern != """"'>
                                <p b-show=""[FIELD].isValidated and ![FIELD].isValid and [FIELD].patternError"" 
                                    class=""[[Settings.ValidationMessageCssClass]]"" 
                                    style=""display:none;"">
                                    [[Settings.ValidationMessage]]
                                </p>
                            </b-condition>";
				}
				else
				{
					template = _fieldLayout.Replace("[FIELD-COMPONENT]", template);
				}
			}

			if (field.Settings.TryGetValue("CustomStyles", out var customStyles) && !string.IsNullOrWhiteSpace(customStyles?.ToString()))
				template = template.Replace("[TOKENS]", $@" style=""{customStyles.ToString()}""[TOKENS]");

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
					throw ex;
				}

				template = template.Replace(match.Value, value ?? "");
			}

			template = template
						.Replace("[FIELD]", $"field.{field.FieldName}")
						.Replace("[FIELD_ID]", $"{field.Id}")
						.Replace("[FIELD_NAME]", $"{field.FieldName}");

			//--------------------- Attribute Condition Expression Proccess --------------------------
			matches = Regex.Matches(template, _ifPattern, RegexOptions.Singleline);
			foreach (Match match in matches)
			{
				var condition = match.Groups["Condition"].Value;
				var conditionDsl = $@"
                if {condition}
                begin
                    _IsTrue = true
                end";

				var dic = new ConcurrentDictionary<string, object>();
				dic.TryAdd("Field", field);
				dic.TryAdd("_IsTrue", false);

				var tokenizer = new Tokenizer(conditionDsl);
				List<Token> tokens = tokenizer.Tokenize();

				var parser = new DslParser(tokens);
				DslScript script = parser.ParseScript();

				var dslContext = new ExpressionContext(dic);
				var compiler = new ExpressionCompiler();
				var executor = new DslExecutor(compiler);
				executor.Execute(script, dslContext);

				dic.TryGetValue("_IsTrue", out var isTrue);
				template = template.Replace(match.Value, (bool)isTrue == true
					? (match.Groups["Exp"].Value ?? "")
					: string.Empty);
			}

			//--------------------- Element Condition Expression Proccess --------------------------
			matches = Regex.Matches(template, _bConditionPattern, RegexOptions.Compiled);
			foreach (Match match in matches)
			{
				var condition = match.Groups[1].Value;
				var conditionDsl = $@"
                if {condition}
                begin
                    _IsTrue = true
                end";

				var dic = new ConcurrentDictionary<string, object>();
				dic.TryAdd("Field", field);
				dic.TryAdd("_IsTrue", false);

				var tokenizer = new Tokenizer(conditionDsl);
				List<Token> tokens = tokenizer.Tokenize();

				var parser = new DslParser(tokens);
				DslScript script = parser.ParseScript();

				var dslContext = new ExpressionContext(dic);
				var compiler = new ExpressionCompiler();
				var executor = new DslExecutor(compiler);
				executor.Execute(script, dslContext);

				dic.TryGetValue("_IsTrue", out var isTrue);
				template = template.Replace(match.Value, (bool)isTrue == true
					? (match.Groups[2].Value ?? "")
					: string.Empty);
			}

			_onProgress.Invoke($"Render template {field.FieldType} of {_module.ModuleName} module", 15 + (_fieldIndex * _progressStep));

			return template.Replace("[TOKENS]", $@"__b=""{field.Id}""");
		}

		#endregion
	}
}
