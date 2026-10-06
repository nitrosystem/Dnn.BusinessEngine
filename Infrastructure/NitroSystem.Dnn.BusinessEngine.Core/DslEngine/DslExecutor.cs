using System;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Statements;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine
{
    public sealed class DslExecutor
    {
        private readonly ExpressionCompiler _compiler;
        private readonly MemberAccessResolver _resolver;

        public DslExecutor(ExpressionCompiler compiler)
        {
            _compiler = compiler;
            _resolver = new MemberAccessResolver();
        }

        public void Execute(DslScript script, IExpressionContext context)
        {
            foreach (var stmt in script.Statements)
                ExecuteStatement(stmt, context);
        }

        private void ExecuteStatement(DslStatement stmt, IExpressionContext ctx)
        {
            if (stmt is IfStatement)
            {
                ExecuteIf((IfStatement)stmt, ctx);
                return;
            }

            if (stmt is AssignmentStatement)
            {
                ExecuteAssignment((AssignmentStatement)stmt, ctx);
                return;
            }

            throw new NotSupportedException();
        }

        private void ExecuteIf(IfStatement stmt, IExpressionContext ctx)
        {
            var cond = _compiler.Compile(stmt.Condition, ctx);
            var result = (bool)cond(ctx);

            var list = result ? stmt.Then : stmt.Else;
            if (list == null) return;

            foreach (var s in list)
                ExecuteStatement(s, ctx);
        }

        private void ExecuteAssignment(AssignmentStatement stmt, IExpressionContext ctx)
        {
            var valueFunc = _compiler.Compile(stmt.Value, ctx);
            var value = valueFunc(ctx);

            var path = stmt.Target.Path;

            // Assignment to root
            if (path.Count == 1)
            {
                ctx.SetRoot(path[0], value);
                return;
            }

            // Assignment to member
            var root = ctx.GetRoot(path[0]);

            for (int i = 1; i < path.Count - 1; i++)
            {
                root = _resolver.GetValue(root, path[i]);
            }

            _resolver.SetValue(root, path[path.Count - 1], value);
        }
    }
}
