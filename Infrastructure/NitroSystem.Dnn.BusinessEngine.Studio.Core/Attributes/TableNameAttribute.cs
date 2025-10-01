using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TableNameAttribute : Attribute
    {
        public string TableName { get; }

        public TableNameAttribute(string tableName)
        {
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }
    }
}
