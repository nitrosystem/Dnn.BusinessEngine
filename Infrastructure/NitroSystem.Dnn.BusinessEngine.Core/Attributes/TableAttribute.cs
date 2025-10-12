using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; }

        public TableAttribute(string tableName)
        {
            this.TableName = tableName;
        }
    }
}
