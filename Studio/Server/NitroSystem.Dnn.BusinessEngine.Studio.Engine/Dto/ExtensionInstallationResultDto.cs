using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto
{
    public class ExtensionInstallationResultDto
    {
        public bool IsInstalled { get; set; }
        public List<string> Logs { get; set; }
    }
}
