using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Base
{
    public class ScenarioViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string ScenarioName { get; set; }
        public string ScenarioTitle { get; set; }
        public string DatabaseObjectPrefix { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
    }
}
