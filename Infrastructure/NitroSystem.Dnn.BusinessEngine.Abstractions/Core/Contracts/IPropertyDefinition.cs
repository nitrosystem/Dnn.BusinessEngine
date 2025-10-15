using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts
{
    public interface IPropertyDefinition
    {
        public string Name { get; set; }
        public string ClrType { get; set; }
    }
}
