function UploadFileController(field, $scope, moduleController, $element) {
    this.init = () => {
        if (field.Value) {
            field.Files = angular.copy(field.Value);
        }
    }

    $scope.bUploadFile_onUploadFile = function (field, $files, $file, $newFiles, $duplicateFiles, $invalidFiles, $event) {
        if ($files.length || $newFiles.length || $duplicateFiles.length || $invalidFiles.length) {
            field.Files = [];
            field.Value = null;

            angular.forEach($invalidFiles, function (f) {
                field.Files.push({
                    FileName: f.name,
                    FileType: f.type,
                    FileSize: f.size,
                    IsError: true,
                    IsUploaded: false,
                    Message: f.$error,
                });
            });

            angular.forEach($files, function (f) {
                field.Files.push({
                    File: f,
                    FileName: f.name,
                    FileType: f.type,
                    FileSize: f.size,
                    IsError: false,
                    IsUploaded: false,
                    Message: f.$error,
                });
            });

            angular.forEach(field.Files, function (f) {
                if (!f.IsError) {
                    uploadFile(field, f);
                }
            });
        }
    };

    function uploadFile(field, file) {
        var headers = {
            'Content-Type': file.File.type,
            Requestverificationtoken: $('[name="__RequestVerificationToken"]').val()
        }

        $scope.waiting = true;

        field.Settings.IsUploading = true;

        var url = window.bEngineGlobalSettings.apiBaseUrl + 'BusinessEngine/API/Common/UploadFile';

        var params = { files: file.File, FileUploadType: 1 };

        return moduleController.uploadService.upload({
            url: url,
            headers: headers,
            data: params,
        }).then(function (data) {
            if (data.status === 200 && data.data) {
                field.Value = field.Settings.AllowUploadMultipleFile ? (field.Value || []) : (field.Settings.IsJsonValue ? {} : '');

                file.FilePath = data.data.FilePath;

                if (field.Settings.AllowUploadMultipleFile)
                    field.Value.push(field.IsJsonValue ? file : data.data.FilePath);
                else
                    field.Value = field.IsJsonValue ? file : data.data.FilePath;

                file.IsError = false;
                file.IsUploaded = true;
                file.Message = 'Upload file successfully'
            } else {
                file.IsError = true;
                file.IsUploaded = false;
            }

            delete $scope.waiting;
            delete field.Settings.IsUploading;
        }, function (error) {
            file.IsError = true;
            file.IsUploaded = false;
            file.Message = error.data && error.data.Message ? error.data.Message : error.data;

            delete $scope.waiting;
            delete field.Settings.IsUploading;
        }, function (data) {
            var progressPercentage = parseInt(100.0 * data.loaded / data.total);
            file.Progress = progressPercentage;

            field.Settings.UploadedPercent = progressPercentage;
        });
    }

    $scope.bUploadFile_onRemoveFile = function (field, $index) {
        field.Files.splice($index, 1);
    };
}