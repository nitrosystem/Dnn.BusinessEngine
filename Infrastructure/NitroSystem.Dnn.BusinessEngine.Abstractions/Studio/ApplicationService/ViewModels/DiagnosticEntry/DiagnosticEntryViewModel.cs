using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.DiagnosticEntry
{
   public class DiagnosticEntryViewModel
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? ModuleId { get; set; }
        public Guid? EntryId { get; set; }
        public int EntryType { get; set; }
        public int DiagnosticType { get; set; }
        public string DiagnosticEntry { get; set; }
    }
}
