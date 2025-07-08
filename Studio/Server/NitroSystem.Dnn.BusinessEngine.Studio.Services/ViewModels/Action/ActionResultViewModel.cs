using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels
{
    public class ActionResultViewModel
    {
        public Guid Id { get; set; }
        public Guid ActionId { get; set; }
        public string LeftExpression { get; set; }
        public string EvalType { get; set; }
        public string RightExpression { get; set; }
        public string GroupName { get; set; }
        public string ExpressionParsingType { get; set; }
        public IEnumerable<ExpressionInfo> Conditions { get; set; }
    }
}
