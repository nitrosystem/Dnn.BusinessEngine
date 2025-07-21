window.bEngineApp.directive('bGridRowItem', function ($compile, $q, globalService, expressionService, actionService) {
    return {
        restrict: "EA",
        scope: { module: "=", row: "=", column: "=" },
        link: (scope, element, attrs) => {
            const module = scope.module;
            const row = scope.row;
            const column = scope.column;

            function raiseElement() {
                scope.Row = row;
                var content = column.Content || "";
                globalService.parseJsonItems(row);

                var showColumn = column.ShowConditions && column.ShowConditions.length ?
                    (showColumn = expressionService.checkConditions(column.ShowConditions, scope, true)) :
                    true;

                if (!showColumn) {
                    $(element).replace("<span></span>");
                    return;
                }

                if (column.ColumnType == "ActionButton") {
                    content =
                        `<a href="" {tooltip} ng-click="bActionButton_onClick(row,column)" class="${column.ButtonCssClass ?? ''}">
                            {text}
                            {icon}
                        </a>`
                            .replace(/{text}/gm, column.ButtonText ? column.ButtonText : "")
                            .replace(
                                /{icon}/gm,
                                column.ButtonIcon ? `<i class="${column.ButtonIcon}"></i>` : ""
                            )
                            .replace(
                                /{tooltip}/gm,
                                column.TooltipTitle ?
                                    `b-custom-tooltip title="${column.TooltipTitle}"` :
                                    ""
                            );
                } else if (column.ColumnType == "ActionButtonList") {
                    if (column.DisplayType == "IconList") {
                        content = "{ActionItems}";
                    } else if (column.DisplayType == "DropdownMenu") {
                        content = `<div class="dropdown">
                              <button type="button" class="btn btn-light-primary btn-icon btn-sm" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                  <i class="ki ki-bold-more-hor"></i>
                              </button>
                              <div class="dropdown-menu">
                                  {ActionItems}
                              </div>
                            </div>`;
                    }
                    var items = [];
                    var index = 0;
                    _.forEach(column.ActionItems, (item) => {
                        const isValid = expressionService.checkConditions(
                            item.Conditions,
                            scope,
                            true
                        );
                        if (isValid) {
                            const value =
                                `<a href="" ng-click="bActionButton_onClick(row,column.ActionItems[${index}])" class="${item.CssClass ?? ''}">
                                    {text}
                                    {icon}
                                </a>`
                                    .replace(
                                        /{text}/gm,
                                        item.Text ?
                                            expressionService.parseExpression(
                                                item.Text,
                                                scope,
                                                "",
                                                true
                                            ) :
                                            ""
                                    )
                                    .replace(
                                        /{icon}/gm,
                                        item.Icon ? `<i class="${item.Icon}"></i>` : ""
                                    );

                            items.push(value);
                        }

                        index++;
                    });
                    content = content.replace("{ActionItems}", items.join(""));
                } else if (column.ColumnType == "DashboardPageLink") {
                    content =
                        `<a href="" {tooltip} b-dashboard-link page="${column.PageName}" params="${column.PageParams}" class="${column.ButtonCssClass ?? ''}">
                            {icon}
                        </a>`
                            .replace(
                                /{icon}/gm,
                                column.ButtonIcon ? `<i class="${column.ButtonIcon}"></i>` : ""
                            )
                            .replace(
                                /{tooltip}/gm,
                                column.TooltipTitle ?
                                    `b-custom-tooltip title="${column.TooltipTitle}"` :
                                    ""
                            );
                } else if (column.ColumnType == "ConditionalContents") {
                    var items = [];
                    _.forEach(column.ContentItems, (item) => {
                        const isValid = expressionService.checkConditions(
                            item.Conditions,
                            scope,
                            true
                        );
                        if (isValid) {
                            const value = expressionService.parseExpression(
                                item.Content,
                                scope,
                                "",
                                true
                            );
                            items.push(
                                `<span class="${item.CssClass ?? ''}">
                                    {icon}
                                    ${value ? value : ""}
                                </span>`.replace(
                                    /{icon}/gm, item.Icon ? `<i class="${item.Icon}"></i>` : ""
                                )
                            );
                        }
                    });
                    content = items.join("");
                } else if (column.ColumnType == "BindToProperty") {
                    if (column.DataType == "Number") {
                        var value = expressionService.parseExpression(content, scope, "", true) || "";
                        if (column.ShowCommasSeparator && value) {
                            const number = _.toNumber(value);
                            value = number.toLocaleString();
                        }

                        content = `<span class="${column.CssClass ?? ''}">
                                        {icon}
                                        ${value}
                                    </span>`.replace(
                            /{icon}/gm,
                            column.Icon ? `<i class="${column.Icon}"></i>` : ""
                        );
                    } else if (column.DataType == "DateTime") {
                        const value = expressionService.parseExpression(
                            content,
                            scope,
                            "",
                            true
                        );

                        const format = column.DateTimeFormat || "MM/DD/YYYY";
                        var dt = moment(value);
                        const valueWithFormat = dt.isValid() ? dt.format(format) : "";

                        content = `<span class="${column.CssClass ?? ''}">
                                        {icon}
                                       ${valueWithFormat}
                                    </span>`.replace(
                            /{icon}/gm,
                            column.Icon ? `<i class="${column.Icon}"></i>` : ""
                        );
                    } else if (column.DataType == "Image") {
                        var value = expressionService.parseExpression(
                            content,
                            scope,
                            "",
                            true
                        );
                        if (value) {
                            if (column.DataType == "Image") {
                                if (column.ImageType == "JsonData") {
                                    const imageIndex = column.ImageIndex ?
                                        parseInt(column.ImageIndex) || 0 :
                                        0;
                                    const imagge = value instanceof Array ? value : [value];
                                    var imageSrc =
                                        _.get(imagge[imageIndex], column.ImageProperty) || "";
                                }
                                value = `<div class="${column.ImageWrapperCssClass ?? ''}"><img src="${imageSrc ?? ''}" class="${column.CssClass ?? ''}" /></div>`;
                            }
                        } else if (column.ShowNoImage && column.NoImageType == "url") {
                            value = `<img src="${column.NoImageUrl}" />`;
                        } else if (column.ShowNoImage && column.NoImageType == "icon") {
                            value = `<icon class="grid-image-icon ${column.NoImageUrl}"></i>`;
                        }

                        content = value;
                        //     `<span class="${column.CssClass}">
                        //     {icon}
                        //     ${value}
                        //    </span>`.replace(
                        //     /{icon}/gm,
                        //     column.Icon ? `<i class="${column.Icon}"></i>` : "");
                    } else {
                        var value =
                            expressionService.parseExpression(content, scope, "", true) || "";

                        content = `<span class="${column.CssClass ?? ''}">
                                       {icon}
                                       ${value}
                                    </span>`.replace(
                            /{icon}/gm,
                            column.Icon ? `<i class="${column.Icon}"></i>` : ""
                        );
                    }
                } else if (column.ColumnType == 'PopularItems') {
                    /*------------------------------------*/
                    /* Progress Bar   */
                    /*------------------------------------*/
                    if (column.PopularItemType == 'Progressbar') {
                        var value = expressionService.parseExpression(content, scope, "", true) || "";
                        content = `
                        <div class="progress grid-progress theme-${column.Progressbar_Theme ? column.Progressbar_Theme : 'default'}">
                            <label class="progress-value">${value}%</label>
                            <div class="progress-bar progress-${value}" role="progressbar" aria-valuenow="${value}" aria-valuemin="0" aria-valuemax="100" style="width:${value}%"></div>
                        </div>`;
                    }

                    /*------------------------------------*/
                    /* Star Rating   */
                    /*------------------------------------*/
                    if (column.PopularItemType == 'StarRating') {
                        var value = expressionService.parseExpression(content, scope, "", true) || "";
                        content = `
                        <div class="star-rating">
                            <span class="five-rate">
                                <i ng-repeat="x in [].constructor(${column.StarRating_StarCount}) track by $index" class="rating-icon ${column.StarRating_FontIcon}" style="color:{{${value}>=($index+1)?'${column.StarRatingFillColor}':'${column.StarRating_EmptyColor}'}}"></i>
                            </span>
                            <label ng-repeat="item in column.StarRating_RateLabels | filter:{Rate:${value}}" class="rating-label">{{item.Text}}</label>
                        </div>`;
                    }

                    /*------------------------------------*/
                    /* Two Lines   */
                    /*------------------------------------*/
                    if (column.PopularItemType == 'TwoLines') {
                        var firstLineValue = expressionService.parseExpression(column.TwoLines_FirstLineProperty, scope, "", true) || "";
                        var secondLineValue = expressionService.parseExpression(column.TwoLines_SecondLineProperty, scope, "", true) || "";

                        content = `
                        <div class="two-lines">
                            <span class="first-line" title="${firstLineValue}" ng-style="{'color':'${column.TwoLines_FirstLineColor}','width':'${column.TwoLines_Width}'}">${firstLineValue}</span>
                            <span class="second-line" title="${secondLineValue}" ng-style="{'color':'${column.TwoLines_SecondLineColor}','width':'${column.TwoLines_Width}'}">${secondLineValue}</span>
                        </div>`;
                    }
                }

                //const $row = $compile(`${content}`)(scope);
                //$(element).replaceWith($row);
                $(element).html($compile(content)(scope));
            }

            raiseElement();

            scope.bActionButton_onClick = function (row, column) {
                if (column.ShowConfirmAlert) {
                    if (confirm(column.ConfirmAlertText)) {
                        callAction(row, column);
                    }
                } else callAction(row, column);
            };

            function callAction(row, column) {
                var defer = $q.defer();

                var action = _.find(module.actions, (a) => {
                    return a.ActionId == column.ActionId;
                });

                const backupcolumnActionParams = _.cloneDeep(column.ActionParams);

                _.forEach(column.ActionParams, (param) => {
                    param.ParamValue = expressionService.parseExpression(
                        param.ParamValue,
                        scope,
                        param.ExpressionParsingType,
                        true
                    );
                });

                action.Params = column.ActionParams;

                var buffer = actionService.createBufferByRootAction(module.actions, action);
                actionService.callActionFromBuffer(buffer, defer, module.$scope).then((data) => {
                    column.ActionParams = _.cloneDeep(backupcolumnActionParams);
                    defer.resolve();
                });

                return defer.promise;
            }

            scope.$watch("Row", (newVal, oldVal) => {
                if (newVal && oldVal && newVal !== oldVal) {
                    raiseElement();
                }
            }, true);
        },
        replace: true,
    };
})