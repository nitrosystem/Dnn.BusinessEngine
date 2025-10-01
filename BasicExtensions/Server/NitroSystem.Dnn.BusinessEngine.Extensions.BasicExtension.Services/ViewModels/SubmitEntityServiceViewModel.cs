using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database.SubmitEntity;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database
{
    public class SubmitEntityServiceViewModel: IExtensionServiceViewModel
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid EntityId { get; set; }
        public string BaseQuery { get; set; }
        public string InsertBaseQuery { get; set; }
        public string UpdateBaseQuery { get; set; }
        public string StoredProcedureName { get; set; }
        public ActionType ActionType { get; set; }
        public EntityInfo Entity { get; set; }
        public string CustomQuery{ get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}