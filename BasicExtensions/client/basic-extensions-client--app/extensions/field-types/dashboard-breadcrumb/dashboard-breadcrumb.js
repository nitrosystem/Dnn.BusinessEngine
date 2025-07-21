function DashboardBreadcrumbController(field, $scope, moduleController, $element) {
    this.init = function() {
        $scope.DashboardBreadcrumb = {};
        raiseElement(field);
    }

    $scope.$watch('currentPageId', function(newVal, oldVal) {
        if (newVal != oldVal)
            raiseElement(field);
    });

    function raiseElement(field) {
        // Parse Current Page
        if ($scope.currentPage) {
            $scope.DashboardBreadcrumb.CurrentPage = {
                Title: $scope.currentPage.Title,
                Icon: $scope.currentPage.Settings ? $scope.currentPage.Settings.Icon : ''
            };

            //Parse Page Parents Path
            $scope.DashboardBreadcrumb.ParentsPath = getPageParentsPath($scope.currentPage);

            const $breadcrumb = moduleController.$compile('<span>' + field.Settings.BreadcrumbTemplate + '</span>')($scope);
            $(`#bDashboardBreadcrumb_${field.FieldName}`).html($breadcrumb);
        }
    }

    function getPageParentsPath(page) {
        var parents = [
            { PageId: page.PageId, Title: page.Title, Icon: page.Settings ? page.Settings.Icon : '' }
        ];

        const findParents = (parentID) => {
            const parent = getPageByPageId(parentID);

            parents.push({ PageId: parent.PageId, Title: parent.Title, Icon: parent.Settings ? parent.Settings.Icon : '' });

            if (parent.ParentID)
                findParents(parent.ParentID);
            else
                return parents
        }

        if (!page.ParentID)
            return parents;
        else
            return findParents(page.ParentID);
    }

    function getPageByPageId(pageId) {
        var result;

        const findNestedPage = (pages) => {
            _.forEach(pages, (p) => {
                if (p.PageId == pageId) result = p;
                else return findNestedPage(p.Pages);
            });
        };

        findNestedPage($scope.pages);

        return result;
    }

    $scope.onDashboardMenuClick = (pageId) => {
        $scope.$emit('onGotoDashboardPage', { pageId: pageId, isUpdatePageParams: true });
    }
}