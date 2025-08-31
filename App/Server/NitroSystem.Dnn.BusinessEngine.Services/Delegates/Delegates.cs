using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Delegates
{
    public class Delegates
    {
        public delegate Dictionary<string, object> ModuleMergeHandler(ConcurrentDictionary<string, object> existingModule, Dictionary<string, object> incomingData);
    }
}
