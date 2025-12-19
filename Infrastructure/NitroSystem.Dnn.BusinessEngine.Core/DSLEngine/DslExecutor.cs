using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Statements;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine
{
    public sealed class DslExecutor
    {
        private readonly IExpressionCompiler _compiler;
        private readonly MemberAccessResolver _resolver;

        public DslExecutor(IExpressionCompiler compiler)
        {
            _compiler = compiler;
            _resolver = new MemberAccessResolver();
        }

        public void Execute(DslScript script, IDslContext context)
        {
            foreach (var stmt in script.Statements)
                ExecuteStatement(stmt, context);
        }

        private void ExecuteStatement(DslStatement stmt, IDslContext ctx)
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

            //برای نسخه بعدی
            //if (stmt is FunctionCallStatement)
            //{
            //    ExecuteFunction((FunctionCallStatement)stmt, ctx);
            //    return;
            //}

            throw new NotSupportedException();
        }

        private void ExecuteIf(IfStatement stmt, IDslContext ctx)
        {
            var cond = _compiler.Compile(stmt.Condition);
            var result = (bool)cond(ctx);

            var list = result ? stmt.Then : stmt.Else;
            if (list == null) return;

            foreach (var s in list)
                ExecuteStatement(s, ctx);
        }

        private void ExecuteAssignment(AssignmentStatement stmt, IDslContext ctx)
        {
            var valueFunc = _compiler.Compile(stmt.Value);
            var value = valueFunc(ctx);

            var path = stmt.Target.Path;

            // ✅ Assignment به root
            if (path.Count == 1)
            {
                ctx.SetRoot(path[0], value);
                return;
            }

            // ✅ Assignment به member
            var root = ctx.GetRoot(path[0]);

            for (int i = 1; i < path.Count - 1; i++)
            {
                root = _resolver.GetValue(root, path[i]);
            }

            _resolver.SetValue(root, path[path.Count - 1], value);
        }

        private void ExecuteAssignment1(AssignmentStatement stmt, IDslContext ctx)
        {
            var valueFunc = _compiler.Compile(stmt.Value);
            var value = valueFunc(ctx);

            var targetPath = stmt.Target.Path;
            var root = ctx.GetRoot(targetPath[0]);

            for (int i = 1; i < targetPath.Count - 1; i++)
            {
                root = _resolver.GetValue(root, targetPath[i]);
            }

            _resolver.SetValue(root, targetPath[targetPath.Count - 1], value);
        }
    }
}
