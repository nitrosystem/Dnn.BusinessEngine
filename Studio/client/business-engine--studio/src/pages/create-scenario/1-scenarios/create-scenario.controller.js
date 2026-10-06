import Swal from 'sweetalert2'
import { GlobalHelper, UrlHelper } from '../../../shared/helpers';
import { GlobalUtil } from '../../../shared/utils';

export class CreateScenarioController {
    constructor(
        $rootScope,
        $scope,
        $timeout,
        apiService,
        validationService,
        notificationService,
        baseOptions
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.baseOptions = baseOptions;

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        this.scenario = GlobalHelper.clone(this.$rootScope.scenario);
        this._setForm();
    }

    onSetValidNameClick() {
        this.scenario.ScenarioName = GlobalHelper.normalizeName(this.scenario.ScenarioName);
    }

    onSetValidDbObjectPrefixClick() {
        this.scenario.DatabaseObjectPrefix = this._sanitizeDatabasePrefix(this.scenario.DatabaseObjectPrefix);
    }

    onSaveScenarioClick() {
        this.form.validated = true;
        this.form.validator(this.scenario);
        if (this.form.valid) {
            this.running = 'save-scenario';
            this.awaitAction = {
                title: 'Saving Scenario',
                subtitle: 'Just a moment for saving the scenario...',
            };

            this.apiService.post('Studio', 'SaveScenario', this.scenario).then((data) => {
                if (!this.scenario.Id)
                    this.notifyService.success('Scenario has been created successfully.');
                else
                    this.notifyService.info('Scenario has been updated successfully.');

                const isNewScenario = !this.scenario.Id;
                this.scenario = data;
                this.$rootScope.scenario = GlobalHelper.clone(this.scenario);
                if (isNewScenario) {
                    this.$rootScope.tabs = [];
                    let url = this.baseOptions.baseUrl + '/?s=' + this.scenario.ScenarioName;
                    if (this.$rootScope.afterCreatingScenario) {
                        url += '&m=' + this.$rootScope.afterCreatingScenario.page ?? '';
                        url += '&d=' + this.$rootScope.afterCreatingScenario.sitePageId ?? '';
                        url += '&ru=' + this.$rootScope.afterCreatingScenario.backUrl ?? '';
                    }
                    location.href = url;
                }

                this.awaitAction = null;
                this.running = null;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                this.notifyService.error(error.data);

                this.running = null;
            });
        }
    }

    onDeleteScenarioClick() {
        let timerInterval;
        Swal.fire({
            title: '<h4>Are you sure remove scenario and child items?</h4><b></b>',
            html: '<p>Be aware that this operation is highly risky and irreversible. Upon confirmation, the entire scenario and all its dependencies will be permanently deleted.</p>',
            icon: 'warning',
            timer: 20000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, Remove it!',
            backdrop: false,
            didOpen: () => {
                const timer = Swal.getPopup().querySelector('b');
                timerInterval = setInterval(() => {
                    timer.textContent = `${Swal.getTimerLeft()}`;
                }, 100);
            },
            willClose: () => {
                clearInterval(timerInterval);
            }
        }).then((result) => {
            if (result.isConfirmed) {
                this.running = 'delete-scenario';
                this.awaitAction = {
                    title: 'Delete Scenario',
                    subtitle: 'Just a moment for deleting the scenario and child items...',
                };

                this.apiService.post('Studio', 'DeleteScenario', { Id: this.scenario.Id }).then((data) => {
                    if (data) {
                        this.notifyService.success('Scenario has been deleted successfully.');
                        location.reload();
                    }

                    this.awaitAction = null;
                    this.running = null;
                }, (error) => {
                    this.awaitAction.isError = true;
                    this.awaitAction.subtitle = error.statusText;
                    this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                    this.notifyService.error(error.data);

                    this.running = null;
                });
            }
        });
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            ScenarioName: {
                id: 'txtScenarioName',
                rule: (value) => {
                    if (/^[A-Za-z][A-Za-z0-9_]*$/.test(value) === false)
                        return 'Scenario name is not valid';
                    else
                        return true;
                },
                required: true,
            },
            ScenarioTitle: {
                id: 'txtScenarioTitle',
                required: true,
            },
            DatabaseObjectPrefix: {
                id: 'txtDatabaseObjectPrefix',
                rule: (value) => {
                    if (/^[a-z]{1,9}_$/.test(value) === false)
                        return 'Database object prefix is not valid. Example: store_';
                    else
                        return true;
                },
                required: true,
            },
        },
            true,
            this.$scope,
            '$.scenario'
        );
    }

    _sanitizeDatabasePrefix(input) {
        if (!input) return '';
        let clean = input
            .toLowerCase()
            .replace(/[^a-z]/g, '');
        clean = clean.slice(0, 9);
        if (!clean.endsWith('_')) {
            clean += '_';
        }
        return clean;
    }

    //#endregion
}
