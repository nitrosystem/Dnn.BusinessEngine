namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Contracts
{
    public interface IExpressionService
    {
        object Evaluate(string expression, IExpressionContext context);
        T Evaluate<T>(string expression, IExpressionContext context);
    }
}
