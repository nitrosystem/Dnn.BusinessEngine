using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels
{
    public class DefinedListItemViewModel
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