using DotNetNuke.Entities.Modules.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Contracts
{
    public  class TokenBase
    {
        public string TokenName { get; set; }
        public byte TokenType { get; set; }
        public object TokenValue { get; set; }
     
        protected event EventHandler<EventArgs> Callback;
    }
}
