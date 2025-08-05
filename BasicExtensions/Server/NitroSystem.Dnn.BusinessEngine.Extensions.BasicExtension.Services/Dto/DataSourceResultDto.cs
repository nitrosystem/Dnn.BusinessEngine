using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Dto
{
    public class DataSourceResultDto
    {
        public IEnumerable<object> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
