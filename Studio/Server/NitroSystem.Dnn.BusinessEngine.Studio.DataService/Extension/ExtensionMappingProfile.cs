using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Entity
{
    public static class ExtensionMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<ExtensionManifest, ExtensionInfo>(
                    (src, dest) => dest.Owner = JsonConvert.SerializeObject(src.Owner));

            HybridMapper.BeforeMap<ExtensionManifest, ExtensionInfo>(
                    (src, dest) => dest.Resources = JsonConvert.SerializeObject(src.Resources));

            HybridMapper.BeforeMap<ExtensionManifest, ExtensionInfo>(
                (src, dest) => dest.Assemblies = JsonConvert.SerializeObject(src.Assemblies));

            HybridMapper.BeforeMap<ExtensionManifest, ExtensionInfo>(
                (src, dest) => dest.SqlProviders = JsonConvert.SerializeObject(src.SqlProviders));
        }
    }
}
