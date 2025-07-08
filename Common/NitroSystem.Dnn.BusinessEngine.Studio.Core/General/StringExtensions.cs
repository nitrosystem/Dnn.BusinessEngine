using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.General
{
    public static class FrequentStringTokenExtensions
    {
        private static readonly Dictionary<string, string> _tokenMappings = new()
        {
            { "[EXTPATH]", "/DesktopModules/BusinessEngine/Extensions" },
            { "[ModulePath]", "/DesktopModules/BusinessEngine" },
            { "[MODULEPATH]", "/DesktopModules/BusinessEngine" },
            { "[BuildPath]", "/DesktopModules/BusinessEngine/Build" }
        };

        public static string ReplaceFrequentTokens(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            foreach (var token in _tokenMappings)
            {
                input = input.Replace(token.Key, token.Value);
                //input = input.Replace(token.Key, token.Value, StringComparison.OrdinalIgnoreCase);
            }

            return input;
        }
    }

}
