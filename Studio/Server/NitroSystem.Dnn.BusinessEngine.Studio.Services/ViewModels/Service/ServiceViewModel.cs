using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels
{
    public class ServiceViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? GroupId { get; set; }
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasResult { get; set; }
        public ServiceResultType? ResultType { get; set; }
        public string ServiceTypeTitle { get; set; }
        public string ServiceComponent { get; set; }
        public string ServiceTypeIcon { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<string> AuthorizationRunService { get; set; }
        public IEnumerable<ServiceParamInfo> Params { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}