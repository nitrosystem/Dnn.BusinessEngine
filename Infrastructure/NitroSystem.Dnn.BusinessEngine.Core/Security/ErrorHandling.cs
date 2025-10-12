using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.Security
{
    public static class ErrorHandling
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
