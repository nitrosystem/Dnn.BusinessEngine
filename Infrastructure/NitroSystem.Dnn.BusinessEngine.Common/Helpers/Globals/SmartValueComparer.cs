using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Globals
{
    public static class SmartValueComparer
    {
        public static bool Compare(object left, object right, string @operator)
        {
            @operator = @operator?.Trim().ToLowerInvariant();

            switch (@operator)
            {
                case "=":
                case "==": return Equals(left, right);
                case "!=": return !Equals(left, right);
                case "not-null": return left != null && !string.IsNullOrWhiteSpace(left.ToString());
                case "is-null": return left == null || string.IsNullOrWhiteSpace(left.ToString());

                case ">":
                case "<":
                case ">=":
                case "<=":
                    return CompareByType(left, right, @operator);

                default:
                    throw new NotSupportedException($"Operator '{@operator}' is not supported.");
            }
        }

        private static bool CompareByType(object left, object right, string op)
        {
            // null check
            if (left == null || right == null)
                return false;

            // Try DateTime
            if (TryParse<DateTime>(left, out var ldt) && TryParse<DateTime>(right, out var rdt))
                return CompareValues(ldt, rdt, op);

            // Try decimal (int, float, etc.)
            if (TryParse<decimal>(left, out var ld) && TryParse<decimal>(right, out var rd))
                return CompareValues(ld, rd, op);

            // Try bool
            if (TryParse<bool>(left, out var lb) && TryParse<bool>(right, out var rb))
                return CompareValues(lb, rb, op);

            // Try string (alphabetical comparison)
            if (left is string ls && right is string rs)
                return CompareValues(string.Compare(ls, rs, StringComparison.Ordinal), 0, op);

            // Try Guid (just equality and inequality)
            if (Guid.TryParse(left.ToString(), out var lg) && Guid.TryParse(right.ToString(), out var rg))
            {
                switch (op)
                {
                    case "==":
                        return lg == rg;
                    case "!=":
                        return lg != rg;
                    default:
                        throw new InvalidOperationException("Cannot compare Guids with relational operators.");
                }
            }

            throw new InvalidOperationException("Unsupported comparison types.");
        }

        private static bool TryParse<T>(object value, out T result)
        {
            result = default;
            if (value == null) return false;

            var type = typeof(T);

            try
            {
                if (type == typeof(DateTime) && DateTime.TryParse(value.ToString(), out var dt))
                {
                    result = (T)(object)dt;
                    return true;
                }

                if (type == typeof(decimal) && decimal.TryParse(value.ToString(), out var dec))
                {
                    result = (T)(object)dec;
                    return true;
                }

                if (type == typeof(bool) && bool.TryParse(value.ToString(), out var b))
                {
                    result = (T)(object)b;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool CompareValues<T>(T left, T right, string op) where T : IComparable
        {
            int cmp = left.CompareTo(right);

            switch (op)
            {
                case ">":
                    return cmp > 0;
                case "<":
                    return cmp < 0;
                case ">=":
                    return cmp >= 0;
                case "<=":
                    return cmp <= 0;
                default:
                    return false;
            }
        }
    }
}
