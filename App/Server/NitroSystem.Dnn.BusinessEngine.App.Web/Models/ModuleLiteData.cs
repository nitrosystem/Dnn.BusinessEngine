using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Models
{
	public class ModuleLiteData
	{
        public Guid? Id { get; set; }
        public string ScenarioName { get; set; }
        public string ModuleName { get; set; }
        public int? ModuleVersion { get; set; }
    }
}