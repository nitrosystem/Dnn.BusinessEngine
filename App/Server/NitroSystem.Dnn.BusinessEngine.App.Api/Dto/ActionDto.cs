using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.App.Api.Dto
{
    public class ActionDto
    {
        public Guid FieldId { get; set; }
        public Guid? ActionId { get; set; }
        public string Event { get; set; }
        public string ConnectionId { get; set; }
        public Guid ModuleId { get; set; }
        public string PageUrl { get; set; }
        public Dictionary<string,object> Data { get; set; }
        public Dictionary<string,object> ExtraParams { get; set; }
    }
}
