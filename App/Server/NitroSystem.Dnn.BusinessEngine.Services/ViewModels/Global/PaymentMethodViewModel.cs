using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels
{
    public class PaymentMethodViewModel
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public int PortalId { get; set; }
        public string PaymentMethodName { get; set; }
        public string PaymentDescription { get; set; }
        public string ReturnUrl { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}
