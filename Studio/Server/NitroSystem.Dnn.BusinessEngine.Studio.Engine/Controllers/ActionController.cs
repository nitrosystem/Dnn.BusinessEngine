using NitroSystem.Dnn.BusinessEngine.BusinessLogic.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using System;
using System.Threading.Tasks;
using System.Web;
using static Dapper.SqlMapper;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Data.RepositoryBase;

namespace NitroSystem.Dnn.BusinessEngine.BusinessLogic.Controllers
{
    public class ActionController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //public static void ImportViewModels(string json, PortalSettings portalSettings, UserInfo user, IDataContext ctx, HttpContext httpContext)
        //{
        //    var viewModels = JsonConvert.DeserializeObject<IEnumerable<ViewModelViewModel>>(json);
        //    foreach (var viewModel in viewModels) SaveViewModel(viewModel, true, user, true, ctx, httpContext);
        //}

        public async Task<ActionViewModel> SaveAction(ActionViewModel action, bool isNew, UserInfo user, bool calledFromImport, IDataContext ctx, HttpContext httpContext)
        {
            var repository = new RepositoryBase<ActionInfo>(_unitOfWork);

            var objActionInfo = new ActionInfo();
            action.CopyProperties(objActionInfo);
            objActionInfo.LastModifiedOnDate = action.LastModifiedOnDate = DateTime.Now;
            objActionInfo.LastModifiedByUserId = action.LastModifiedByUserId = user.UserID;

            if (isNew)
            {
                objActionInfo.CreatedOnDate = action.CreatedOnDate = DateTime.Now;
                objActionInfo.CreatedByUserId = action.CreatedByUserId = user.UserID;

                action.Id = await repository.AddAsync(objActionInfo);
            }
            else
            {
                objActionInfo.CreatedOnDate = action.CreatedOnDate == DateTime.MinValue ? DateTime.Now : action.CreatedOnDate;
                objActionInfo.CreatedByUserId = action.CreatedByUserId;

                await repository.UpdateAsync(objActionInfo);
            }

            return action;
        }
    }
}
