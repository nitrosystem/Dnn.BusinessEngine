using System;
using System.IO;
using System.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;


namespace NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Services
{
    public class UpdateUserService
    {
        private readonly IServiceFactory _serviceFactory;

        public UpdateUserService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        // --- Constants to remove magic strings ---
        private static class UserKeys
        {
            public const string UserID = "UserID";
            public const string FirstName = "FirstName";
            public const string LastName = "LastName";
            public const string DisplayName = "DisplayName";
            public const string UserPhoto = "UserPhoto";
        }

        public async Task UpdateUser(Guid serviceId, IReadOnlyList<ParamInfo> filledParams)
        {
            var service = await _serviceFactory.GetServiceViewModelAsync(serviceId)
                ?? throw new InvalidOperationException($"Service '{serviceId}' was not found.");

            var settings = service.Settings ?? new Dictionary<string, object>();
            var paramLookup = ParamHelper.GetLookup(filledParams);
            int.TryParse(ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.UserID), paramLookup), out var userId);
            string firstName = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.FirstName), paramLookup);
            string lastName = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.LastName), paramLookup);
            string displayName = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.DisplayName), paramLookup) ?? (firstName + " " + lastName);
            string userPhoto = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.UserPhoto), paramLookup);

            var portalSettings = PortalSettings.Current;
            var objUserInfo = UserController.GetUserById(portalSettings.PortalId, userId);
            ProfileController.GetUserProfile(ref objUserInfo);

            int? fileId = GetOrUpdateDnnFile(portalSettings, objUserInfo, userPhoto);
            objUserInfo.Profile.Photo = fileId?.ToString();
            objUserInfo.Profile.FirstName = firstName;
            objUserInfo.Profile.LastName = lastName;

            objUserInfo.FirstName = firstName;
            objUserInfo.LastName = lastName;
            objUserInfo.DisplayName = firstName + " " + lastName;

            UserController.UpdateUser(portalSettings.PortalId, objUserInfo);
            ProfileController.UpdateUserProfile(objUserInfo);
        }

        private int GetOrUpdateDnnFile(PortalSettings portalSettings, UserInfo objUserInfo, string filePath)
        {
            string filename = Path.GetFileNameWithoutExtension(filePath) + Path.GetExtension(filePath);
            string dirPath = Path.GetDirectoryName(filePath).Replace(@"\", "/");
            string part = portalSettings.HomeDirectory;

            int index = dirPath.IndexOf(part, StringComparison.OrdinalIgnoreCase);
            string folderPath = index >= 0
                ? dirPath.Substring(index + part.Length)
            : dirPath;

            IFolderInfo folder = FolderManager.Instance.GetUserFolder(objUserInfo);
            //if (FolderManager.Instance.FolderExists(portalSettings.PortalId, folderPath))
            //    folder = FolderManager.Instance.GetFolder(portalSettings.PortalId, folderPath);
            //else
            //{
            //    var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalSettings.PortalId, "Standard");
            //    folder = FolderManager.Instance.AddFolder(new FolderMappingInfo
            //    {
            //        FolderProviderType = folderMapping.FolderProviderType,
            //        FolderMappingID = folderMapping.FolderMappingID,
            //        Priority = 1,
            //        PortalID = portalSettings.PortalId,
            //    }, folderPath);
            //}

            var file = FileManager.Instance.GetFile(folder.FolderID, filename);
            if (file == null)
            {
                var fileBytes = File.ReadAllBytes(GlobalHelper.MapPath(filePath));
                var stream = new MemoryStream(fileBytes);
                var contentType = MimeMapping.GetMimeMapping(Path.GetFileName(filePath));

                file = FileManager.Instance.AddFile(folder, filename, stream, true, false, contentType);
            }

            return file.FileId;
        }
    }
}
