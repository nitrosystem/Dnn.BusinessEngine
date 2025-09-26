using DotNetNuke.Collections;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping
{
    public static class AppModelMapping
    {
        #region ViewModel Mapping

        public static IEnumerable<AppModelViewModel> MapAppModelsViewModel(IEnumerable<AppModelInfo> appModels, IEnumerable<AppModelPropertyInfo> properties)
        {
            var itemsDict = properties.GroupBy(c => c.AppModelId)
                         .ToDictionary(g => g.Key, g => g.AsEnumerable());

            return appModels.Select(appModel =>
            {
                var items = itemsDict.TryGetValue(appModel.Id, out var cols) ? cols : Enumerable.Empty<AppModelPropertyInfo>();
                return MapAppModelViewModel(appModel, items);
            });
        }

        public static AppModelViewModel MapAppModelViewModel(AppModelInfo appModel, IEnumerable<AppModelPropertyInfo> properties)
        {
            var mapper = new ExpressionMapper<AppModelInfo, AppModelViewModel>();
            mapper.AddCustomMapping(src => src, dest => dest.Properties,
                map => properties,
                src => properties != null
            );

            var result = mapper.Map(appModel);
            return result;
        }

        public static AppModelInfo MapAppModelInfo(AppModelViewModel appModel)
        {
            var mapper = new ExpressionMapper<AppModelViewModel, AppModelInfo>();
            return mapper.Map(appModel);
        }

        #endregion
    }
}