using System.Linq.Expressions;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Workflow.Extensions
{
    public static class MethodProfilerExtensions
    {
        public static async Task<MethodProfileResult<T>> ExecuteTasksAsync<T>(
            this ResourceProfiler profiler,
            bool isTask,
            bool isVoid,
            LambdaExpression expression)
        {
            var name = ExtractMethodName(expression);
            var result = await profiler.ProfileAsync<T>(expression, name,isTask, isVoid);

            return result;
        }

        private static string ExtractMethodName(LambdaExpression expression)
        {
            if (expression.Body is MethodCallExpression call)
                return call.Method.Name;
            return "Unknown";
        }
    }
}
