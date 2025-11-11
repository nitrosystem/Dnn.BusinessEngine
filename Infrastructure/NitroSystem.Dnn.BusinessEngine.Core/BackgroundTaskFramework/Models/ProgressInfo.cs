using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models
{
    public class ProgressInfo
    {
        public string TaskId { get; set; }
        public int Percent { get; set; }
        public string Message { get; set; }
    }
}
