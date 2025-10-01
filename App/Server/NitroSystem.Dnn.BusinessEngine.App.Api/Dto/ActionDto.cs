using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.App.Api.Dto
{
    public class ActionDto
    {
        public string ConnectionId { get; set; }
        public Guid ModuleId { get; set; }
        public string PageUrl { get; set; }
        public Dictionary<string,object> Data { get; set; }
        public IEnumerable<Guid> ActionIds { get; set; }
    }
}
