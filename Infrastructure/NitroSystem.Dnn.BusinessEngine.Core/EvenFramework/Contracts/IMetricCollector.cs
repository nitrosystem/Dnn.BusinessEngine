using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Contracts
{
    public interface IMetricCollector
    {
        void RecordNodeMetric(Guid eventId, string nodeName, string metricKey, double value);
        double? GetMetric(Guid eventId, string nodeName, string metricKey);
        void ClearForEvent(Guid eventId);
    }
}
