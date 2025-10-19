using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models
{
    public class LogResult
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }

        public static LogResult Success(string msg = null) => new LogResult { IsSuccess = true, Message = msg };
        public static LogResult Failure(string msg) => new LogResult { IsSuccess = false, Message = msg };
    }
}
