﻿using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.AppModel
{
    public class AppModelViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? GroupId { get; set; }
        public string ModelName { get; set; }
        public string TypeFullName { get; set; }
        public string TypeRelativePath { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<AppModelPropertyViewModel> Properties { get; set; }
    }
}