namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Security
{
    using System;

    /// <summary>
    /// Defines the <see cref="SafeSqlExecutor" />
    /// </summary>
    public abstract class SafeSqlExecutor
    {
        /// <summary>
        /// Defines the _blacklistPatterns
        /// </summary>
        private static readonly string[] _blacklistPatterns =
        {
            "--", ";--", ";", "/*", "*/", "@@",
            "CHAR(", "NCHAR(", "VARCHAR(", "NVARCHAR(",
            "ALTER ", "DROP ", "INSERT ", "DELETE ", "UPDATE ", "EXEC "
        };

        /// <summary>
        /// The IsQuerySafe
        /// </summary>
        /// <param name="query">The query<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        protected bool IsQuerySafe(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return false;

            string upperQuery = query.ToUpperInvariant();

            foreach (var pattern in _blacklistPatterns)
            {
                if (upperQuery.Contains(pattern))
                {
                    Console.WriteLine($"⚠️ SQL Injection Detected: {pattern}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The ValidateQuery
        /// </summary>
        /// <param name="query">The query<see cref="string"/></param>
        protected void ValidateQuery(string query)
        {
            if (!IsQuerySafe(query))
            {
                throw new InvalidOperationException("🚨 Potential SQL Injection detected! Query execution blocked.");
            }
        }
    }

}
