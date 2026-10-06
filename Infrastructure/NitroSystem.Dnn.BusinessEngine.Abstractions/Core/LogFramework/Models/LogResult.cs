using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models
{
    public class LogResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }

        private LogResult(bool success, string message)
        {
            IsSuccess = success;
            Message = message;
        }

        public static LogResult Success(string msg = "Success") => new LogResult(true, msg);
        public static LogResult Failure(string msg) => new LogResult(false, msg);
    }
}
