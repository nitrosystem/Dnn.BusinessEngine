using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module
{
    public class ModuleVariableSpResult
    {
        public Guid Id { get; set; }
        public Guid? AppModelId { get; set; }
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public string DefaultValue { get; set; }
        public int Scope { get; set; }
        public string ModelName { get; set; }
        public string ModelTypeRelativePath { get; set; }
        public string ModelTypeFullName { get; set; }
        public string ScenarioName { get; set; }
    }
}
