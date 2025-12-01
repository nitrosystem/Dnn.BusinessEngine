using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    public class EngineResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public List<string> Errors { get; } = new List<string>();

        public static EngineResult<T> Success(T data) => new EngineResult<T> { IsSuccess = true, Data = data };
        public static EngineResult<T> Failure(params string[] errors)
        {
            var r = new EngineResult<T> { IsSuccess = false };
            r.Errors.AddRange(errors);
            return r;
        }

        public void AddError(string err)
        {
            if (!string.IsNullOrEmpty(err)) Errors.Add(err);
            IsSuccess = false;
        }
    }
}
