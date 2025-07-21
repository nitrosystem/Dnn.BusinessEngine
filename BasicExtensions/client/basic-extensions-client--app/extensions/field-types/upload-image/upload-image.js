function UploadImageController(field, $scope, moduleController, $element) {
    this.init = () => {
    }

    //#region $scope methods

    $scope.bUploadImage_onUploadChange = (field, $file, $invalidFiles) => {
        if ($invalidFiles && $invalidFiles.length) {
            field.isError = true;
            field.errorMessage = $invalidFiles[0].$error;
        }
        else if ($file) {
            moduleController.globalService.previewImage($file).then(function (data) {
                field.preview = data;
            });

            this.uploadFile(field, $file);
        }
    };

    $scope.bUploadImage_onRemoveImage = function (field) {
        delete field.Value;
    };

    //#endregion

    //#region controller methods

    this.uploadFile = (field, $file) => {
        var params = { file: $file };

        field.isUploading = true;

        moduleController.apiService.uploadImage(params).then((data) => {
            field.Value = data.FilePath;

            delete field.uploadedPercent;
            delete field.preview;
            delete field.isUploading;
        }, (error) => {
            field.isError = true;
            field.isUploaded = false;
            field.errorMessage = error.data && error.data.Message ? error.data.Message : error.data;

            delete field.Value;
            delete field.isUploading;
        }, (evt) => {
            var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
            field.uploadedPercent = progressPercentage;
        });
    }

    //#endregion
}