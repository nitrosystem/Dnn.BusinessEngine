using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class ServiceParamRepository
    {
        private readonly IDataContext _ctx;
        private const string _cachePrefix = "BE_ServiceParams_";

        public ServiceParamRepository(IDataContext ctx)
        {
            _ctx = ctx;
        }

        public static ServiceParamRepository Instance
        {
            get
            {
                return new ServiceParamRepository(DataContext.Instance());
            }
        }

        public Guid AddParam(ServiceParamInfo objServiceParamInfo)
        {
            Guid paramID = objServiceParamInfo.ParamID;
            objServiceParamInfo.ParamID = paramID == Guid.Empty ? Guid.NewGuid() : objServiceParamInfo.ParamID;

            var rep = _ctx.GetRepository<ServiceParamInfo>();
            rep.Insert(objServiceParamInfo);

            DataCache.ClearCache(_cachePrefix);

            return objServiceParamInfo.ParamID;
        }

        public void DeleteParams(Guid serviceID)
        {
            var rep = _ctx.GetRepository<ServiceParamInfo>();
            rep.Delete("Where ServiceID = @0", serviceID);

            DataCache.ClearCache(_cachePrefix);
        }

        public IEnumerable<ServiceParamInfo> GetParams(Guid serviceID)
        {
            var rep = _ctx.GetRepository<ServiceParamInfo>();
            return rep.Get(serviceID);
        }
    }
}