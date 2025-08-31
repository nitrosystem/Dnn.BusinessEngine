using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ModuleData
{
    public class GarbageCollector : SchedulerClient
    {
        private readonly IUserDataStore _userDataStore;

        public GarbageCollector(ScheduleHistoryItem objScheduleHistoryItem, IUserDataStore userDataStore)
        {
            base.ScheduleHistoryItem = objScheduleHistoryItem;

            _userDataStore = userDataStore;
        }

        public override void DoWork()
        {
            try
            {
                base.ScheduleHistoryItem.AddLogNote(@"<br/><strong>""Business Engine Garbage Collector"" task job was started!</strong>");

                _userDataStore.CleanupOldConnections(TimeSpan.FromMinutes(5));

                base.ScheduleHistoryItem.AddLogNote(@"<br/><strong>""Business Engine Garbage Collector"" ran successfully!</strong>");

                base.ScheduleHistoryItem.Succeeded = true;
            }
            catch (Exception exc)
            {
                base.ScheduleHistoryItem.Succeeded = false;

                base.ScheduleHistoryItem.AddLogNote(@"<br/><strong>""Business Engine Garbage Collector"" has execption error</strong>");
                base.ScheduleHistoryItem.AddLogNote("EXCEPTION: " + exc);

                Errored(ref exc);

                Exceptions.LogException(exc);
            }
        }
    }
}
