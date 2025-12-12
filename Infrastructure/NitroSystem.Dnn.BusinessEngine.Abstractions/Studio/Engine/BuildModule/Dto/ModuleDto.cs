using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto
{
	public class ModuleDto
	{
		public Guid Id { get; set; }
		public Guid? ParentId { get; set; }
		public int? SitePageId { get; set; }
		public string ScenarioName { get; set; }
		public string ParentModuleName { get; set; }
		public string ModuleName { get; set; }
		public string PreloadingTemplate { get; set; }
		public string LayoutTemplate { get; set; }
		public ModuleWrapper Wrapper { get; set; }
		public IEnumerable<ModuleFieldDto> Fields { get; set; }
		public IEnumerable<ModuleResourceDto> Resources { get; set; }
		public IEnumerable<ModuleResourceDto> ExternalResources { get; set; }
	}
}
