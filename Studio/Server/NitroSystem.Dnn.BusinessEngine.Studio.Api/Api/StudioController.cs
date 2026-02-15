using System;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using DotNetNuke.Web.Api;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Controllers;
using NitroSystem.Dnn.BusinessEngine.Studio.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.AppModel;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.WebApi;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using DotNetNuke.Entities.Tabs;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    [DnnAuthorize(StaticRoles = "Administrators")]
    public class StudioController : DnnApiController
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICacheService _cacheService;
        private readonly IBaseService _baseService;
        private readonly IEntityService _entityService;
        private readonly IAppModelService _appModelServices;
        private readonly IServiceFactory _serviceFactory;
        private readonly IDefinedListService _definedListService;
        private readonly IExtensionService _extensionService;
        private readonly BackgroundJobWorker _backgroundJobWorker;
        private readonly BuildTypeRunner _buildTypeRunner;
        private readonly InstallExtensionRunner _installExtensionRunner;

        public StudioController(
            IServiceProvider serviceProvider,
            ICacheService cacheService,
            IBaseService baseService,
            IEntityService entityService,
            IAppModelService appModelService,
            IServiceFactory serviceFactory,
            IDefinedListService definedListService,
            IExtensionService extensionService,
            BackgroundJobWorker backgroundJobWorker,
            BuildTypeRunner buildTypeRunner,
            InstallExtensionRunner installExtensionRunner
            )
        {
            _serviceProvider = serviceProvider;
            _cacheService = cacheService;
            _baseService = baseService;
            _entityService = entityService;
            _appModelServices = appModelService;
            _serviceFactory = serviceFactory;
            _definedListService = definedListService;
            _extensionService = extensionService;
            _backgroundJobWorker = backgroundJobWorker;
            _buildTypeRunner = buildTypeRunner;
            _installExtensionRunner = installExtensionRunner;

        }

        #region Common

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ClearCacheAndAddCmsVersion()
        {
            try
            {
                HostController.Instance.Update("CrmVersion", (Host.CrmVersion + 1).ToString());

                _cacheService.ClearAll();

                //HttpRuntime.UnloadAppDomain();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage InitWebSocket()
        //{
        //    try
        //    {
        //        int? portId = !_webSocketManager.IsRunning
        //                ? _webSocketManager.EnsureStarted()
        //                : _webSocketManager.GetPort();

        //        return Request.CreateResponse(HttpStatusCode.OK, portId);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        #endregion

        #region Sidebar Explorer

        [HttpGet]
        public async Task<HttpResponseMessage> GetStudioOptions()
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var scenarios = await _baseService.GetScenariosViewModelAsync();
                var scenario = await _baseService.GetScenarioViewModelAsync(scenarioId);
                var roles = await _baseService.GetPortalRolesAsync(PortalSettings.PortalId);
                var groups = await _baseService.GetGroupsViewModelAsync(scenarioId, "SidebarExplorer");
                var explorerItems = await _baseService.GetExplorerItemsViewModelAsync(scenarioId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Scenarios = scenarios,
                    Scenario = scenario,
                    Roles = roles,
                    Groups = groups,
                    ExplorerItems = explorerItems
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> RefreshSidebarExplorerItems()
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var explorerItems = await _baseService.GetExplorerItemsViewModelAsync(scenarioId);

                return Request.CreateResponse(HttpStatusCode.OK, explorerItems);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UpdateItemGroup(GroupItemDto item)
        {
            try
            {
                bool result = false;

                switch (item.GroupType)
                {
                    case "Entity":
                        result = await _entityService.UpdateGroupColumn(item.ItemId, item.GroupId);
                        break;
                    case "AppModel":
                        result = await _appModelServices.UpdateGroupColumnAsync(item.ItemId, item.GroupId);
                        break;
                    case "Service":
                        result = await _serviceFactory.UpdateGroupColumn(item.ItemId, item.GroupId);
                        break;
                }

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Scenario

        [HttpGet]
        public async Task<HttpResponseMessage> GetScenarios()
        {
            try
            {
                var scenarios = await _baseService.GetScenariosViewModelAsync();

                return Request.CreateResponse(HttpStatusCode.OK, scenarios);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetScenario()
        {
            try
            {
                return await GetScenario(Guid.Empty);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetScenario(Guid scenarioId)
        {
            try
            {
                var scenario = await _baseService.GetScenarioViewModelAsync(scenarioId);

                return Request.CreateResponse(HttpStatusCode.OK, scenario);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveScenario(ScenarioViewModel scenario)
        {
            try
            {
                scenario.Id = await _baseService.SaveScenarioAsync(scenario, scenario.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, scenario);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteScenario(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _baseService.DeleteScenarioAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Group

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveGroup(GroupViewModel group)
        {
            try
            {
                group.ScenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                group.Id = await _baseService.SaveGroupAsync(group, group.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, group.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteGroup(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _baseService.DeleteGroupAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetGroupItems(Guid groupId, string groupType)
        {
            try
            {
                var items = await _baseService.GetGroupItemsAsync(groupId, groupType);

                return Request.CreateResponse(HttpStatusCode.OK, items);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Entities

        [DnnAuthorize]
        [HttpGet]
        public async Task<HttpResponseMessage> GetEntities(int pageIndex, int pageSize, string searchText = null,
            byte? entityType = null, bool? isReadonly = null, string sortBy = "Newest")
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var entitiesTask = await _entityService.GetEntitiesViewModelAsync(scenarioId, pageIndex, pageSize,
                    searchText, entityType, isReadonly, sortBy);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Entities = entitiesTask.Items,
                    Page = new PagingInfo(entitiesTask.TotalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetEntity()
        {
            return await GetEntity(Guid.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetEntity(Guid entityId)
        {
            try
            {
                var entity = await _entityService.GetEntityViewModelAsync(entityId);

                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDatabaseObjects()
        {
            try
            {
                var results = await _entityService.GetDatabaseObjects();

                return Request.CreateResponse(HttpStatusCode.OK, new { Tables = results.Tables, Views = results.Views });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDatabaseObjectColumns(string objectName)
        {
            try
            {
                var columns = await _entityService.GetDatabaseObjectColumns(objectName);

                return Request.CreateResponse(HttpStatusCode.OK, columns);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveEntity(EntityViewModel entity)
        {
            try
            {
                if (!entity.IsReadonly)
                    entity.TableName = (entity.Settings["DatabaseObjectPrefixName"].ToString() +
                                        entity.Settings["DatabaseObjectPostfixName"].ToString());

                entity.Id = await _entityService.SaveEntity(entity, entity.Id == Guid.Empty, HttpContext.Current);

                entity = await _entityService.GetEntityViewModelAsync(entity.Id);

                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteEntity(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _entityService.DeleteEntityAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region App Models

        [HttpGet]
        public async Task<HttpResponseMessage> GetAppModels(int pageIndex, int pageSize, string searchText = null,
            string sortBy = "Newest")
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                var appModels = await _appModelServices.GetAppModelsAsync(scenarioId, pageIndex, pageSize, searchText, sortBy);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    AppModels = appModels.Items,
                    Page = new PagingInfo(appModels.TotalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAppModel()
        {
            return await GetAppModel(Guid.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAppModel(Guid appModelId)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                var entities = await _entityService.GetEntitiesListItemAsync(scenarioId, "EntityName");
                var appModels = await _appModelServices.GetAppModelsAsync(scenarioId, 1, 1000, null, "Title");
                var appModel = await _appModelServices.GetAppModelAsync(appModelId);
                var propertyTypes = Constants.VariableTypes.Select(kvp => new { Text = kvp.Key, Value = kvp.Value });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Entities = entities,
                    AppModels = appModels,
                    AppModel = appModel,
                    PropertyTypes = propertyTypes,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveAppModel(AppModelViewModel appModel)
        {
            try
            {
                var scenarioName = await _baseService.GetScenarioNameAsync(appModel.ScenarioId);
                var properties = HybridMapper.MapCollection<AppModelPropertyViewModel, PropertyDefinition>(appModel.Properties);
                var request = new BuildTypeRequest()
                {
                    ScenarioName = scenarioName,
                    BasePath = PortalSettings.HomeSystemDirectory,
                    ModelName = appModel.ModelName,
                    Version = "01.00.00",
                    Properties = properties.Cast<IPropertyDefinition>().ToList()
                };

                var type = await _buildTypeRunner.RunAsync(request);
                appModel.TypeRelativePath = type.RelativePath;
                appModel.TypeFullName = type.TypeFullName;
                appModel.Id = await _appModelServices.SaveAppModelAsync(appModel, appModel.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, appModel);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteAppModel(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _appModelServices.DeleteAppModelAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Services

        [HttpGet]
        public async Task<HttpResponseMessage> GetServiceTypesListItem()
        {
            var serviceTypes = await _serviceFactory.GetServiceTypesListItemAsync("GroupViewOrder", "ViewOrder");

            return Request.CreateResponse(HttpStatusCode.OK, serviceTypes);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetServices(int pageIndex, int pageSize, string searchText = null,
            string serviceDomain = null, string serviceType = null, string sortBy = "Newest")
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                var serviceTypes = await _serviceFactory.GetServiceTypesListItemAsync("ServiceDomain", "ServiceType");
                var items = await _serviceFactory.GetServicesViewModelAsync(scenarioId, pageIndex, pageSize, searchText, serviceDomain, serviceType, sortBy);
                var services = items.Items;
                var totalCount = items.TotalCount;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ServiceTypes = serviceTypes,
                    Services = services,
                    Page = new PagingInfo(totalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetService(string serviceType)
        {
            return await GetService(serviceType, Guid.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetService(string serviceType, Guid serviceId)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var items = await _serviceFactory.GetServiceViewModelAsync(scenarioId, serviceType, serviceId);
                var service = items.Service;
                var extension = items.Extension;
                var extensionDependency = items.ExtensionDependency;
                var serviceCacheKeys = await _serviceFactory.GetServiceKeysAsync();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Service = service,
                    ExtensionService = extension,
                    ExtensionDependency = extensionDependency,
                    ServiceCacheKeys = serviceCacheKeys
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetServiceParams(Guid serviceId)
        {
            try
            {
                var serviceParams = await _serviceFactory.GetServiceParamsAsync(serviceId);

                return Request.CreateResponse(HttpStatusCode.OK, serviceParams);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveService(SaveServiceDto postData)
        {
            try
            {
                var result = await _serviceFactory.SaveServiceAsync(postData.Service, postData.ExtensionServiceJson, postData.Service.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ServiceId = result.ServiceId,
                    ExtensionServiceId = result.ExtensionServiceId
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteService(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _serviceFactory.DeleteServiceAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Defined Lists

        [HttpGet]
        public async Task<HttpResponseMessage> GetDefinedLists()
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                var definedLists = await _definedListService.GetDefinedLists(scenarioId);

                return Request.CreateResponse(HttpStatusCode.OK, definedLists);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDefinedListByListName(string listName = "")
        {
            try
            {
                var definedList = !string.IsNullOrEmpty(listName)
                    ? await _definedListService.GetDefinedListByListName(listName)
                    : null;

                return Request.CreateResponse(HttpStatusCode.OK, definedList);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveDefinedList(DefinedListViewModel definedList)
        {
            try
            {
                definedList.ScenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                definedList.Id = await _definedListService.SaveDefinedList(definedList, definedList.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, definedList.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Extensions

        [HttpGet]
        public async Task<HttpResponseMessage> GetExtensions()
        {
            try
            {
                var extensions = await _extensionService.GetExtensionsViewModelAsync();
                var availableExtensions = _extensionService.GetAvailableExtensionsViewModel();

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { Extensions = extensions, AvailableExtensions = availableExtensions });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<HttpResponseMessage> onInstallAvailableExtension([FromUri] string extensionFilename)
        //{
        //    try
        //    {
        //        var scenarioID = Guid.Parse(Request.Headers.GetValues("ScenarioID").First());

        //        var basePath = Constants.MapPath("~/DesktopModules/BusinessEngine/install");
        //        var filename = Path.Combine(basePath, extensionFilename);
        //        var result = await InstallExtension(filename);

        //        File.Delete(filename);

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UploadExtensionPackage()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, BadRequest("Invalid request format. Multipart content expected."));

                // Create temp upload folder
                var uploadPath = Path.Combine(PortalSettings.HomeSystemDirectoryMapPath, @"business-engine\temp\extensions\");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(uploadPath);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                var filename = uploadPath + Path.GetFileName(streamProvider.FileData[0].LocalFileName);
                var extractPath = uploadPath + Path.GetFileNameWithoutExtension(filename);
                ZipProvider.Unzip(filename, extractPath);

                var files = Directory.GetFiles(extractPath);
                var manifestFile = files.FirstOrDefault(f => Path.GetFileName(f) == "manifest.json");
                var manifestJson = await FileUtil.GetFileContentAsync(manifestFile);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ManifestJson = manifestJson,
                    ExtractPath = extractPath
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> InstallExtension(InstallExtensionRequest request)
        {
            request.BasePath = PortalSettings.HomeSystemDirectory;
            request.ModulePath = Constants.MapPath("~/DesktopModules/BusinessEngine");

            var files = Directory.GetFiles(request.ExtractPath);
            var manifestFile = files.FirstOrDefault(f => Path.GetFileName(f) == "manifest.json");
            var manifestJson = await FileUtil.GetFileContentAsync(manifestFile);

            request.Manifest = JsonConvert.DeserializeObject<ExtensionManifest>(manifestJson);

            var response = await _installExtensionRunner.RunAsync(request);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        #region Export Export

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> LoadExportedFile()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, BadRequest("Invalid request format. Multipart content expected."));

                // Create temp upload folder
                var uploadPath = Path.Combine(PortalSettings.HomeSystemDirectoryMapPath, @"business-engine\temp\import\");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(uploadPath);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                var filename = uploadPath + Path.GetFileName(streamProvider.FileData[0].LocalFileName);
                var extractPath = uploadPath + Path.GetFileNameWithoutExtension(filename);
                ZipProvider.Unzip(filename, extractPath);

                var tabs = TabController.Instance.GetTabsByPortal(PortalSettings.PortalId)
                .Values
                .Where(t => !t.IsDeleted) // Exclude deleted pages
                .OrderBy(t => t.TabOrder)
                .Select(t => new { t.TabID, t.TabName })
                .ToList();

                var exportedFile = $@"{extractPath}\export.json";
                var json = await FileUtil.GetFileContentAsync(exportedFile);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ExportedJson = json,
                    ExportedFile = exportedFile,
                    Pages = tabs,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> Export(ExportRequest postData)
        {
            try
            {
                var basePath = PortalSettings.HomeSystemDirectoryMapPath;
                var baseRelativePath = PortalSettings.HomeSystemDirectory;

                postData.Id = Guid.NewGuid();

                var exportRunner = new ExportRunner(_serviceProvider, postData, basePath, baseRelativePath);
                await exportRunner.Export();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> Import(ImportRequest postData)
        {
            try
            {
                postData.Params.Add("BasePath", PortalSettings.HomeSystemDirectoryMapPath);
                postData.Params.Add("CurrentPortalId", PortalSettings.PortalId);
                postData.Params.Add("CurrentUserId", UserInfo.UserID);

                var importRunner = new ImportRunner(_serviceProvider, postData);
                await importRunner.Import();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion
    }
}