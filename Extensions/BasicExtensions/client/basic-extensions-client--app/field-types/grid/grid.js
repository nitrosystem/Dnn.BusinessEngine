ComponentRegistry.register("Grid", function (controller, globalService) {
    this.init = (field) => {
        field.__ignoreReinit = true;

        this.onUpdateRows(field);

        field.onRowButtonClick = this.onRowButtonClick;
        field.onUpdateRows = this.onUpdateRows;
    }

    this.onUpdateRows = async (field, reloadItems) => {
        if (reloadItems) await controller.callAction(field.Id, null, 'OnDataLoading');

        if (field.DataSource.VariableName) {
            const rows = controller.get(field.DataSource.VariableName);
            this._processRows(field, rows);

            if (reloadItems) {
                globalService.nextMicroTask(() => {
                    field._ignoreWatch = false;
                    controller.notify(`field.${field.FieldName}.rows`);
                });
            }

            controller.listenTo(field.DataSource.VariableName, (value) => {
                if (!field._ignoreWatch) this._processRows(field, value);
            });
        }
    }

    this.onRowButtonClick = async (field, row, column, element) => {
        const rowIndex = field.rows.indexOf(row);
        const extraPrams = {};
        const childScope = Object.create(controller.scope);
        childScope.row = row;


        if (column.ActionId) {
            for (const param of column.ActionParams ?? []) {
                extraPrams[param.ParamName] = controller.get(param.ParamValue, childScope);
            }

            if (column.ShowConfirmAlert && confirm(column.ConfirmAlertText)) {
                element.disabled = true;
                await controller.callAction(field.Id, column.ActionId, 'OnCustomAction', extraPrams, { row });
                element.disabled = false;
            }
            else {
                element.disabled = true;
                await controller.callAction(field.Id, column.ActionId, 'OnCustomAction', extraPrams, { row });
                element.disabled = false;
            }
        }
    }

    this._processRows = (field, rows) => {
        for (const row of rows ?? []) {
            const columns = globalService.clone(field.Settings.Columns ?? []);
            for (const column of columns) {
                if (column.ColumnType === 'ConditionalContents') {
                    for (const item of column.ContentItems ?? []) {
                        item.Conditions = item.Conditions?.trim()
                            ? controller.checkConditions(item.Conditions, { row })
                            : true;

                        item.Content = item.Conditions
                            ? (item.Content?.startsWith('row.') ? controller.get(item.Content, { row }) : item.Content)
                            : ''
                    }
                }
            }

            row.columns = columns;
        }

        field._ignoreWatch = true;
        field.rows = rows;

        this._renderPaging(field);
        controller.notify(`field.${field.FieldName}.rows`);
        globalService.nextMicroTask(() => {
            field._ignoreWatch = false;
        });
    }

    this._renderPaging = (field) => {
        if (!field.Settings.EnablePaging) return;

        const totalCount = controller.get(field.Settings.TotalCount, controller.scope) || 0;
        const pageIndex = controller.get(field.Settings.PageIndex, controller.scope) || 1;
        const pageSize = controller.get(field.Settings.PageSize, controller.scope) || 10;
        const pageCount = Math.ceil(totalCount / pageSize);

        if (field.$paging)
            field.$paging.twbsPagination('destroy');

        if (pageCount > 1) {
            field.$paging = $(`#bGridPaging${field.Id}`).twbsPagination({
                totalPages: pageCount,
                visiblePages: field.Settings.VisiblePages || 5,
                startPage: typeof pageIndex === 'string' ? parseInt(pageIndex) : pageIndex,
                pageClass: field.Settings.PageClass || 'page-item',
                first: field.Settings.FirstPageLabel || 'اولین',
                prev: field.Settings.PreviousPageLabel || 'قبلی',
                next: field.Settings.NextPageLabel || 'بعدی',
                last: field.Settings.LastPageLabel || 'آخرین',
                initiateStartPageClick: false,
                onPageClick: async (event, page) => {
                    field.isLoading = true;
                    if (typeof (field.Settings.PageIndex) === 'string')
                        controller.set(field.Settings.PageIndex, page);

                    await controller.callAction(field.Id, null, 'OnDataLoading');

                    field.isLoading = false;
                }
            });
        }
    }
});
