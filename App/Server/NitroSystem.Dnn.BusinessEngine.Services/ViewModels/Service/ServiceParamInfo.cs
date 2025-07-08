using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels
{
    public class ServiceParamViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string ParamName { get; set; }
        public string ParamType { get; set; }
        public object ParamValue { get; set; }
        public string DefaultValue { get; set; }
        public string ExpressionParsingType { get; set; }
        public bool IsCustomParam { get; set; }
        public int ViewOrder { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
    }
}
