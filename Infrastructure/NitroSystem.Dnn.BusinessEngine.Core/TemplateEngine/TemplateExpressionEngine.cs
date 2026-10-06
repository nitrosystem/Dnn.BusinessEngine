using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine
{
    /// <summary>
    /// Shared facade: parses a raw expression string (a member path for #For,
    /// or a full condition for #If) through the DSL tokenizer/parser and compiles
    /// it via the shared ExpressionCompiler. This is what removes the parallel,
    /// template-only evaluator — #For and #If now speak the exact same grammar
    /// as the DSL and ModuleBuilder.
    ///
    /// ASTs are cached by source text so a loop body doesn't re-tokenize/re-parse
    /// on every single iteration; compiled delegates are cached inside
    /// ExpressionCompilerBase itself, keyed by the *same* ExpressionBase instance
    /// (reference identity) — which is exactly why we cache the AST here instead
    /// of re-parsing into a fresh instance each call.
    ///
    /// NOTE (concurrency): ExpressionCompilerBase._cache is currently a plain
    /// Dictionary, not a ConcurrentDictionary. Holding one Compiler instance as a
    /// process-wide static (as done here, for render performance) means concurrent
    /// web requests compiling different expressions at the same time can race on
    /// that dictionary. This is a pre-existing gap in the shared base class, not
    /// something introduced here — see CHANGES.md 'Open items' before this goes
    /// under real concurrent load.
    /// </summary>
    public static class TemplateExpressionEngine
    {
        private static readonly ExpressionCompiler Compiler = new ExpressionCompiler();
        private static readonly ConcurrentDictionary<string, ExpressionBase> AstCache = new();

        public static object Evaluate(string expressionText, TemplateContext templateContext)
        {
            var ast = AstCache.GetOrAdd(expressionText, ParseExpression);
            var ctx = new TemplateExpressionContext(templateContext);
            var compiled = Compiler.Compile(ast, ctx);
            return compiled(ctx);
        }

        private static ExpressionBase ParseExpression(string text)
        {
            var tokens = new Tokenizer(text).Tokenize();
            return new DslParser(tokens).ParseExpressionEntry();
        }
    }
}
