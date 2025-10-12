using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Base
{
    public class ExplorerItemViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public Guid? GroupId { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? ParentId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int ViewOrder { get; set; }
    }
}