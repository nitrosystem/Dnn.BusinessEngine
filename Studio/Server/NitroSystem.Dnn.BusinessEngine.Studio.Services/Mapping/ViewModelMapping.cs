using DotNetNuke.Collections;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping
{
    public static class ViewModelMapping
    {
        #region ViewModel Mapping

        public static IEnumerable<ViewModelViewModel> MapViewModels(IEnumerable<ViewModelInfo> viewModels, IEnumerable<ViewModelPropertyInfo> properties)
        {
            var itemsDict = properties.GroupBy(c => c.ViewModelId)
                         .ToDictionary(g => g.Key, g => g.AsEnumerable());

            return viewModels.Select(viewModel =>
            {
                var items = itemsDict.TryGetValue(viewModel.Id, out var cols) ? cols : Enumerable.Empty<ViewModelPropertyInfo>();
                return MapViewModel(viewModel, items);
            });
        }

        public static ViewModelViewModel MapViewModel(ViewModelInfo viewModel, IEnumerable<ViewModelPropertyInfo> properties)
        {
            var mapper = new ExpressionMapper<ViewModelInfo, ViewModelViewModel>();
            mapper.AddCustomMapping(src => src, dest => dest.Properties,
                map => properties,
                src => properties != null
            );

            var result = mapper.Map(viewModel);
            return result;
        }

        public static ViewModelInfo MapViewModelInfo(ViewModelViewModel viewModel)
        {
            var mapper = new ExpressionMapper<ViewModelViewModel, ViewModelInfo>();
            return mapper.Map(viewModel);
        }

        #endregion
    }
}