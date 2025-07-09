using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Common;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Common.IO;

namespace NitroSystem.Dnn.BusinessEngine.Core.Common
{
    public class ProgressMonitoring
    {
        private readonly string _monitoringFile;
        private readonly string _progressFile;

        public ProgressMonitoring(string monitoringFile, string progressFile, string startMessage, int progressValue = 0)
        {
            _monitoringFile = monitoringFile;
            _progressFile = progressFile;

            var path = Path.GetDirectoryName(HttpContext.Current.Server.MapPath(monitoringFile));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            FileUtil.CreateTextFile(HttpContext.Current.Server.MapPath(monitoringFile), startMessage, true);
            FileUtil.CreateTextFile(HttpContext.Current.Server.MapPath(progressFile), progressValue.ToString(), true);
        }

        public void Progress(string message, byte value)
        {
            FileUtil.CreateTextFile(HttpContext.Current.Server.MapPath(_monitoringFile), message, true);
            FileUtil.CreateTextFile(HttpContext.Current.Server.MapPath(_progressFile), value.ToString(), true);
        }

        public void Progress(string message, float value)
        {
            FileUtil.CreateTextFile(HttpContext.Current.Server.MapPath(_monitoringFile), message, true);
            FileUtil.CreateTextFile(HttpContext.Current.Server.MapPath(_progressFile), value.ToString(), true);
        }

        public void Progress(string message, string value, HttpContext context)
        {
            FileUtil.CreateTextFile(context.Server.MapPath(_monitoringFile), message, true);
            FileUtil.CreateTextFile(context.Server.MapPath(_progressFile), value, true);
        }

        public void End()
        {
            FileUtil.CreateTextFile(HttpContext.Current.Server.MapPath(_monitoringFile), "end.", true);
            FileUtil.CreateTextFile(HttpContext.Current.Server.MapPath(_progressFile), "b---end", true);
        }
    }
}
