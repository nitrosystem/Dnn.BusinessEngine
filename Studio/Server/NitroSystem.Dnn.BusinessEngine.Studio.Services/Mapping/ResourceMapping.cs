using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Entity;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping
{
    public static class ResourceMapping
    {
        public static IEnumerable<PageResourceDto> MapPageResourcesDto(IEnumerable<PageResourceInfo> resources)
        {
            return resources.Select(resource =>
            {
                return MapPageResourceDto(resource);
            });
        }

        public static PageResourceDto MapPageResourceDto(PageResourceInfo resource)
        {
            var mapper = new ExpressionMapper<PageResourceInfo, PageResourceDto>();
            return mapper.Map(resource);
        }

        public static IEnumerable<PageResourceInfo> MapPageResourcesInfo(IEnumerable<PageResourceDto> resources)
        {
            return resources.Select(resource =>
            {
                return MapPageResourceInfo(resource);
            });
        }

        public static PageResourceInfo MapPageResourceInfo(PageResourceDto resource)
        {
            var mapper = new ExpressionMapper<PageResourceDto, PageResourceInfo>();
            return mapper.Map(resource);
        }
    }
}
