import Swal from 'sweetalert2';
import { GlobalUtil } from '../../../shared/utils';
import { UrlHelper } from '../../../shared/helpers';

export class CreateGroupController {
    constructor($rootScope, $scope, validationService, apiService, notificationService) {
        'ngInject';

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.validationService = validationService;
        this.apiService = apiService;
        this.notifyService = notificationService;

        $rootScope.$on('onEditGroup', (e, args) => {
            this.group = args?.group ?? {};

            this._setForm();
        });

        $rootScope.$on('onDeleteGroup', (e, args) => {
            if (args?.groupId) this.onDeleteGroupClick(args.groupId);
        });
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');

        this.running = 'get-group';
        this.awaitAction = {
            title: 'Loading group',
            subtitle: 'Just a moment for loading group...',
        };

        this.apiService.get('Studio', 'GetGroup', { groupId: id }).then((data) => {
            this.group = data;

            this._setForm();

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

    onSaveGroupClick() {
        this.form.validated = true;
        this.form.validator(this.group);
        if (this.form.valid) {
            this.running = 'save-group';
            this.awaitAction = {
                title: 'Creating Group',
                subtitle: 'Just a moment for creating sidebar explorer group...',
            };

            this.apiService.post('Studio', 'SaveGroup', this.group).then((data) => {
                if (!this.group.Id)
                    this.notifyService.success('Group has been created successfully.');
                else
                    this.notifyService.info('Group has been updated successfully.');

                this.group.Id = data;

                this.$scope.$emit('onGroupCreated', { group: this.group });

                this.running = null;
                this.awaitAction = null;
            }, (error) => {
                this.notifyService.error(error.data);

                this.running = null;
            });
        }
    }

    onDeleteGroupClick(groupId) {
        groupId = groupId ?? this.group.Id;

        let timerInterval;
        Swal.fire({
            title: 'Are you sure?',
            html: '<p>Once deleted, you will not be able to recover this imaginary group!</p><b></b>',
            icon: 'warning',
            timer: 5000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!',
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
                this.running = 'delete-group';
                this.awaitAction = {
                    title: 'Delete Group',
                    subtitle: 'Just a moment for deleting group...',
                };

                this.apiService.post('Studio', 'DeleteGroup', { Id: groupId }).then((data) => {
                    if (data) {
                        this.notifyService.info('Group has been deleted successfully.');
                        this.$scope.$emit('onGroupIsDeleted', { groupId });
                    }

                    this.running = null;
                    this.awaitAction = null;
                }, (error) => {
                    this.notifyService.error(error.data);

                    this.running = null;
                });
            }
        });
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            GroupType: {
                id: 'drpGroupType',
                required: true,
            },
            GroupName: {
                id: 'txtGroupName',
                required: true,
            },
        },
            true,
            this.$scope,
            '$.group'
        );
    }

    //#endregion
}
