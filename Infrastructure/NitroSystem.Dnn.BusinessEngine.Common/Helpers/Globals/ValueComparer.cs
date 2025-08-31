using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Common.Globals
{
    public static class ValueComparer
    {
        public static bool Compare(object left, object right, string @operator)
        {
            switch (@operator.ToLowerInvariant())
            {
                case "=":
                case "==":
                    return object.Equals(left, right);

                case "!=":
                    return !object.Equals(left, right);

                case ">":
                    return CompareAsComparable(left, right, (x, y) => x > y);

                case "<":
                    return CompareAsComparable(left, right, (x, y) => x < y);

                case ">=":
                    return CompareAsComparable(left, right, (x, y) => x >= y);

                case "<=":
                    return CompareAsComparable(left, right, (x, y) => x <= y);

                case "isfilled":
                    return left != null && !string.IsNullOrWhiteSpace(left.ToString());

                case "isnull":
                    return left == null || string.IsNullOrWhiteSpace(left.ToString());

                default:
                    throw new NotSupportedException($"Operator '{@operator}' is not supported.");
            }
        }

        private static bool CompareAsComparable(object left, object right, Func<decimal, decimal, bool> comparator)
        {
            if (TryConvertToDecimal(left, out var leftVal) && TryConvertToDecimal(right, out var rightVal))
            {
                return comparator(leftVal, rightVal);
            }

            throw new InvalidOperationException("Values are not numeric-comparable.");
        }

        private static bool TryConvertToDecimal(object input, out decimal result)
        {
            if (input == null)
            {
                result = 0;
                return false;
            }

            return decimal.TryParse(input.ToString(), out result);
        }
    }
}
