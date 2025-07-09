using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.RepositoryBase
{
    public class BaseEntity
    {
        public string TableName { get; }

        private BaseEntity(string name)
        {
            TableName = name;
        }

        public static readonly BaseEntity Scenario = new BaseEntity("BusinessEngine_Scenarios");
        public static readonly BaseEntity Service = new BaseEntity("BusinessEngine_Services");

        public override string ToString() => TableName;
    }
}
