import { GlobalHelper } from '../helpers';

export class ValidationService {
    constructor($rootScope, notificationService) {
        // Verification flag: true means validation passed, false means failed
        this.valid = false;
        // Verification prompt: default is the first failed field's tip (useful for mobile Toast)
        this.tip = '';
        // Initial flag: true means validator() has never been called
        this.pristine = true;
        // Error set: failed fields and their corresponding messages
        this.error = {};
        // Notification service
        this.notifyService = notificationService;
        this.$rootScope = $rootScope;
    }

    /**
     * Initialize a validation form instance based on a scheme.
     * @param {Object} scheme - Verification plan
     * @param {String|Function|Array} scheme[].rule - Validation rules
     * @param {String} scheme[].id - HTML element id
     * @param {Boolean} scheme[].required - Whether the field is required (default true)
     * @param {String} scheme[].nullTip - Empty value message (default: 'Data cannot be empty')
     * @param {String} scheme[].errorTip - Invalid value message (default: 'Please fill in the correct information')
     * @param {Boolean} watchModel - If true, attach an Angular $watch on the given model name
     * @param {Object} $scope - Angular $scope object
     * @param {String} modelName - Model name to watch
     * @param {Boolean} focusOnElement - Whether to focus the first invalid element
     * @param {Boolean} enableNotification - Whether to display invalid notifications
     */
    init(
        scheme = {},
        watchModel,
        $scope,
        modelName,
        focusOnElement = true,
        enableNotification = false
    ) {
        const form = new ValidationService(this.$rootScope, this.notifyService);
        form.scheme = scheme;
        form.focusOnElement = focusOnElement;
        form.$scope = $scope;
        form.enableNotification = enableNotification;

        if (watchModel && $scope && modelName) {
            $scope.$watch(
                modelName,
                (newVal, oldVal) => {
                    if (form.validated && newVal !== oldVal) {
                        form.validator(newVal, true);
                    }
                },
                true
            );
        }

        return form;
    }

    /**
     * Main validation method for a single field.
     * @param {String|Function|Array|RegExp} rule - Current validation rule(s)
     * @param {String} key - Currently validated field name
     * @param {Object} values - All values being validated (used by custom validators)
     * @returns {Boolean} true if valid, false otherwise
    */
    check(rule, key, values) {
        if (Array.isArray(rule)) {
            return rule.every((item) => this.check(item, key, values));
        }

        const flag = rule(GlobalHelper.getByPath(values, key), values, key);
        // Error message can come from the validator itself, the scheme, or a default
        if (typeof flag === 'string') {
            GlobalHelper.setByPath(this.error, key, flag);
        } else if (!flag) {
            GlobalHelper.setByPath(
                this.error,
                key,
                this.scheme[key].errorTip || 'Please fill in the correct information'
            );
        }

        return flag === true;
    }

    /**
     * Validate a key-value object against the previously defined scheme.
     * @param {Object} values - The values to validate
     * @param {Boolean} disableFocus - If true, do not focus the first invalid element
     * @returns {Object} error - The validation errors
    */
    validator = (values, disableFocus) => {
        const { scheme } = this;
        this.pristine = false;
        this.tip = '';
        this.error = {};
        let firstElementId = '';

        Object.keys(scheme).forEach((key) => {
            const {
                rule,
                nullTip = 'Data cannot be empty',
                required,
            } = scheme[key];

            // When the value is not empty, verify with the rule.
            // Otherwise, if required is true, show the empty-value tip.
            const value = GlobalHelper.getByPath(values, key);
            if ((value !== undefined && value !== null) || typeof rule === 'function') {
                if (rule !== undefined) this.check(rule, key, values);
            } else if (required) {
                GlobalHelper.setByPath(this.error, key, nullTip);
            }

            if (GlobalHelper.getByPath(this.error, key) && !firstElementId && scheme[key].id) {
                firstElementId = scheme[key].id;
            }
        });

        const errorList = Object.values(this.error);
        this.valid = errorList.length === 0;
        if (!this.valid) [this.tip] = errorList;
        if (!disableFocus && this.focusOnElement && !this.valid && firstElementId) {
            const $el = $('#' + firstElementId);
            if ($el.is(':hidden')) {
                $el.addClass('b-hide-elem');
                $el.focus();
                setTimeout(() => $el.removeClass('b-hide-elem'));
            } else {
                $el.focus();
            }
        }

        this.validated = true;
        if (this.enableNotification && this.notifyService) {
            Object.keys(this.error).forEach((key) => {
                this.notifyService.error(`Field '${key}' is not valid!`, 3000);
            });
        }

        return this.error;
    };
}
