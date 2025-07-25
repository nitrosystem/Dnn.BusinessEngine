using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Api.DTO;
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

namespace NitroSystem.Dnn.BusinessEngine.Api
{
    public class ModuleController : DnnApiController
    {
        private readonly IModuleData _moduleData;
        private readonly IModuleService _moduleService;
        private readonly IActionWorker _actionWorker;

        public ModuleController(
            IModuleData moduledata,
            IModuleService moduleService,
            IActionWorker actionWorker
        )
        {
            _moduleData = moduledata;
            _moduleService = moduleService;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> GetModuleData(ModuleDTO postData)
        {
            try
            {
                var moduleId = Guid.Parse(Request.Headers.GetValues("ModuleId").First());

                string connectionId = Guid.Empty.ToString();// postData.ConnectionId;

                var module = await _moduleService.GetModuleViewModelAsync(moduleId);
                if (module == null) throw new Exception("Module Not Config");

                await _moduleData.InitializeModuleData(connectionId, module.Id);
                //this._moduleData.InitModuleData(moduleGuid, connectionId, this.UserInfo.UserId, null, null, postData.PageUrl, !module.IsSSR);

                //await _actionWorker.CallActions(moduleId, null, "OnPageLoad", true); 

                var fields = await _moduleService.GetFieldsViewModelAsync(moduleId);

                //this._moduleData.SetFieldItem(field.FieldName, lightField);

                //foreach (var field in fields.Where(f => f.IsSelective && !string.IsNullOrWhiteSpace(f.DataSource.DataSourceJson)))
                //{
                //    field.DataSource = ModuleFieldMappings.GetFieldDataSource(field.DataSource.DataSourceJson, this._serviceWorker, false);
                //}

                //var variables = ModuleVariableMapping.GetVariablesViewModel(moduleId);

                //var actions = ActionMapping.GetActionsDTO(moduleId);

                //var moduleData = this._moduleData.GetModuleData(connectionId, moduleId);

                //if (this.UserInfo.UserId >= 0)
                //    moduleData["CurrentUser"] = JObject.FromObject(new { this.UserInfo.UserId, this.UserInfo.DisplayName });

                //IEnumerable<PageResourceView> moduleResources = Enumerable.Empty<PageResourceView>();
                //if (postData.IsDashboardModule) moduleResources = PageResourceRepository.Instance.GetActiveModuleResources(moduleId).Where(r => r.ResourceType == "css").OrderBy(r => r.ResourceType);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Fields = fields,
                    //Actions = actions,
                    ConnectionId = connectionId,
                    //Variables = variables,
                    //Data = moduleData,
                    //DashboardModuleResources = moduleResources
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}