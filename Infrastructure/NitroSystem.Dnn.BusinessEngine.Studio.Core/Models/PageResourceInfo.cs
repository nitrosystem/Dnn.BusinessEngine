using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Core.Models
{
    public class PageResourceInfo 
    {
        public bool IsSystemResource { get; set; }
        public bool IsCustomResource { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public bool IsActive { get; set; }
        public int LoadOrder { get; set; }
    }
}