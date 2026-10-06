import { GlobalHelper } from '../../helpers';
import template from './template.html';

class SelectiveController {
    constructor($scope, $rootScope, $timeout) {
        'ngInject';

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
    }

    $onInit() {
        this.id = GlobalHelper.generateGuid();
        this.ngModel.$render = () => {
            if (this.ngModel.$viewValue) {
                this.onFetchItems(this.filter).then((data) => this.onUpdatedItems(data));
            }
            else {
                this.onFetchItems(this.filter).then((data) => this.onUpdatedItems(data));
            }
        };

        this.paging.onPageClick = (e, pageIndex) => {
            this.filter.pageIndex = pageIndex;
            this.onFetchItems(this.filter).then((data) => this.onUpdatedItems(data));
        };
    }

    onUpdatedItems(data) {
        this.paging.totalPages = data.Page.PageCount
        this.paging.startPage = data.Page.PageIndex;
        this.items = data[this.itemsKey];

        if (this.ngModel.$viewValue && !this.selected) this._fillSelected()
    }

    onShowModal() {
        $(`#wnSelectiveModal${this.id}`).modal('show');
        this.$timeout(() => this.$scope.$broadcast(`onFocusModalInput${this.id}`));
    }

    onSearchClick() {
        this.items = null;
        this.$timeout(() => {
            this.filter.pageIndex = 1;
            this.onFetchItems(this.filter).then((data) => this.onUpdatedItems(data));
        });
    }

    onSelectItemClick(item) {
        this.ngModel.$setViewValue(item[this.value]);
        this.selected = { text: item[this.text], value: item[this.value] };
        $(`#wnSelectiveModal${this.id}`).modal('hide');
        this.onSelectItem({ item: item });
    }

    _fillSelected() {
        const item = this.items.find(i => i[this.value] === this.ngModel.$viewValue)
        if (item) this.selected = { text: item[this.text], value: item[this.value] };
    }
}

const SelectiveComponent = {
    require: {
        ngModel: '^ngModel'
    },
    bindings: {
        placeholder: '@',
        itemsKey: '@',
        text: '@',
        value: '@',
        table: '<',
        paging: '<',
        modal: '<',
        filter: '<',
        onFetchItems: '&',
        onSelectItem: '&'
    },
    controller: SelectiveController,
    controllerAs: '$',
    templateUrl: template
};

export default SelectiveComponent;