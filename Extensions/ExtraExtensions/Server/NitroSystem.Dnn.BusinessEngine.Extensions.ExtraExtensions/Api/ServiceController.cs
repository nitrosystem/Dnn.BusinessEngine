using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetNuke.Web.Api;
using DotNetNuke.Entities.Host;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Fields.Captcha;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Api
{
    [AllowAnonymous]
    public class ServiceController : DnnApiController
    {
        private readonly IModuleService _moduleService;
        private readonly IModuleFieldService _moduleFieldService;
        private readonly CaptchaService _captchaService;
        private static readonly HashSet<string> _allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/webp",
                "image/bmp",
                "image/tiff",
                "image/svg+xml",
                "image/x-icon",       // For favicon
                "image/heic",         // Apple new format
                "image/heif"
            };

        public ServiceController(IModuleService moduleService, IModuleFieldService moduleFieldService, CaptchaService captchaService)
        {
            _moduleService = moduleService;
            _moduleFieldService = moduleFieldService;
            _captchaService = captchaService;
        }

        [HttpPost]
        public async Task<IHttpActionResult> UploadImage()
        {
            if (!Request.Content.IsMimeMultipartContent())
                return Content(HttpStatusCode.UnsupportedMediaType, "Expected multipart content");

            // Use a stream provider to temporarily store parts
            var provider = new MultipartMemoryStreamProvider();
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid multipart request: " + ex.Message);
            }

            var result = new object();
            var contents = provider.Contents.ToList();
            var content = contents[0];
            if (content == null)
                return BadRequest("No file found");

            // skip non-file fields if necessary
            var disposition = content.Headers.ContentDisposition;
            if (disposition == null || string.IsNullOrEmpty(disposition.FileName))
                return BadRequest("No disposition found");

            var originalFileName = disposition.FileName.Trim('"').Trim();
            var ext = Path.GetExtension(originalFileName);
            var contentType = content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var buffer = await content.ReadAsByteArrayAsync();
            var fileSize = buffer.LongLength;

            content = contents[1];
            var fieldIdString = await content.ReadAsStringAsync();
            Guid.TryParse(fieldIdString, out var fieldId);
            var field = await _moduleFieldService.GetFieldDtoAsync(fieldId, false);
            var scenarioName = await _moduleService.GetScenarioNameAsync(field.ModuleId);
            var moduleName = await _moduleService.GetModuleNameAsync(field.ModuleId);

            // server-side validation
            if (!_allowedTypes.Contains(contentType) || !Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(ext))
            {
                result = new { IsSuccess = true, Status = $"invalid-type {contentType}", };
            }
            else if (fileSize == 0)
            {
                result = new { IsSuccess = false, Status = "empty" };
            }
            else if (field.Settings.TryGetValue("MaxFileLength", out var maxFileSize) && fileSize > (int)maxFileSize * 1024)
            {
                result = new { IsSuccess = false, Status = "too-large" };
            }
            else
            {
                var scenarioFolder = StringHelper.ToKebabCase(scenarioName);
                var moduleFolder = StringHelper.ToKebabCase(moduleName);
                var fieldFolder = StringHelper.ToKebabCase(field.FieldName);
                var relativePth = $"{PortalSettings.HomeDirectory}business-engine/{scenarioFolder}/images/{moduleFolder}/{fieldFolder}/{originalFileName}";
                var filePath = GlobalHelper.MapPath(relativePth);
                var directoryPath = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                    {
                        await stream.WriteAsync(buffer, 0, buffer.Length);
                    }

                    result = new { IsSuccess = true, FilePath = relativePth, Status = "ok" };
                }
                catch (Exception ex)
                {
                    result = new { IsSuccess = false, Status = ex.Message };
                }
            }

            return Ok(result);
        }

        public async Task<IHttpActionResult> UploadFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
                return Content(HttpStatusCode.UnsupportedMediaType, "Expected multipart content");

            // Use a stream provider to temporarily store parts
            var provider = new MultipartMemoryStreamProvider();
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid multipart request: " + ex.Message);
            }

            var result = new object();
            var contents = provider.Contents.ToList();
            var content = contents[0];
            if (content == null)
                return BadRequest("No file found");

            // skip non-file fields if necessary
            var disposition = content.Headers.ContentDisposition;
            if (disposition == null || string.IsNullOrEmpty(disposition.FileName))
                return BadRequest("No disposition found");

            var originalFileName = disposition.FileName.Trim('"').Trim();
            var ext = Path.GetExtension(originalFileName);
            var contentType = content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var buffer = await content.ReadAsByteArrayAsync();
            var fileSize = buffer.LongLength;

            content = contents[1];
            var fieldIdString = await content.ReadAsStringAsync();
            Guid.TryParse(fieldIdString, out var fieldId);
            var field = await _moduleFieldService.GetFieldDtoAsync(fieldId, false);
            var scenarioName = await _moduleService.GetScenarioNameAsync(field.ModuleId);
            var moduleName = await _moduleService.GetModuleNameAsync(field.ModuleId);

            // server-side validation
            //if (!_allowedTypes.Contains(contentType) || !Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(ext))
            //{
            //    result = new { IsSuccess = true, Status = $"invalid-type {contentType}", };
            //}
            //else
            if (fileSize == 0)
            {
                result = new { IsSuccess = false, Status = "empty" };
            }
            else if (field.Settings.TryGetValue("MaxFileLength", out var maxFileSize) && fileSize > (int)maxFileSize * 1024)
            {
                result = new { IsSuccess = false, Status = "too-large" };
            }
            else
            {
                var scenarioFolder = StringHelper.ToKebabCase(scenarioName);
                var moduleFolder = StringHelper.ToKebabCase(moduleName);
                var fieldFolder = StringHelper.ToKebabCase(field.FieldName);
                var relativePth = $"{PortalSettings.HomeDirectory}business-engine/{scenarioFolder}/files/{moduleFolder}/{fieldFolder}/{originalFileName}";
                var filePath = GlobalHelper.MapPath(relativePth);
                var directoryPath = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                    {
                        await stream.WriteAsync(buffer, 0, buffer.Length);
                    }

                    result = new { IsSuccess = true, FilePath = relativePth, Status = "ok" };
                }
                catch (Exception ex)
                {
                    result = new { IsSuccess = false, Status = ex.Message };
                }
            }

            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult GenerateCaptcha()
        {
            var result = _captchaService.Generate();
            return Ok(new CaptchaResponse
            {
                Token = result.Token,
                ImageBase64 = result.ImageBase64
            });
        }

        [HttpPost]
        public IHttpActionResult VerifyCaptcha([FromBody] CaptchaVerifyRequest request)
        {
            var isValid = _captchaService.Verify(request.Token, request.Answer);
            return Ok(new { success = isValid });
        }
    }
}
