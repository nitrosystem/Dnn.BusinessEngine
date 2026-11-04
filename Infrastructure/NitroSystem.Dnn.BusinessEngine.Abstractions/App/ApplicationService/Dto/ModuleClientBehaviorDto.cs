using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto
{
    public class ModuleClientBehaviorDto
    {
        public Guid Id { get; set; }
        public Guid? ActionId { get; set; }
        public string Event { get; set; }
        public string Script { get; set; }
    }
}
