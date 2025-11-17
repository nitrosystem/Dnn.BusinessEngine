using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Workflow
{
    public sealed class ResourceProfiler
    {
        private readonly Process _process;

        public ResourceProfiler()
        {
            _process = Process.GetCurrentProcess();
        }

        public async Task<MethodProfileResult<T>> ProfileAsync<T>(LambdaExpression expression, string name, bool isTask, bool isVoid)
        {
            var sw = Stopwatch.StartNew();
            var cpuStart = _process.TotalProcessorTime;
            var memStart = GC.GetTotalMemory(false);

            var data = new MethodProfileResult<T>()
            {
                Name = name,
                StartTime = DateTime.Now
            };

            T result = default;

            try
            {
                // تبدیل Expression به LambdaExpression قابل کامپایل
                var lambda = Expression.Lambda(expression.Body, expression.Parameters);
                var del = lambda.Compile(); // -> Delegate

                if (isTask && isVoid)
                {
                    // Func<Task>
                    dynamic task = del.DynamicInvoke();
                    await task;
                }
                else if (isTask && !isVoid)
                {
                    // Func<Task<T>>
                    dynamic task = del.DynamicInvoke();
                    result = await task;
                }
                if (!isTask && isVoid)
                {
                    var action = (Action)expression.Compile();
                    action();
                }
                else if (!isTask && !isVoid)
                {
                    var func = (Func<T>)expression.Compile();
                    result = func();
                }

                data.EndTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                data.IsError = true;
                data.Exception = ex;
            }

            var cpuEnd = _process.TotalProcessorTime;
            var memEnd = GC.GetTotalMemory(false);
            sw.Stop();

            data.ResourceUsage = new TaskResourceUsage
            {
                ElapsedMs = sw.ElapsedMilliseconds,
                CpuMs = (cpuEnd - cpuStart).TotalMilliseconds,
                MemoryDeltaBytes = memEnd - memStart
            };

            data.Result = result;

            return data;
        }

    }
}
