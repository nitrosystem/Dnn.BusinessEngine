using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class ServiceRepository
    {
        private readonly IDataContext _ctx;
        private const string CachePrefix = "BE_Services_";

        public ServiceRepository(IDataContext ctx)
        {
            _ctx = ctx;
        }

        public static ServiceRepository Instance
        {
            get
            {
                return new ServiceRepository(DataContext.Instance());
            }
        }

        public Guid AddService(ServiceInfo objServiceInfo)
        {
            Guid serviceID = objServiceInfo.ServiceID;
            objServiceInfo.ServiceID = serviceID == Guid.Empty ? Guid.NewGuid() : objServiceInfo.ServiceID;

            var rep = _ctx.GetRepository<ServiceInfo>();
            rep.Insert(objServiceInfo);

            DataCache.ClearCache(CachePrefix);

            return objServiceInfo.ServiceID;
        }

        public void UpdateService(ServiceInfo objServiceInfo)
        {
            var rep = _ctx.GetRepository<ServiceInfo>();
            rep.Update(objServiceInfo);

            DataCache.ClearCache(CachePrefix);
        }

        public void UpdateGroup(Guid itemID, Guid? groupID)
        {
            _ctx.Execute(System.Data.CommandType.Text, "UPDATE dbo.BusinessEngine_Services SET GroupID = @0 WHERE ServiceID = @1", groupID, itemID);

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteService(Guid serviceID)
        {
            ServiceInfo objServiceInfo = GetService(serviceID);

            var rep = _ctx.GetRepository<ServiceInfo>();
            rep.Delete(objServiceInfo);

            DataCache.ClearCache(CachePrefix);
        }

        public ServiceInfo GetService(Guid serviceID)
        {
            var rep = _ctx.GetRepository<ServiceInfo>();
            return rep.GetById(serviceID);
        }

        public ServiceInfo GetServiceByName(Guid scenarioID, string serviceName, bool isEnabled)
        {
            return GetServices().FirstOrDefault(s => s.ScenarioID == scenarioID && s.ServiceName == serviceName && s.IsEnabled == isEnabled);
        }

        public ServiceInfo GetServiceByName(string scenarioName, string serviceName, bool isEnabled)
        {
            return _ctx.ExecuteSingleOrDefault<ServiceInfo>(System.Data.CommandType.Text, "SELECT sr.* FROM dbo.BusinessEngine_Services sr INNER JOIN dbo.BusinessEngine_Scenarios s on sr.ScenarioID = sr.ScenarioID WHERE s.ScenarioName = @0 and sr.ServiceName = @1 and sr.IsEnabled = @2", scenarioName, serviceName, isEnabled);
        }

        public ServiceInfo GetServiceByID(int portalid, Guid serviceID, bool isEnabled)
        {
            return GetServices().FirstOrDefault(t => t.ServiceID == serviceID && (!isEnabled || t.IsEnabled));
        }

        public IPagedList<ServiceInfo> GetServices(Guid scenarioID, int pageIndex, int pageSize, string searchText, string serviceType, string serviceSubtype)
        {
            if (searchText == null) searchText = "";

            var rep = _ctx.GetRepository<ServiceInfo>();
            return rep.Find(pageIndex, pageSize, "WHERE scenarioID = @0 and ServiceName like N'%' + @1 +'%' and (ISNULL(@2,'') = '' or ServiceType =  @2) and (ISNULL(@3,'') = '' or ServiceSubtype = @3)", scenarioID, searchText, serviceType, serviceSubtype);
        }

        public IEnumerable<ServiceInfo> GetServices(string serviceType = "")
        {
            var rep = _ctx.GetRepository<ServiceInfo>();
            return rep.Find("Where @0 = '' or ServiceType = @0", serviceType).OrderBy(s => s.ViewOrder);
        }

        public IEnumerable<ServiceInfo> GetServices(Guid? scenarioID)
        {
            var rep = _ctx.GetRepository<ServiceInfo>();
            return rep.Get(scenarioID);
        }

        public IPagedList<ServiceInfo> GetServices(Guid scenarioID, int pageIndex, int pageSize, string searchText)
        {
            if (searchText == null) searchText = "";

            var rep = _ctx.GetRepository<ServiceInfo>();
            return rep.Find(pageIndex, pageSize, "WHERE scenarioID = @0 and (ServiceName like N'%' + @1 +'%' or ServiceType like N'%' + @1 +'%' or ServiceSubtype like N'%' + @1 +'%')", scenarioID, searchText);
        }
    }
}