using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Base
{
    public class DefinedListViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid? ScenarioId { get; set; }
        public string ListName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<DefinedListItemViewModel> Items { get; set; }
    }
}