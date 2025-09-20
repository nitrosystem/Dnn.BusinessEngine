using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Api.Models;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.IO;
using System.Drawing;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System.IO.Compression;
using NitroSystem.Dnn.BusinessEngine.App.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Services.ModuleData;
using NitroSystem.Dnn.BusinessEngine.Common.Helpers.Globals;

namespace NitroSystem.Dnn.BusinessEngine.App.Api
{
    public class ModuleController : DnnApiController
    {
        private readonly IUserDataStore _userDataStore;
        private readonly IModuleService _moduleService;
        private readonly IActionService _actionService;
        private readonly IActionWorker _actionWorker;

        public ModuleController(
            IUserDataStore userDataStore,
            IModuleService moduleService,
            IActionService actionService,
            IActionWorker actionWorker
        )
        {
            _userDataStore = userDataStore;
            _moduleService = moduleService;
            _actionService = actionService;
            _actionWorker = actionWorker;
        }

        #region Common

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UploadImage()
        {
            var result = new UploadImageInfo() { Thumbnails = new List<string>() };

            try
            {
                var currentRequest = HttpContext.Current.Request;
                string fileName = string.Empty;

                if (HttpContext.Current.Request.Files.Count > 0)
                {
                    fileName = HttpContext.Current.Request.Files[0].FileName;
                    if (!Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(Path.GetExtension(fileName).ToLower()))
                        throw new Exception("File type not allowed");
                }

                if (Request.Content.IsMimeMultipartContent())
                {
                    string uploadPath = PortalSettings.HomeDirectory + "BusinessEngine/Images/";

                    var mapPath = HttpContext.Current.Server.MapPath(uploadPath);
                    if (!Directory.Exists(mapPath)) Directory.CreateDirectory(mapPath);

                    if (Path.GetExtension(fileName).ToLower() == ".webp")
                    {
                        var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(mapPath);

                        await Request.Content.ReadAsMultipartAsync(streamProvider);

                        foreach (var file in streamProvider.FileData)
                        {
                            result.FilePath = uploadPath + Path.GetFileName(file.LocalFileName);
                        }
                    }
                    else
                    {
                        var streamProvider = new MultipartMemoryStreamProvider();
                        await Request.Content.ReadAsMultipartAsync(streamProvider);

                        var content = streamProvider.Contents[streamProvider.Contents.Count - 1];
                        var stream = content.ReadAsStreamAsync().Result;

                        string imageFile = mapPath + "\\" + Guid.NewGuid().ToString() + Path.GetExtension(fileName);

                        var image = Image.FromStream(stream);
                        int imageWidth = image.Size.Width;
                        int imageHeight = image.Size.Height;

                        //resize large image
                        bool resizeLargeImage = false;
                        bool.TryParse(currentRequest.Params["ResizeLargeImage"], out resizeLargeImage);
                        if (resizeLargeImage)
                        {
                            if (resizeLargeImage)
                            {
                                int largeImageWidth = 1024;
                                int largeImageHeight = 0;

                                int.TryParse(currentRequest.Params["LargeImageWidth"], out largeImageWidth);
                                int.TryParse(currentRequest.Params["LargeImageHeight"], out largeImageHeight);

                                if (imageWidth > largeImageWidth || (largeImageHeight != 0 && imageHeight > largeImageHeight))
                                {
                                    imageWidth = largeImageWidth;
                                    imageHeight = largeImageHeight;
                                }
                            }
                        }

                        image = ImageUtil.ResizeImage(stream, imageFile, imageWidth, imageHeight);

                        result.FilePath = uploadPath + Path.GetFileName(imageFile);

                        //create thumbnail 1
                        bool createThumbnail = false;
                        bool.TryParse(currentRequest.Params["CreateThumbnail1"], out createThumbnail);
                        if (createThumbnail)
                        {
                            int thumbnailWidth = 150;
                            int thumbnailHeight = 0;
                            int.TryParse(currentRequest.Params["Thumbnail1Width"], out thumbnailWidth);
                            int.TryParse(currentRequest.Params["Thumbnail1Height"], out thumbnailHeight);

                            string thumbnailPath = Path.GetDirectoryName(imageFile) + "\\Thumbnails\\";
                            if (!Directory.Exists(thumbnailPath)) Directory.CreateDirectory(thumbnailPath);

                            string thumbnailFileName = Guid.NewGuid() + Path.GetExtension(imageFile);
                            string thumbnailFilePath = thumbnailPath + thumbnailFileName;

                            if (ImageUtil.ResizeImage(stream, thumbnailFilePath, thumbnailWidth, thumbnailHeight) != null)
                                result.Thumbnails.Add(uploadPath + "Thumbnails/" + thumbnailFileName);
                        }

                        //create thumbnail 2
                        createThumbnail = false;
                        bool.TryParse(currentRequest.Params["CreateThumbnail2"], out createThumbnail);
                        if (createThumbnail)
                        {
                            int thumbnailWidth = 150;
                            int thumbnailHeight = 0;
                            int.TryParse(currentRequest.Params["Thumbnail2Width"], out thumbnailWidth);
                            int.TryParse(currentRequest.Params["Thumbnail2Height"], out thumbnailHeight);

                            string thumbnailPath = Path.GetDirectoryName(imageFile) + "\\Thumbnails\\";
                            if (!Directory.Exists(thumbnailPath)) Directory.CreateDirectory(thumbnailPath);

                            string thumbnailFileName = Guid.NewGuid() + Path.GetExtension(imageFile);
                            string thumbnailFilePath = thumbnailPath + thumbnailFileName;

                            if (ImageUtil.ResizeImage(stream, thumbnailFilePath, thumbnailWidth, thumbnailHeight) != null)
                                result.Thumbnails.Add(uploadPath + "Thumbnails/" + thumbnailFileName);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!");
                    throw new HttpResponseException(response);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UploadPhoto()
        {
            var result = new UploadImageInfo() { Thumbnails = new List<string>() };

            try
            {
                var currentRequest = HttpContext.Current.Request;
                string fileName = string.Empty;

                if (HttpContext.Current.Request.Files.Count > 0)
                {
                    fileName = HttpContext.Current.Request.Files[0].FileName;
                    if (!Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(Path.GetExtension(fileName).ToLower()))
                        throw new Exception("File type not allowed");
                }

                if (Request.Content.IsMimeMultipartContent())
                {
                    string uploadPath = PortalSettings.HomeDirectory + "BusinessEngine/Images/";

                    var mapPath = HttpContext.Current.Server.MapPath(uploadPath);
                    if (!Directory.Exists(mapPath)) Directory.CreateDirectory(mapPath);

                    //Mahmoud => Add .svg
                    if (Path.GetExtension(fileName).ToLower() == ".webp" || Path.GetExtension(fileName).ToLower() == ".svg")
                    {
                        var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(mapPath);

                        await Request.Content.ReadAsMultipartAsync(streamProvider);

                        foreach (var file in streamProvider.FileData)
                        {
                            result.FilePath = uploadPath + Path.GetFileName(file.LocalFileName);
                        }
                    }
                    else
                    {
                        var streamProvider = new MultipartMemoryStreamProvider();
                        await Request.Content.ReadAsMultipartAsync(streamProvider);

                        var content = streamProvider.Contents[streamProvider.Contents.Count - 1];
                        var stream = content.ReadAsStreamAsync().Result;

                        string imageFile = mapPath + "\\" + Guid.NewGuid().ToString() + Path.GetExtension(fileName);

                        var image = Image.FromStream(stream);
                        int imageWidth = image.Size.Width;
                        int imageHeight = image.Size.Height;

                        //resize large image
                        bool resizeLargeImage = false;
                        bool.TryParse(currentRequest.Params["ResizeLargeImage"], out resizeLargeImage);
                        if (resizeLargeImage)
                        {
                            if (resizeLargeImage)
                            {
                                int largeImageWidth = 1024;
                                int largeImageHeight = 0;

                                int.TryParse(currentRequest.Params["LargeImageWidth"], out largeImageWidth);
                                int.TryParse(currentRequest.Params["LargeImageHeight"], out largeImageHeight);

                                if (imageWidth > largeImageWidth || (largeImageHeight != 0 && imageHeight > largeImageHeight))
                                {
                                    imageWidth = largeImageWidth;
                                    imageHeight = largeImageHeight;
                                }
                            }
                        }

                        image = ImageUtil.ResizeImage(stream, imageFile, imageWidth, imageHeight);

                        result.FilePath = uploadPath + Path.GetFileName(imageFile);

                        //create thumbnail 1
                        bool createThumbnail = false;
                        bool.TryParse(currentRequest.Params["CreateThumbnail1"], out createThumbnail);
                        if (createThumbnail)
                        {
                            int thumbnailWidth = 150;
                            int thumbnailHeight = 0;
                            int.TryParse(currentRequest.Params["Thumbnail1Width"], out thumbnailWidth);
                            int.TryParse(currentRequest.Params["Thumbnail1Height"], out thumbnailHeight);

                            string thumbnailPath = Path.GetDirectoryName(imageFile) + "\\Thumbnails\\";
                            if (!Directory.Exists(thumbnailPath)) Directory.CreateDirectory(thumbnailPath);

                            string thumbnailFileName = Guid.NewGuid() + Path.GetExtension(imageFile);
                            string thumbnailFilePath = thumbnailPath + thumbnailFileName;

                            if (ImageUtil.ResizeImage(stream, thumbnailFilePath, thumbnailWidth, thumbnailHeight) != null)
                                result.Thumbnails.Add(uploadPath + "Thumbnails/" + thumbnailFileName);
                        }

                        //create thumbnail 2
                        createThumbnail = false;
                        bool.TryParse(currentRequest.Params["CreateThumbnail2"], out createThumbnail);
                        if (createThumbnail)
                        {
                            int thumbnailWidth = 150;
                            int thumbnailHeight = 0;
                            int.TryParse(currentRequest.Params["Thumbnail2Width"], out thumbnailWidth);
                            int.TryParse(currentRequest.Params["Thumbnail2Height"], out thumbnailHeight);

                            string thumbnailPath = Path.GetDirectoryName(imageFile) + "\\Thumbnails\\";
                            if (!Directory.Exists(thumbnailPath)) Directory.CreateDirectory(thumbnailPath);

                            string thumbnailFileName = Guid.NewGuid() + Path.GetExtension(imageFile);
                            string thumbnailFilePath = thumbnailPath + thumbnailFileName;

                            if (ImageUtil.ResizeImage(stream, thumbnailFilePath, thumbnailWidth, thumbnailHeight) != null)
                                result.Thumbnails.Add(uploadPath + "Thumbnails/" + thumbnailFileName);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!");
                    throw new HttpResponseException(response);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UploadFile()
        {
            var result = new UploadFileInfo();

            try
            {
                if (HttpContext.Current.Request.Files.Count > 0)
                {
                    var fileName = HttpContext.Current.Request.Files[0].FileName;
                    if (!Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(Path.GetExtension(fileName).ToLower()))
                        throw new Exception("File type not allowed");

                    result.FileName = fileName;
                    result.FileType = MimeMapping.GetMimeMapping(fileName);
                }

                if (Request.Content.IsMimeMultipartContent())
                {
                    string uploadPath = PortalSettings.HomeDirectory + "BusinessEngine/Files/";
                    var mapPath = HttpContext.Current.Server.MapPath(uploadPath);
                    if (!Directory.Exists(mapPath)) Directory.CreateDirectory(mapPath);

                    var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(mapPath);

                    await Request.Content.ReadAsMultipartAsync(streamProvider);

                    foreach (var file in streamProvider.FileData)
                    {
                        result.FilePath = uploadPath + Path.GetFileName(file.LocalFileName);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!");
                    throw new HttpResponseException(response);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //public IFileInfo UploadDnnFile(string fileName, PortalSettings portalSetting, Stream stream)
        //{
        //    string rootPath = "BusinessEngine";
        //    int portalId = portalSetting.PortalId;

        //    IFolderInfo folder = null;
        //    if (FolderManager.Instance.FolderExists(portalId, rootPath))
        //        folder = FolderManager.Instance.GetFolder(portalId, rootPath);
        //    else
        //    {
        //        var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, "Standard");
        //        folder = FolderManager.Instance.AddFolder(new FolderMappingInfo
        //        {
        //            FolderProviderType = folderMapping.FolderProviderType,
        //            FolderMappingId = folderMapping.FolderMappingId,
        //            Priority = 1,
        //            PortalId = portalId,
        //        }, rootPath);
        //    }

        //    var file = FileManager.Instance.AddFile(folder, fileName, stream);
        //    return file;
        //}

        //[AllowAnonymous]
        //[HttpPost]
        //public async Task<HttpResponseMessage> UploadVideo()
        //{
        //    var result = new UploadVideoInfo();

        //    try
        //    {
        //        var currentRequest = HttpContext.Current.Request;

        //        if (HttpContext.Current.Request.Files.Count > 0)
        //        {
        //            var fileName = HttpContext.Current.Request.Files[0].FileName;
        //            if (!Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(Path.GetExtension(fileName)))
        //                throw new Exception("File type not allowed");
        //        }

        //        if (Request.Content.IsMimeMultipartContent())
        //        {
        //            int id = new Random().Next(1, 9999);
        //            string uploadPath = PortalSettings.HomeDirectory + "BusinessEngine/SubmitEntity/Videos/" + id + "/";

        //            var mapPath = HttpContext.Current.Server.MapPath(uploadPath);
        //            if (!Directory.Exists(mapPath)) Directory.CreateDirectory(mapPath);

        //            string watermark = HttpContext.Current.Request.Params["WatermarkImagePath"];
        //            if (!string.IsNullOrEmpty(watermark)) watermark = currentRequest.MapPath(watermark);

        //            var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(mapPath);

        //            await Request.Content.ReadAsMultipartAsync(streamProvider);

        //            foreach (var file in streamProvider.FileData)
        //            {
        //                result = new VideoUtil().SetVideo(currentRequest, file.LocalFileName, watermark);

        //                result.FilePath = uploadPath + Path.GetFileName(file.LocalFileName);
        //                result.ThumbnailPath = uploadPath + "thumbnail.png";
        //                result.Watermark = uploadPath + "watermark.mp4";
        //                result.Preloader = uploadPath + "preloader.mp4";
        //            }

        //            return Request.CreateResponse(HttpStatusCode.OK, result);
        //        }
        //        else
        //        {
        //            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!");
        //            throw new HttpResponseException(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        //[AllowAnonymous]
        //[HttpPost]
        //public async Task<HttpResponseMessage> UploadExcel()
        //{
        //    try
        //    {
        //        var currentRequest = HttpContext.Current.Request;

        //        if (HttpContext.Current.Request.Files.Count > 0)
        //        {
        //            var fileName = HttpContext.Current.Request.Files[0].FileName;
        //            if (!Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(Path.GetExtension(fileName)))
        //                throw new Exception("File type not allowed");
        //        }

        //        if (Request.Content.IsMimeMultipartContent())
        //        {
        //            string uploadPath = PortalSettings.HomeDirectory + "BusinessEngine/SubmitEntity/Excels/";

        //            var mapPath = HttpContext.Current.Server.MapPath(uploadPath);
        //            if (!Directory.Exists(mapPath)) Directory.CreateDirectory(mapPath);

        //            string columns = HttpContext.Current.Request.Params["Columns"];

        //            var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(mapPath);

        //            await Request.Content.ReadAsMultipartAsync(streamProvider);

        //            var file = streamProvider.FileData[0];

        //            var result = "";// populateExcelData(file.LocalFileName, columns.Split(',').ToList());

        //            return Request.CreateResponse(HttpStatusCode.OK, result);
        //        }
        //        else
        //        {
        //            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!");
        //            throw new HttpResponseException(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        #endregion

        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> GetModule(Guid moduleId, string connectionId, string pageUrl)
        {
            try
            {
                var module = await _moduleService.GetModuleViewModelAsync(moduleId);
                if (module == null) throw new Exception("Module Not Config");

                var moduleData = await _userDataStore.GetOrCreateModuleDataAsync(connectionId, module.Id, PortalSettings);
                moduleData["_PageParam"] = UrlHelper.ParsePageParameters(pageUrl);

                await _actionWorker.CallActionsAsync(moduleData, moduleId, null, "OnPageLoad", PortalSettings);

                var data = _userDataStore.GetDataForClients(moduleId, moduleData);

                var variables = await _moduleService.GetModuleVariablesAsync(moduleId, Services.Enums.ModuleVariableScope.Global);
                var fields = await _moduleService.GetFieldsViewModelAsync(moduleId);
                var actions = await _actionService.GetActionsDtoForClientAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    fields,
                    actions,
                    data,
                    variables
                });

                //return Request.CreateResponse(HttpStatusCode.OK, new
                //{
                //    mf = ProtectPayload(fields),
                //    ma = ProtectPayload(actions),
                //    md = ProtectPayload(data)
                //});
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> CallAction(ActionDto action)
        {
            try
            {
                var moduleData = await _userDataStore.UpdateModuleDataAsync(
                    action.ConnectionId,
                    action.ModuleId,
                    action.Data,
                    PortalSettings
                );

                moduleData["_PageParam"] = UrlHelper.ParsePageParameters(action.PageUrl);

                await _actionWorker.CallActionsAsync(action.ActionIds, moduleData, PortalSettings);

                var data = _userDataStore.GetDataForClients(action.ModuleId, moduleData);

                return Request.CreateResponse(HttpStatusCode.OK, new { data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage PingConnection(ActionDto user)
        {
            try
            {
                _userDataStore.Ping(user.ConnectionId);

                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static string ProtectPayload(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);

            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionMode.Compress))
                using (var writer = new StreamWriter(gzip))
                {
                    writer.Write(json);
                }

                return Convert.ToBase64String(output.ToArray());
            }
        }


        //یک نمونه هندلرد
        //(existingModule, incomingData) =>
        //            {
        //                var existing = existingModule;
        //                existing.Merge(incomingData, new JsonMergeSettings
        //                {
        //                    MergeArrayHandling = MergeArrayHandling.Replace,
        //                    MergeNullValueHandling = MergeNullValueHandling.Merge
        //                });
        //                return incomingData;
        //            },
    }
}