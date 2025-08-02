using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels
{
  public  class ActionTypeViewModel
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public string ActionType { get; set; }
        public string Title { get; set; }
        public string ActionComponent { get; set; }
        public string ComponentSubParams { get; set; }
        public int ExecutionScope { get; set; }
        public bool HasResults { get; set; }
        public string BusinessControllerClass { get; set; }
        public string ActionJsPath { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int GroupViewOrder { get; set; }
        public int ViewOrder { get; set; }
    }
}
