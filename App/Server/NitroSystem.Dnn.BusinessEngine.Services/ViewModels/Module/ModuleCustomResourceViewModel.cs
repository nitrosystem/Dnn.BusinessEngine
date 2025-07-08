using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels
{
    public class ModuleCustomResourceViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid LibraryId { get; set; }
        public string AddressType { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}