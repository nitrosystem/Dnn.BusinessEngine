export class NotificationService {
    constructor($rootScope, $sce, $compile, $timeout) {
        'ngInject';
        this.$rootScope = $rootScope;
        this.$sce = $sce;
        this.$compile = $compile;
        this.$timeout = $timeout;
    }

    static NOTIFICATION_TEMPLATE = `
        <div class="notification-item notify-{{type}}">
            <div class="notify-container">
                <i class="notify-icon codicon codicon-{{icon}}"></i>
                <div class="notify-text-wrapper">
                    <p class="notify-text">{{text}}</p>
                </div>
                <span class="notify-close" ng-click="onCloseNotifyClick()">
                    <i class="codicon codicon-close"></i>
                </span>
            </div>
            <div class="notify-progress-bar" ng-class="{'is-started':notify.isVisible}"></div>
        </div>
    `;

    /**
     * Display a notification.
     * @param {String} type - 'success' | 'info' | 'warning' | 'error'
     * @param {String} text - The text (HTML allowed, trusted via $sce)
     * @param {String} icon - Codicon name
     * @param {object} [options] - Notification options
    */
    notify(type, text, icon, options) {
        const scope = this.$rootScope.$new();
        scope.type = type;
        scope.icon = icon;
        scope.text = this.$sce.trustAsHtml(text);
        const $notificationElement = this.$compile(NotificationService.NOTIFICATION_TEMPLATE)(scope);
        $('#bNotifications').append($notificationElement);
        scope.onCloseNotifyClick = () => $notificationElement.remove();

        // Trigger enter animation on next tick
        this.$timeout(() => {
            $notificationElement.addClass('is-visible');
            $notificationElement.find('.notify-progress-bar').addClass('b-w0');
        }, 100);

        // Auto-dismiss after the configured delay
        this.$timeout(() => $notificationElement.remove(), options.delay);
    }

    success(text) {
        this.notify('success', text, 'check-all', { delay: 3000 });
    }

    build(text) {
        this.notify('build', text, 'run-all-coverage', { delay: 2000 });
    }

    info(text) {
        this.notify('info', text, 'check-all', { delay: 3000 });
    }

    warning(text) {
        this.notify('warning', text, 'warning', { delay: 5000 });
    }

    error(text) {
        this.notify('error', text, 'bug', { delay: 10000 });
    }
}
