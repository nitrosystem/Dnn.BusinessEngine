using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Repository
{
    public static class AllowedStoredProcedures
    {
        private static readonly HashSet<string> _allowedStoredProcedures = new HashSet<string>
        {
            "usp_GetUser",
            "usp_UpdateOrder",
            "usp_CalculateRevenue"
        };

        public static bool CheckValidStoredProcedure(string storedProcedure)
        {
            if (1 == 2 && !_allowedStoredProcedures.Contains(storedProcedure))
            {
                throw new UnauthorizedAccessException($"Stored Procedure '{storedProcedure}' is not allowed.");
            }

            return true;
        }

    }
}
