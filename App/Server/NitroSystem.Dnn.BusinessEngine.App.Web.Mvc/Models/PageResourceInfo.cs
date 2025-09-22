using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Mvc.Models
{
    public class PageResourceDto 
    {
        public bool IsSystemResource { get; set; }
        public bool IsCustomResource { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public bool IsActive { get; set; }
        public int LoadOrder { get; set; }
    }
}