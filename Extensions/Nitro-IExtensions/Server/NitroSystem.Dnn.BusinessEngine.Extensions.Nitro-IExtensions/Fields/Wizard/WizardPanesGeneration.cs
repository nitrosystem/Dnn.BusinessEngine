using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.Nitro_IExtensions.Fields.Wizard
{
    public class WizardPanesGeneration : IFieldTypePaneGeneration
    {
        public async Task<string> GeneratePanes(ModuleFieldDto field)
        {
            await Task.Yield();

            var result = new StringBuilder();

            var steps = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<StepInfo>>(field.Settings["Steps"]?.ToString());
            foreach (var step in steps)
            {
                var pane = $"<div id=\"WizardStep_{step.Name}\" data-pane=\"WizardStep_{step.Name}\" data-pane-title=\"{step.Name} Pane\" data-parent-id=\"{field.Id}\" b-show=\"[FIELD].currentStep=='{step.Name}'\" class=\"{step.PaneCssClass}\" b-class=\"{{'active':[FIELD].currentStep=='{step.Name}'}}\"></div>";

                result.AppendLine(pane);
            }

            return result.ToString();
        }
    }
}
