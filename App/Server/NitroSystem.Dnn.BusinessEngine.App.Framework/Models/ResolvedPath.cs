using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Framework.Models
{
    public class ResolvedPath
    {
        public string Path { get; set; } 
        public object Parent { get; set; }
        public PropertyInfo Property { get; set; }
        public int? Index { get; set; }
        public object Value { get; set; }
    }
}
