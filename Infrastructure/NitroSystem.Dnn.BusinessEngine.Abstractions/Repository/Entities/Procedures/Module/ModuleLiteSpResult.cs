using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module
{
    public class ModuleLiteSpResult
    {
        public Guid Id { get; set; }
        public string ScenarioName { get; set; }
        public string ModuleName { get; set; }
        public bool IsSSR { get; set; }
    }
}
