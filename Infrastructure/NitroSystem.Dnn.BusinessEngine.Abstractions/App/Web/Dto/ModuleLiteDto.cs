using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto
{
	public class ModuleLiteDto
	{
        public Guid Id { get; set; }
        public string ScenarioName { get; set; }
        public string ModuleName { get; set; }
        public bool IsSSR { get; set; }
    }
}