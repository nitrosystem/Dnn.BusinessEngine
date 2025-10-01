using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Service
{
    public class ServiceViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? GroupId { get; set; }
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
        public string ServiceTypeTitle { get; set; }
        public string ServiceComponent { get; set; }
        public string ServiceTypeIcon { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<ParamInfo> Params { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}