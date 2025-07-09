using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Security.Models
{
    public class EncryptedModel
    {
        public string EncryptedKey { get; set; }
        public string EncryptedData { get; set; }
        public string IV { get; set; }
    }
}
