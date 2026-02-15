export class ExtensionsController {
    constructor(
        $scope,
        $rootScope,
        $timeout,
        Upload,
        studioService,
        globalService,
        apiService,
        notificationService) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.uploadService = Upload;
        this.globalService = globalService;
        this.apiService = apiService;
        this.notifyService = notificationService;

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        $scope.$emit('onChangeActivityBar', {
            name: 'extensions',
            title: 'Extensions',
            disableActivityBarCallback: true
        });

        $rootScope.$on('onListenToPushingServer', (e, args) => {
            debugger
            if (args.type == 'InstallExtension') {
                this.notifyExtensionInstallStatus(args.message, args.percent);
            }
        });

        this.onPageLoad();
    }

    onPageLoad() {
        this.running = "get-extensions";
        this.awaitAction = {
            title: "Loading Extensions",
            subtitle: "Just a moment for loading extensions...",
        };

        this.apiService.get("Studio", "GetExtensions").then((data) => {
            this.extensions = data.Extensions;
            this.availableExtensions = data.AvailableExtensions;

            this.onFocusModule();

            delete this.running;
            delete this.awaitAction;
        }, (error) => {
            if (error.status == 401) this.$rootScope.$broadcast('onUnauthorized401', { error: error }); // if user is logoff then refresh page for redirect to login page

            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            delete this.running;
        });
    }

    onFocusModule() {
        this.$scope.$emit('onChangeActivityBar', { name: 'extensions' })
    }

    onInstallAvailableExtension(item, $index) {
        this.running = "install-available-extensions";
        this.awaitAction = {
            title: `Unzip & Ready ${item.extensionFile}`,
            subtitle: `Just a moment for unzip ${item.extensionName} file and ready to install...`,
            extIndex: $index
        };

        this.apiService.post("Studio", "InstallAvailableExtension", item).then((data) => {
            this.workingMode = "install-extension";
            this.$scope.$emit("onShowRightWidget");
            this.extInstalingStep = 4;

            this.availableExtensions.splice($index, 1);

            delete this.running;
            delete this.awaitAction;
        }, (error) => {
            if (error.status == 401) this.$rootScope.$broadcast('onUnauthorized401', { error: error }); // if user is logoff then refresh page for redirect to login page

            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            delete this.running;
        });
    }

    onInstallExtensionClick() {
        this.workingMode = "install-extension";
        this.$scope.$emit("onShowRightWidget");

        this.extInstalingStep = 1;
    }

    onUploadExtensionPackage($files, $file, $newFiles, $duplicateFiles, $invalidFiles, $event) {
        if ($file) {
            this.running = "upload-extension";
            this.awaitAction = {
                title: "Uploading Extensions",
                subtitle: "Just a moment for uploading extension...",
                showProgress: true
            };

            this.apiService.uploadFile('Studio', 'UploadExtensionPackage', { files: $file }).then((data) => {
                this.extension = JSON.parse(data.ManifestJson);
                this.extractPath = data.ExtractPath;
                this.extInstalingStep = 2;

                delete this.running;
                delete this.awaitAction;
            }, (error) => {
                if (error.status == 401) this.$rootScope.$broadcast('onUnauthorized401', { error: error }); // if user is logoff then refresh page for redirect to login page

                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                delete this.running;
            }, (evt) => {
                var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
                $('.progress-bar').css('width', progressPercentage + '%')
            });
        }
    }

    onInstallExtensionStepClick() {
        this.extInstalingStep = 3;

        this.running = "install-extensions";
        this.awaitAction = {
            title: "Install Extension",
            subtitle: "Just a moment for installing extension...",
            showProgress: true,
        };

        this.apiService.post("Studio", "InstallExtension",
            {
                Channel: this.$rootScope.scenario.ScenarioName,
                ExtractPath: this.extractPath
            }).then((data) => {
                this.extInstalingStep = 4;

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                if (error.status == 401) this.$rootScope.$broadcast('onUnauthorized401', { error: error }); // if user is logoff then refresh page for redirect to login page

                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                $('#progressLog').append(error.data.Message + '\n');

                clearInterval(this.monitoringTimer);
                this.monitoringTimer = 0;

                delete this.running;
            });
    }

    onDoneInstallExtensionClick() {
        location.reload();
    }

    onCancelInstallExtensionClick() {
        location.reload();
    }

    onCloseWindow() {
        location.reload();
    }

    notifyExtensionInstallStatus(message, percent) {
        $('#progressLog').append(`<li>${message}</li>`)
        $('#progressLog').scrollTop($('#progressLog')[0].scrollHeight);

        $('.progress-bar').css('width', percent + '%');
    }
}