using NitroSystem.Dnn.BusinessEngine.App.Services.ModuleData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Mvc.Models
{
	public class ModuleView
	{
        public string ConnectionId { get; set; }
        public Guid Id { get; set; }
        public ModuleData Data { get; set; }
        public string Template { get; set; }
        public string RtlCssClass { get; set; }
    }
}