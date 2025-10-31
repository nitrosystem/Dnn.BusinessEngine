import template from "./template.html";

class SelectiveController {
    constructor($scope, $rootScope, $timeout, globalService) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.globalService = globalService;
        this.$timeout = $timeout;
    }

    $onInit() {
        this.id = this.globalService.generateGuid();
        this.filter = {};

        this.ngModel.$render = () => {
            if (this.ngModel.$viewValue) {
                this.filter = this.storage[this.storageKey].SelectiveData || {};
                this.onFetchItems({ pageIndex: this.filter.pageIndex ?? 1, searchText: this.filter.searchText ?? '' }).then((data) => this.onUpdatedItems(data));
            }
            else this.onFetchItems({ pageIndex: 1, searchText: '' }).then((data) => this.onUpdatedItems(data));
        };

        let $this = this;
        this.paging.onPageClick = (e, pageIndex) => {
            $this.filter.pageIndex = pageIndex ;
            this.onFetchItems({ pageIndex: $this.filter.pageIndex, searchText: $this.filter.searchText }).then((data) => $this.onUpdatedItems(data));
        };
    }

    onUpdatedItems(data) {
        this.paging.totalPages = data.PageCount
        this.paging.startPage = data.PageIndex + 1;
        this.items = data.Services;

        if (this.ngModel.$viewValue && !this.selected) this.fillSelected()
    }

    onShowModal() {
        $(`#wnSelectiveModal${this.id}`).modal('show');

        this.$timeout(() => {
            this.$scope.$broadcast(`onFocusModalInput${this.id}`);
        });
    }

    fillSelected() {
        _.filter(this.items, (i) => { return i[this.value] == this.ngModel.$viewValue }).map((item) => {
            this.selected = { text: item[this.text], value: item[this.value] };
        });
    }

    onSearchTextChange() {
        delete this.items;

        this.$timeout(() => {
            this.filter.pageIndex = 1;
            this.onFetchItems({ pageIndex: this.filter.pageIndex, searchText: this.filter.searchText }).then((data) => this.onUpdatedItems(data));
        });
    }

    onResetClick() {
        delete this.items;
        this.filter = { pageIndex: 1, searchText: '' };
        this.paging.startPage = 1;

        this.$timeout(() => {
            this.onFetchItems({ pageIndex: this.filter.pageIndex, searchText: this.filter.searchText }).then((data) => this.onUpdatedItems(data));
        });
    }

    onSelectItemClick(item) {
        this.ngModel.$setViewValue(item[this.value]);
        this.selected = { text: item[this.text], value: item[this.value] };

        if (this.storage) {
            if (!this.storage[this.storageKey]) this.storage[this.storageKey] = {};
            this.storage[this.storageKey].SelectiveData = this.filter;
        }

        $(`#wnSelectiveModal${this.id}`).modal('hide');

        this.onSelectItem({ item: item });
    }
}

const SelectiveComponent = {
    require: {
        ngModel: '^ngModel'
    },
    bindings: {
        placeholder: '@',
        text: '@',
        value: '@',
        table: '<',
        paging: '<',
        items: '<',
        modal: '<',
        storage: '=',
        storageKey: '@',
        onFetchItems: '&',
        onSelectItem: '&'
    },
    controller: SelectiveController,
    controllerAs: "$",
    templateUrl: template
};

export default SelectiveComponent;