using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Contracts
{
    public interface IDslContext
    {
        /// <summary>
        /// Returns the root object by name (e.g. "ArtistCategory")
        /// </summary>
        object GetRoot(string name);

        /// <summary>
        /// Returns the static CLR type of the root object
        /// </summary>
        Type GetRootType(string name);

        void SetRoot(string name, object value);

        object InvokeFunction(string name, object[] args);
    }
}
