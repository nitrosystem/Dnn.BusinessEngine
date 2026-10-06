import { GlobalUtil } from '../../shared/utils';

export class ExtensionsController {
    constructor(
        $rootScope,
        $scope,
        $timeout,
        apiService,
        notificationService,
        baseOptions) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.apiService = apiService;
        this.notifyService = notificationService;
        this.baseOptions = baseOptions;

        $rootScope.$on('onListenToPushingServer', (e, args) => {
            if (args.type === 'InstallExtension')
                this._notifyExtensionInstallStatus(args.message, args.percent);
        });

        $scope.$emit('onChangeActivityBar', {
            name: 'extensions',
            title: 'Extensions',
            disableActivityBarCallback: true
        });

        const currentTabKey = this.$rootScope.currentTab.key;
        $scope.$on(`onTab--${currentTabKey}--Selected`, (e, args) => {
            this.onFocusModule();
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        this.running = 'get-extensions';
        this.awaitAction = {
            title: 'Loading Extensions',
            subtitle: 'Just a moment for loading extensions...',
        };

        this.apiService.get('Studio', 'GetExtensions').then((data) => {
            this.extensions = data.Extensions;
            this.availableExtensions = data.AvailableExtensions;

            this.onFocusModule();

            this.running = null;
            this.awaitAction = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.running = null;
        });
    }

    onFocusModule() {
        this.$scope.$emit('onChangeActivityBar', { name: 'extensions' })
    }

    onInstallAvailableExtensionClick(extensionFilename, $index) {
        this.running = 'install-available-extensions';
        this.awaitAction = {
            title: `Unzip & Ready ${extensionFilename}`,
            subtitle: `Just a moment for unzip ${extensionFilename} file and ready to install...`
        };

        this.apiService.get('Studio', 'GetAvailableExtension', { extensionFilename }).then((data) => {
            this.extension = data.Manifest;
            this.extractPath = data.ExtractPath;
            this.isAvailableExtension = true;
            this.availableExtensionFileName = extensionFilename;
            this.extInstallingStep = 2;
            this.workingMode = 'install-extension';
            this.$scope.$emit('onShowRightWidget');

            this.availableExtensions.splice($index, 1);

            this.running = null;
            this.awaitAction = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.running = null;
        });
    }

    onInstallExtensionClick() {
        this.extInstallingStep = 1;
        this.workingMode = 'install-extension';
        this.$scope.$emit('onShowRightWidget');

        setTimeout(() => {
            const input = document.getElementById('extensionFileInput');
            if (input && !input.__eventRegistered) {
                input.__eventRegistered = true;
                input.addEventListener('change',
                    (e) => this.$scope.$apply(() => this.onUploadExtensionPackage(e)));
            }
        });
    }

    onUploadExtensionPackage(e) {
        const file = e.target.files[0];
        if (!file) return;

        if (!['application/zip', 'application/x-zip-compressed'].includes(file.type)) {
            throw new Error(`File type is not allowed. Allowed types: zip file`);
        }

        this.running = 'upload-extension';
        this.awaitAction = {
            title: 'Uploading Extensions',
            subtitle: 'Just a moment for uploading extension...',
            showProgress: true
        };

        this.apiService.upload('Studio', 'UploadExtensionPackage', file,
            {
                'Content-Type': undefined
            },
            (percent) => {
                $('.progress-bar').css('width', percent + '%');
            }
        ).then((data) => {
            this.extension = data.Manifest;
            this.extractPath = data.ExtractPath;
            this.extInstallingStep = 2;

            this.running = null;
            this.awaitAction = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    onInstallExtensionStepClick() {
        this.extInstallingStep = 3;
        this.running = 'install-extensions';
        this.awaitAction = {
            title: 'Install Extension',
            subtitle: 'Just a moment for installing extension...',
            showProgress: true,
        };

        this.apiService.post('Studio', 'InstallExtension',
            {
                Channel: this.baseOptions.scenarioName,
                ExtractPath: this.extractPath,
                IsAvailableExtension: this.isAvailableExtension,
                AvailableExtensionFileName: this.availableExtensionFileName
            }).then((data) => {
                this.extInstallingStep = 4;

                this.awaitAction = null;
                this.running = null;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                this.notifyService.error(error.data);
                $('#progressLog').append(error.data.Message + '\n');

                this.running = null;
            });
    }

    onDoneInstallExtensionClick() {
        location.reload();
    }

    onCancelInstallExtensionClick() {
        this.$scope.$emit('onHideRightWidget');
        this.$timeout(() => this.workingMode = null, 200);
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#endregion

    //#region Private Methods

    _notifyExtensionInstallStatus(message, percent) {
        $('#progressLog').append(`<li>${message}</li>`)
        $('#progressLog').scrollTop($('#progressLog')[0].scrollHeight);
        $('.progress-bar').css('width', percent + '%');
    }

    //#endregion
}