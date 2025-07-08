using DotNetNuke.Entities.Portals;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundService
{
    internal class BackgroundService
    {
    }
}

//using DotNetNuke.Entities.Portals;
//using Newtonsoft.Json;
//using NitroSystem.Dnn.BusinessEngine.Common.IO;
//using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import;
//using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export.Enums;
//using NitroSystem.Dnn.BusinessEngine.Utilities;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export
//{
//    public class FrameworkManager
//    {
//        private readonly List<IComponent> _components = new List<IComponent>();
//        private readonly ManifestModel _manifestModel;
//        private readonly string _manifestPath;
//        private readonly BackgroundWorker _worker;

//        Func<string, string, bool, bool> createFile = FileUtil.CreateTextFile;
//        Func<string, string, bool, string> zipFunc = ZipUtil.Zip;
//        Action<string, string> unzipAction = ZipUtil.UnZip;
//        public delegate void Worker(Action<int> reportProgress);
//        private Thread worker;


//        public FrameworkManager(string manifestPath, ManifestModel manifestModel, PortalSettings portalSettings, OperationType operationType)
//        {
//            _manifestPath = manifestPath;
//            _manifestModel = manifestModel;

//            this.InitializeProcess();

//            //_worker = new BackgroundWorker();
//            //_worker.DoWork += Worker_DoWork;
//            //_worker.WorkerReportsProgress = true;
//            //_worker.ProgressChanged += _worker_ProgressChanged;
//            //_worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
//        }

//        public void InitializeProcess()
//        {
//            if (Directory.Exists(_manifestPath)) Directory.Delete(_manifestPath, true);

//            Directory.CreateDirectory(_manifestPath);
//        }

//        public void RegisterComponent(IComponent component)
//        {
//            _components.Add(component);
//        }

//        public void CreateWorkflow()
//        {
//            foreach (var component in _components)
//            {
//                component.CreateTasks();
//            }
//        }

//        public void StartExport()
//        {
//            foreach (var component in _components)
//            {
//                using (var cmp = component)
//                {
//                    cmp.Export();
//                    cmp.CompleteComponentExport(_manifestModel);
//                }
//            }
//        }

//        public void StartImport()
//        {
//            if (!_worker.IsBusy)
//            {
//                _worker.RunWorkerAsync();
//            }
//        }

//        public void FinalizeExport()
//        {
//            string json = JsonConvert.SerializeObject(_manifestModel);
//            string filename = _manifestPath + "_manifest.json";
//            this.createFile(filename, json, true);

//            string targetPath = Path.GetDirectoryName(_manifestPath);
//            string result = this.zipFunc(targetPath + ".zip", targetPath, true);

//            Directory.Delete(_manifestPath, true);
//        }

//        private void Worker_DoWork(object sender, DoWorkEventArgs e)
//        {
//            int inedx = 0;
//            foreach (var component in _components)
//            {
//                using (var cmp = component)
//                {
//                    cmp.Export();
//                    cmp.CompleteComponentExport(_manifestModel);
//                }

//                _worker.ReportProgress(++inedx * 10);

//            }
//        }

//        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
//        {
//            var percent = e.ProgressPercentage;
//        }

//        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
//        {
//            this.FinalizeExport();
//        }
//    }
//}
