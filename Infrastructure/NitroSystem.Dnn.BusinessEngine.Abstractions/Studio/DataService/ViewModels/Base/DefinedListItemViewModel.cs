using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Base
{
    public class DefinedListItemViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ListId { get; set; }
        public string ParentId { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public object Data { get; set; }
        public int ItemLevel { get; set; }
        public int ViewOrder { get; set; }
        public List<DefinedListItemViewModel> Items { get; set; }
    }
}