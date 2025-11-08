using DotNetNuke.ComponentModel;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Import;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Tasks;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export.Components
{
    public class PageResourceComponent : ComponentBase
    {
        private readonly object[] _params;
        public PageResourceComponent(ManifestModel manifestModel, params object[] args)
        {
            this.Name = Enums.ComponentType.Entity;
            this.ManifestModel = manifestModel;
            this._params = args;
        }

        public override void CreateTasks()
        {
            var tasks = new Queue<ICustomTask>();

            var task1 = new GetModelsAsJsonTask<string>("NitroSystem.Dnn.BusinessEngine.Api.Mapping.EntityMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetEntitiesViewModel", this.ManifestPath, "entities.json", _params);
            tasks.Enqueue(task1);

            this.Tasks = tasks;
        }

        public override async Task<bool> Export()
        {
            return await base.Export();
        }
    }
}