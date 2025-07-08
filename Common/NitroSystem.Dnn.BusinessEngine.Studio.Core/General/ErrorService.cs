using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.General
{
    public static class ErrorService
    {
        public static Exception ThrowUpdateFailedException(IEntity entity)
        {
            throw new Exception($"Update failed for entity {entity.GetType().Name} with ID {entity.Id}.");
        }

        public static Exception ThrowDeleteFailedException(IEntity entity)
        {
            throw new Exception($"Delete failed for entity {entity.GetType().Name} with ID {entity.Id}.");
        }
    }
}
