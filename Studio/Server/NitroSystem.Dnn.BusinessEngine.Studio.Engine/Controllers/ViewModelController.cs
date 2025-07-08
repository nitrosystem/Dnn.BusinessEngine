using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.BusinessLogic.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Dapper.SqlMapper;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace NitroSystem.Dnn.BusinessEngine.BusinessLogic.Controllers
{
    public static class ViewModelController
    {
        public static void ImportViewModels(string json, PortalSettings portalSettings, UserInfo user, IDataContext ctx, HttpContext httpContext)
        {
            var viewModels = JsonConvert.DeserializeObject<IEnumerable<ViewModelViewModel>>(json);
            foreach (var viewModel in viewModels) SaveViewModel(viewModel, true, user, true, ctx, httpContext);
        }

        public static ViewModelViewModel SaveViewModel(ViewModelViewModel viewModel, bool isNew, UserInfo user, bool calledFromImport, IDataContext ctx, HttpContext httpContext)
        {
            var objViewModel = new ViewModelInfo();
            viewModel.CopyProperties(objViewModel);
            objViewModel.LastModifiedOnDate = viewModel.LastModifiedOnDate = DateTime.Now;
            objViewModel.LastModifiedByUserID = viewModel.LastModifiedByUserID = user.UserID;

            ViewModelRepository worker = calledFromImport ? new ViewModelRepository(ctx) : ViewModelRepository.Instance;

            if (calledFromImport || viewModel.ViewModelID == Guid.Empty)
            {
                objViewModel.CreatedOnDate = viewModel.CreatedOnDate = DateTime.Now;
                objViewModel.CreatedByUserID = viewModel.CreatedByUserID = user.UserID;

                viewModel.ViewModelID = worker.AddViewModel(objViewModel);
            }
            else
            {
                objViewModel.CreatedOnDate = viewModel.CreatedOnDate == DateTime.MinValue ? DateTime.Now : viewModel.CreatedOnDate;
                objViewModel.CreatedByUserID = viewModel.CreatedByUserID;

                worker.UpdateViewModel(objViewModel);
            }

            if (viewModel.Properties != null)
            {
                ViewModelPropertyRepository propertyWorker = calledFromImport ? new ViewModelPropertyRepository(ctx) : ViewModelPropertyRepository.Instance;

                if (!isNew)
                {
                    var oldProperties = propertyWorker.GetProperties(viewModel.ViewModelID);

                    var propertyIDs = oldProperties.Where(p => viewModel.Properties.Select(pp => pp.PropertyID).Contains(p.PropertyID) == false).Select(p => p.PropertyID);
                    propertyWorker.DeleteProperties(propertyIDs);
                }

                foreach (var property in viewModel.Properties)
                {
                    property.ViewModelID = viewModel.ViewModelID;

                    if (property.PropertyType != "viewModel" && property.PropertyType != "listOfViewModel") property.PropertyTypeID = null;

                    if (property.PropertyID == Guid.Empty)
                        property.PropertyID = propertyWorker.AddProperty(property);
                    else
                        propertyWorker.UpdateProperty(property);
                }
            }

            return viewModel;
        }
    }
}
