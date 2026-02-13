using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Components;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export.Enums;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static System.Net.WebRequestMethods;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import
{
    public class ImportFramework
    {
        private readonly List<IComponent> _components = new List<IComponent>();
        private readonly IDataContext _ctx;
        private readonly HttpContext _httpContext;
        private readonly string _zipFilePath;
        private readonly string _manifestFolderPath;

        private static Thread worker;
        private static ManifestModel _manifestModel;

        public ImportFramework(string zipFilePath, string manifestFolderPath, IDataContext ctx, HttpContext httpContext)
        {
            _zipFilePath = zipFilePath;
            _ctx = ctx;
            _httpContext = httpContext;
            _manifestFolderPath = manifestFolderPath;
        }
        
        Func<string, string> openFileFunc = FileUtil.GetFileContent;
        Action<string, string> unzipAction = ZipUtil.UnZip;

        public delegate void Worker(Action<int> reportProgress);

        public void Init(Worker work, Action<int> reportProgress)
        {
            _ctx.BeginTransaction();

            try
            {
                unzipAction(_zipFilePath, _manifestFolderPath);

                var json = openFileFunc(_manifestFolderPath + @"_manifest.json");
                _manifestModel = JsonConvert.DeserializeObject<ManifestModel>(json);

                worker = new Thread(() => work(reportProgress));
                worker.IsBackground = true;
                worker.Start();
            }
            catch (Exception ex)
            {
                FailedImport(true, ex);
            }
        }

        public void RegisterComponent(IComponent component)
        {
            _components.Add(component);
        }

        public void CreateWorkflow()
        {
            foreach (var component in _components)
            {
                component.CreateTasks();
            }
        }

        public void Work(Action<int> reportProgress)
        {
            int inedx = 0;
            foreach (var component in _components)
            {
                using (var cmp = component)
                {
                    try
                    {
                        cmp.Import(_manifestModel, _ctx, _httpContext);
                    }
                    catch (Exception ex)
                    {
                        if (cmp.ContinueOnError) continue;
                        else FailedImport(true, ex);
                    }

                    reportProgress(++inedx * 10);
                }
            }

            this.CompleteImport();
        }

        public void ProgressChanged(int index)
        {
            Console.WriteLine(index);
        }

        private void FailedImport(bool raiseError, Exception ex)
        {
            _ctx.RollbackTransaction();

            if (raiseError) throw ex;
        }

        private void CompleteImport()
        {
            _ctx.Commit();

            //File.Delete(_zipFilePath);
            //Directory.Delete(_manifestFolderPath, true);
        }
    }
}
