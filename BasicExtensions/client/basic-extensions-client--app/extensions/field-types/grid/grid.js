function GridController(field, $scope, moduleController, $element) {
    this.init = () => {
        $scope.gridPageSizes = [10, 20, 30, 50, 100];

        if (field.Settings.EnablePaging) {
            field.Settings.PageCount = Math.ceil(field.DataSource.TotalCount / field.Settings.PageSize);
            moduleController.$timeout(() => this.renderPaging());
        }
    }

    this.renderPaging = () => {
        if (field.Settings.PageCount > 1) {
            $(`#bGridPaging${field.FieldID}`).twbsPagination({
                initiateStartPageClick: false,
                totalPages: field.Settings.PageCount,
                visiblePages: field.Settings.VisiblePages || 5,
                startPage: field.Settings.PageIndex || 1,
                paginationClass: 'grid-paging',
                pageClass: 'grid-paging-item',
                activeClass: 'grid-paging-active-item',
                disabledClass: 'grid-paging-disabled-item',
                firstClass: 'grid-paging-first-item',
                prevClass: 'grid-paging-prev-item',
                nextClass: 'grid-paging-next-item',
                lastClass: 'grid-paging-last-item',
                anchorClass: 'grid-paging-link-item',
                first: field.Settings.FirstPageLabel || 'First',
                prev: field.Settings.PreviousPageLabel || 'Previous ',
                next: field.Settings.NextPageLabel || 'Next',
                last: field.Settings.LastPageLabel || 'Last',
                onPageClick: function (event, page) {
                    field.Settings.PageIndex = page + 1;
                    $scope.bGrid_onPageChange(field);
                    if (!$scope.$$phase) $scope.$apply();
                }
            });
        }
    }

    $scope.bGrid_onPageChange = function (field) {
        field.DataSource.Type = 2;
        moduleController.getFieldDataSource(field.FieldID, field.Settings.PageIndex - 1);

        if (field.Settings.SaveCurrentPageInStorage) localStorage.setItem(pageIndexKey, field.Settings.PageIndex);
    };

    //Event dropdown page size change
    $scope.bGrid_onPageSizeChange = function (field, pageSize) {
        $(`#bGridPaging${field.FieldID}`).twbsPagination('destroy');

        field.Settings.PageSize = pageSize;
        field.Settings.PageCount = Math.ceil(field.DataSource.TotalCount / pageSize);

        field.Settings.PageIndex = 1;
        initialPaging = 0;

        $scope.bGrid_onPageChange(field);

        renderPaging();
    };
}