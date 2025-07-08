using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.BusinessLogic.DTO;
using NitroSystem.Dnn.BusinessEngine.BusinessLogic.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Data.RepositoryBase;

namespace NitroSystem.Dnn.BusinessEngine.BusinessLogic.Controllers
{
    public class ServiceController<T>  where T :class
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RepositoryBase<ServiceInfo> _repository;

        public ServiceController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = new RepositoryBase<ServiceInfo>(_unitOfWork);
        }

        //public static void ImportServices(string json, PortalSettings portalSettings, UserInfo user, IdataContext ctx, HttpContext httpContext)
        //{
        //    var services = JsonConvert.DeserializeObject<IEnumerable<ServiceViewModel>>(json);
        //    foreach (var service in services) SaveService(service, true, user, true, ctx);
        //}

        public async Task<ServiceInfo> GetService(Guid id)
        {
            await _repository.GetAsync(id);
        }

        public async Task<ServiceViewModel> SaveService(ServiceViewModel service, bool isNew, UserInfo user, bool calledFromImport, IdataContext ctx)
        {
            var objServiceInfo = new ServiceInfo();
            service.CopyProperties(objServiceInfo);
            objServiceInfo.LastModifiedOnDate = service.LastModifiedOnDate = DateTime.Now;
            objServiceInfo.LastModifiedByUserId = service.LastModifiedByUserId = user.UserId;

            if (isNew)
            {
                objServiceInfo.CreatedOnDate = service.CreatedOnDate = DateTime.Now;
                objServiceInfo.CreatedByUserId = service.CreatedByUserId = user.UserId;

                service.Id = await _repository.AddAsync(objServiceInfo);
            }
            else
            {
                objServiceInfo.CreatedOnDate = service.CreatedOnDate == DateTime.MinValue ? DateTime.Now : service.CreatedOnDate;
                objServiceInfo.CreatedByUserId = service.CreatedByUserId;

                await _repository.UpdateAsync(objServiceInfo);
            }

            //if (service.Params != null && service.Params.Count() > 0)
            //{
            //    ServiceParamRepository workerServiceParam = calledFromImport ? new ServiceParamRepository(ctx) : ServiceParamRepository.Instance;

            //    workerServiceParam.DeleteParams(service.ServiceId);

            //    foreach (var objServiceParamInfo in service.Params ?? Enumerable.Empty<ServiceParamInfo>())
            //    {
            //        objServiceParamInfo.ServiceId = service.ServiceId;

            //        workerServiceParam.AddParam(objServiceParamInfo);
            //    }
            //}

            return service;
        }

        public async void DeleteService(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
