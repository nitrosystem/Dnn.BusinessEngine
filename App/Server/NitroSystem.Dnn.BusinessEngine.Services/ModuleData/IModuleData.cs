using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ModuleData
{
    public interface IModuleData
    {
        DateTime LastPing { get; set; }

        IReadOnlyDictionary<string, object> GetAll();
        object Get(string key);
        bool TryGet(string key, out object data);
        void Set(string key, object value);
        void UpdateObjects(Dictionary<string, object> data);
        void SetPageParams(string pageUrl);
    }
}
