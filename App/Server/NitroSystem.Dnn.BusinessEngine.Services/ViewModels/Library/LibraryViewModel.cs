using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels
{
    public class LibraryViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public string Type { get; set; }
        public string LibraryName { get; set; }
        public string Logo { get; set; }
        public string Summary { get; set; }
        public string Version { get; set; }
        public string LocalPath { get; set; }
        public bool IsSystemLibrary { get; set; }
        public bool IsCDN { get; set; }
        public bool IsCommercial { get; set; }
        public bool IsOpenSource { get; set; }
        public bool IsStable { get; set; }
        public string LicenseJson { get; set; }
        public string GithubPage { get; set; }
        public IEnumerable<LibraryResourceViewModel> Resources { get; set; }
        public DateTime LastModifiedOnDate { get ; set ; }
        public int LastModifiedByUserId { get ; set ; }
        public DateTime CreatedOnDate { get ; set ; }
        public int CreatedByUserId { get ; set ; }
    }
}