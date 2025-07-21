using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Module.Field
{
    public class ModuleFieldSettingViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid FieldId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
    }
}
