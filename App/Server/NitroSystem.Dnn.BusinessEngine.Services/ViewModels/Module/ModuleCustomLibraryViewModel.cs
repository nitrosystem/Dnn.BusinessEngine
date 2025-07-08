using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels
{
    public class ModuleCustomLibraryViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid LibraryId { get; set; }
        public string LibraryName { get; set; }
        public string LocalPath { get; set; }
        public string Version { get; set; }
        public string Logo { get; set; }
        public int LoadOrder { get; set; }
        public IEnumerable<ModuleCustomLibraryResourceViewModel> Resources { get; set; }
    }
}