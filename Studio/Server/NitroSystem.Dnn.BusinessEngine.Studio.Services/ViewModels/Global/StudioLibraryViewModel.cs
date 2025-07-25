using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    public class StudioLibraryViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string LibraryName { get; set; }
        public string Version { get; set; }
        public int LoadOrder { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
    }
}