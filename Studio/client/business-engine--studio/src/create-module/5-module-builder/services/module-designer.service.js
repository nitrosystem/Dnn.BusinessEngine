export class ModuleDesignerService {
  constructor(
    $filter,
    $q,
    $compile,
    $timeout,
    apiService,
    notificationService
  ) {
    this.$filter = $filter;
    this.$q = $q;
    this.$compile = $compile;
    this.$timeout = $timeout;
    this.apiService = apiService;
    this.notifyService = notificationService;

    this.fieldLayoutTemplate;
    this.fieldsTemplate = {};
    this.fieldsScripts = {};
  }

  renderDesignForm(module, fields, $scope, actions) {
    const $defer = this.$q.defer();

    this.module = module;
    this.fields = fields;
    this.$scope = $scope;
    this.actions = actions;
    this.$board = $("<div></div>");

    _.forEach(this.fields, (f) => {
      f.AddedToPane = false;
    });

    this.panes = [];
    $(`<div>${module.LayoutTemplate}</div>`)
      .find("*[data-pane]")
      .each((index, element) => {
        var paneName = $(element).data("pane");
        var paneTitle = $(element).data("pane-title");
        this.panes.push({ paneName: paneName, paneTitle: paneTitle });

        this.$board.append(this.getBoardPane(paneName, paneTitle));
      });

    //This section removed. because when panes of fields not in layout template panes then them panes are not usable
    _.forEach(this.fields, (field) => {
      if (_.filter(this.panes, (p) => { return p.paneName == field.PaneName; }).length == 0)
        this.panes.push({
          paneName: field.PaneName,
          paneTitle: field.PaneName,
          parentId: field.ParentId,
        });
    });

    this.buffer = [];
    _.forEach(this.panes, (p) => {
      this.buffer.push({ PaneName: p.paneName });
    });

    this.processBuffer($defer).then((data) => { $defer.resolve(data); });

    return $defer.promise;
  }

  processBuffer($defer) {
    if (!this.buffer.length) {
      this.completed().then((data) => { $defer.resolve(data); });
    } else {
      const currentPane = this.buffer[0];
      const $pane = this.$board.find('*[data-pane="' + currentPane.PaneName + '"]');

      if ($pane.length) {
        const paneFields = _.orderBy(
          _.filter(this.fields, (f) => {
            return f.PaneName == currentPane.PaneName && !f.AddedToPane;
          }), ["ViewOrder"], ["desc"]
        );

        this.parsePaneFields($pane, paneFields).then(() => {
          this.buffer.shift();

          this.processBuffer($defer);
        });
      } else {
        currentPane.tryForFindPane = currentPane.tryForFindPane || 0;
        if (currentPane.tryForFindPane < 10) {
          currentPane.tryForFindPane++;

          this.buffer.push(_.clone(currentPane));
        } else {
          if (!this.$board.find('*[data-pane="bTempPane"]').length)
            this.$board.append(
              this.getBoardPane("bTempPane", "Temp Pane")
            );

          var findPane = _.filter(this.panes, (p) => {
            return p.paneName == "bTempPane";
          });

          if (!findPane.length)
            this.panes.push({
              paneName: "bTempPane",
              paneTitle: "Temp Pane",
            });

          _.filter(this.fields, (f) => {
            return f.PaneName == currentPane.PaneName;
          }).map((f) => {
            f.PaneName = "bTempPane";
          });

          this.buffer.push({ PaneName: "bTempPane" });
        }

        this.buffer.shift();
        this.processBuffer($defer);
      }
    }

    return $defer.promise;
  }

  parsePaneFields($pane, fields) {
    const $defer = this.$q.defer();

    this.parseField($defer, $pane, fields, fields.length).then(() =>
      $defer.resolve()
    );

    return $defer.promise;
  }

  parseField($defer, $pane, fields, index) {
    if (index <= 0) {
      $defer.resolve();
    } else {
      var field = fields[index - 1];

      this.getFieldUI(field, this.$scope).then(($fieldItem) => {
        $pane.append($fieldItem);
        field.AddedToPane = true;

        this.parseField($defer, $pane, fields, index - 1);
      });
    }

    return $defer.promise;
  }

  completed() {
    const $defer = this.$q.defer();

    $defer.resolve({ $board: this.$board, panes: this.panes });

    return $defer.promise;
  }

  getFieldUI(field, $scope) {
    const $defer = this.$q.defer();

    if (!field.FieldTypeObject) field.FieldTypeObject = { FieldComponent: 'div' };

    const fieldTemplate = this.getBoardFieldItem(field);
    const $fieldItem = this.$compile(fieldTemplate)($scope);
    this.$timeout(() => {
      if (!field.IsGroup) {
        $defer.resolve($fieldItem);
      } else {
        $($fieldItem).find("*[data-pane]").each((index, element) => {
          const $pane = this.getBoardPane(
            $(element).data("pane"),
            $(element).data("pane-title"),
            field.Id,
            field.FieldName
          );

          $(element).replaceWith($pane);
        });

        this.$timeout(() => $defer.resolve($fieldItem));
      }
    });

    return $defer.promise;
  }

  getBoardPane(paneName, paneTitle, fieldId, fieldName) {
    fieldName = fieldName || "";

    var _layout = `
          <div class="b-group board-pane"> 
            <div class="group-header" data-bs-toggle="collapse" data-bs-target="#grpPane${paneName}">
              <h3 class="group-label">
                  <span class="group-icon">
                      <i class="codicon codicon-server-process"></i>
                  </span>
                  ${paneTitle}
              </h3>
              <span class="group-collapse">
                  <i class="codicon codicon-chevron-up"></i>
              </span>
            </div>
            <div id="grpPane${paneName}" class="group-content collapse show ${paneName == 'bTempPane' ? 'b-temp-pane-warning' : ''}">
              <div class="pane-body sortable-row" data-pane="${paneName}" data-parent-id="${fieldId}"></div> 
              <div class="pane pane-footer" field-drop="${paneName}" data-pane-title="${fieldName} ${paneTitle}" data-parent-id="${fieldId}"></div> 
            </div> 
          </div>`;

    return $(_layout);
  }

  getBoardFieldItem(field) {
    const result =
      `
<div b-field="${field.Id}" class="b-field-wrapper">
    <!------------------------------------>
    <!--One section for drop another field before this field-->
    <!------------------------------------>
    <div class="field-drag-panel" data-drop="true" jqyoui-droppable="{onOver:'$.onFieldDragOver()',onOut:'$.onFieldDragOut()',onDrop:'$.onFieldDrop($.field.${field.FieldName}.PaneName,$.field.${field.FieldName}.ParentId,$.field.${field.FieldName}.Id)'}"></div>
    <div data-field="${field.Id}" class="b-field-item" 
         ng-class="{'active':$.currentField.Id=='${field.Id}','field-hide':$.currentField.Id!=='${field.Id}' && (!$.field.${field.FieldName}.IsShow || ($.field.${field.FieldName}.ShowConditions && $.field.${field.FieldName}.ShowConditions.length))}"
         ng-click="$.onFieldItemClick($event,'${field.Id}')" tabindex="-1" ng-blur="$.onFieldItemBlur($event,$.field.${field.FieldName})">
        <div class="field-deleted-wrapper" ng-if="$.field.${field.FieldName}.isDeleted">
            <button type="button" class="shine2" ng-click="$.onUndoDeleteFieldClick($event,'${field.Id}','${field.FieldName}')"
                  title="Undo Delete" b-custom-tooltip data-bs-placement="top">
                <i class="codicon codicon-discard"></i>
            </button>
        </div>    
        <!------------------------------------>
        <!--Field toolbar when this field is the current field-->
        <!------------------------------------>
        <div class="field-toolbar" ng-if="$.currentField.Id=='${field.Id}'"
            ng-click="$.onFieldToolbarClick($event)" ng-class="{'shine2':$.running=='refresh-field' || $.running=='save-field'}">
            <div class="col">
                <ul class="toolbar-items">
                    <li class="toolbar-item">
                        <a class="field-settings-item" ng-click="$.onFieldSettingsClick($event)" role="button">
                            <i class="codicon codicon-gear"></i>
                        </a>
                    </li>
                    <li class="toolbar-item">
                        <a class="field-move handle" ng-click="$.onFieldSettingsClick($event)" role="button">
                            <i class="codicon codicon-move"></i>
                        </a>
                    </li>
                    <li class="toolbar-item">
                        <a class="field-settings-item" role="button" title="shift key + 🠅 arrow" b-custom-tooltip
                            data-bs-placement="top" ng-click="$.onFieldSwap($event,'up')" role="button">
                            <i class="codicon codicon-arrow-up"></i>
                        </a>
                    </li>
                    <li class="toolbar-item">
                        <a class="field-settings-item" title="shift key + 🠛 arrow" b-custom-tooltip
                            data-bs-placement="top" ng-click="$.onFieldSwap($event,'down')" role="button">
                            <i class="codicon codicon-arrow-down"></i>
                        </a>
                    </li>
                    <li class="toolbar-item">
                        <a class="field-settings-item" role="button" data-bs-toggle="dropdown"
                            data-bs-auto-close="outside" aria-expanded="false">
                            <i class="codicon codicon-references"></i>
                        </a>
                        <ul class="dropdown-menu">
                            <li ng-repeat="pane in $.panes">
                                <a class="dropdown-item" href="#" ng-click="$.onFieldChangePaneClick(pane,$event)" ng-disabled="pane.paneName=='${field.PaneName}'">{{pane.paneTitle}}</a>
                            </li>
                        </ul>
                    </li>
                    <li class="toolbar-item">
                        <a class="field-settings-item" title="Field Template & Theme" b-custom-tooltip data-bs-placement="top" ng-click="$.onShowFieldTemplateClick()" role="button">
                            <i class="codicon codicon-notebook-template"></i>
                        </a>
                    </li>                    
                    <li class="toolbar-item">
                        <div class="dropdown">
                            <i class="codicon codicon-ellipsis align-middle ms-1" role="button"
                                data-bs-toggle="dropdown" aria-expanded="false" data-bs-auto-close="outside"></i>
                            <div class="dropdown-menu p-4 w25-rem">
                                <div class="mb-3">
                                    <label class="form-label">Field Css Class</label>
                                    <input type="text" ng-model="$.currentField.Settings.CssClass"
                                        class="b-input form-control" placeholder="Example mb-3 (bootstrap)">
                                </div>
                                <div class="mb-3">
                                    <label class="form-label">Field Subtext</label>
                                    <input type="text" ng-model="$.currentField.Settings.Subtext"
                                        class="b-input form-control" placeholder="Field subtext message">
                                </div>
                                <div class="mb-3">
                                    <label class="form-label">Field Subtext Css Class</label>
                                    <input type="text" ng-model="$.currentField.Settings.SubtextCssClass"
                                        class="b-input form-control" placeholder="Field subtext css class">
                                </div>
                            </div>
                        </div>
                    </li>
                </ul>
            </div>
            <div class="col">
                <ul class="toolbar-items d-flex justify-content-end">
                    <li class="toolbar-item" ng-if="$.currentField.IsValuable">
                        <label class="b-switch switch-sm me-3" b-custom-tooltip title="Is Required">
                            <input type="checkbox" ng-model="$.currentField.IsRequired">
                            <span class="slider"></span>
                        </label>
                    </li>
                    <li class="toolbar-item">
                        <button type="button" class="field-settings-item"
                            ng-click="$.onRefreshFieldClick($event,'${field.Id}')" role="button"
                            title="Reload & Refresh Field(ctrl + 5)" b-custom-tooltip data-bs-placement="top">
                            <i class="codicon codicon-refresh"></i>
                        </button>
                    </li>
                    <li class="toolbar-item">
                        <button type="button" class="field-settings-item"
                            ng-click="$.onSaveFieldClick($event,false,true)" role="button" title="Save Field(ctrl + s)"
                            b-custom-tooltip data-bs-placement="top">
                            <i class="codicon codicon-save"></i>
                        </button>
                    </li>
                    <li class="toolbar-item">
                        <button type="button" class="field-settings-item" ng-click="$.onCancelEditFieldClick($event,'${field.Id}')"
                            role="button" title="Cancel Field Changes(esc)" b-custom-tooltip data-bs-placement="top">
                            <i class="codicon codicon-circle-slash"></i>
                        </button>
                    </li>
                    <li class="toolbar-item">
                        <button type="button" class="field-actions-item" ng-click="$.onShowFieldActionsClick($event)"
                            role="button" title="Goto Field Actions(ctrl + q)" b-custom-tooltip data-bs-placement="top">
                            <i class="codicon codicon-github-action"></i>
                        </button>
                    </li>
                    <li class="toolbar-item">
                        <button type="button" class="field-settings-item" ng-click="$.onDeleteFieldClick($event)"
                            role="button" title="Delete Field(ctrl + del)" b-custom-tooltip data-bs-placement="top">
                            <i class="codicon codicon-trash"></i>
                        </button>
                    </li>
                </ul>
            </div>
        </div>
        <!------------------------------------>
        <!--Field content and options-->
        <!------------------------------------>
        <div class="field-body">
            <div class="row mb-3" ng-if="!$.field.${field.FieldName}.FieldTypeObject.IsContent">
                <div class="col-6">
                    <div class="field-title-wrapper">
                        <label class="b-switch switch-sm" title="Show Field Text(Label)">
                            <input type="checkbox" ng-checked="!$.field.${field.FieldName}.Settings.IsHideFieldText"
                                ng-click="$.field.${field.FieldName}.Settings.IsHideFieldText=!$.field.${field.FieldName}.Settings.IsHideFieldText">
                            <span class="slider"></span>
                        </label>
                        <b ng-class="{'opacity-25':$.field.${field.FieldName}.Settings.IsHideFieldText}">
                          Field Text(Label):
                        </b>
                        <input type="text" ng-model="$.field.${field.FieldName}.FieldText" class="b-input-edit"
                            ng-class="{'opacity-25 text-decoration-line-through':$.field.${field.FieldName}.Settings.IsHideFieldText}"
                            ng-readonly="$.field.${field.FieldName}.Settings.IsHideFieldText || $.currentField.Id!=='${field.Id}'"
                            placeholder="Enter field text(label)" autocomplete="off" />
                    </div>
                </div>
                <div class="col-3">
                    <div class="field-title-wrapper">
                        <b>Field Name:</b>
                        <input type="text" class="b-input-edit" ng-model="$.field.${field.FieldName}.FieldName"
                            ng-readonly="$.currentField.Id!=='${field.Id}'" placeholder="Enter field name"
                            autocomplete="off" />
                    </div>
                </div>
                <div class="col-3">
                    <div class="field-title-wrapper">
                        <b>Field Type:</b>
                        <span class="field-type-name"
                            title="{{$.field.${field.FieldName}.FieldType}}">{{$.field.${field.FieldName}.FieldType}}</span>
                    </div>
                </div>
            </div>
            <${field.FieldTypeObject.FieldComponent} field="$.field.${field.FieldName}" module-builder-controller="$" fields="$.fields" actions="$.actions" all-actions="$.allActions"></${field.FieldTypeObject.FieldComponent}>
            <span class="mb-2 {{$.field.${field.FieldName}.Settings.SubtextCssClass||''}}">{{$.field.${field.FieldName}.Settings.Subtext}}</span>
            <!------------------------------------>
            <!--Field actions-->
            <!------------------------------------>
            <div class="b-field mt-3"
                ng-if="$.field.${field.FieldName}.Actions && $.field.${field.FieldName}.Actions.length">
                <label class="form-label">Field Actions</label>
                <div ng-repeat="action in $.field.${field.FieldName}.Actions" role="button"
                    ng-click="$.onEditActionClick(action.Id,'${field.FieldType}','${field.Id}')">
                    <div class="b-notify notify mb-3">
                        <i class="codicon codicon-github-action icon-sm"></i>
                        <div class="text">
                            <span class="subtext">{{action.ActionName}}</span>
                        </div>
                    </div>
                </div>
            </div>
            <!------------------------------------>
            <!--Field data source for slective fields-->
            <!------------------------------------>
            <div ng-if="$.field.${field.FieldName}.IsSelective">
                <div class="b-field mt-3" ng-if="$.field.${field.FieldName}.DataSource">
                    <label class="form-label">Data Source</label>
                    <div role="button" ng-click="$.onEditFieldDataSourceClick($event,'${field.Id}')">
                        <div class="b-notify notify mb-3">
                            <i class="codicon codicon-combine icon-sm"></i>
                            <div class="text">
                                <span class="subtext">
                                    Type :
                                    <label ng-if="$.field.${field.FieldName}.DataSource.Type==0">Standard(Static
                                        items)</label>
                                    <label ng-if="$.field.${field.FieldName}.DataSource.Type==1">Use Defined
                                        List</label>
                                    <label ng-if="$.field.${field.FieldName}.DataSource.Type==2">Data Source Service
                                    </label>
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="b-field mt-3" ng-if="!$.field.${field.FieldName}.DataSource">
                    <button type="button" class="b-btn btn-text-icon btn-action"
                        ng-click="$.onEditFieldDataSourceClick($event,'${field.Id}')">
                        <i class="codicon codicon-plus"></i>
                        Set Data Source
                    </button>
                </div>
            </div>
        </div>
    </div>
    <div id="pnlMoreOption_${field.Id}" class="collapse">
      <!------------------------------------>
      <!--Field conditional value -->
      <!------------------------------------>
      <div class="d-flex flex-column align-items-start small mb-3 b-notify hover-box-shadow" ng-if="$.field.${field.FieldName}.FieldValues && $.field.${field.FieldName}.FieldValues.length">
        <label class="form-label small mb-0">Field Value(s)</label>
        <table class="table small mt-2" ng-click="$.onShowConditionalValuesClick($event,'${field.Id}')" role="button">
            <thead>
                <tr>
                    <th>
                        Value Expressions
                    </th>
                    <th>
                        Condition(s)
                    </th>
                    <th>
                        <i class="codicon codicon-ellipsis"></i>
                    </th>
                </tr>
            </thead>
            <tbody>
                <tr ng-repeat="item in $.field.${field.FieldName}.FieldValues">
                    <td>
                        {{item.ValueExpression}}
                    </td>
                    <td>
                        <p ng-if="item.Conditions && item.Conditions.length" ng-repeat="condition in item.Conditions">
                          {{condition.LeftExpression}}
                          <b>{{condition.EvalType}}</b>
                          {{condition.RightExpression}}                        
                        </p>
                        <span ng-if="!item.Conditions || !item.Conditions.length">
                          No Condition!.
                        </span>
                    </td>
                    <td>
                        <i class="codicon codicon-trash"></i>
                    </td>
                </tr>
            </tbody>
        </table>
      </div>
      <!------------------------------------>
      <!--Field show conditions-->
      <!------------------------------------>
      <div class="d-flex flex-column align-items-start small mb-3 b-notify hover-box-shadow" ng-if="$.field.${field.FieldName}.ShowConditions && $.field.${field.FieldName}.ShowConditions.length">
        <label class="form-label small mb-0">Show Condition(s)</label>
        <table class="table mt-2" ng-click="$.onShowConditionsClick($event,'${field.Id}')" role="button">
            <thead>
                <tr>
                    <th>
                        <i class="codicon codicon-eye"></i>
                    </th>
                    <th>
                        Show Condition
                    </th>
                    <th>
                        Group
                    </th>
                    <th>
                        <i class="codicon codicon-ellipsis"></i>
                    </th>
                </tr>
            </thead>
            <tbody>
                <tr ng-repeat="condition in $.field.${field.FieldName}.ShowConditions">
                    <td>
                        <i class="codicon codicon-eye-closed"></i>
                    </td>
                    <td>
                        {{condition.LeftExpression}}
                        <b>{{condition.EvalType}}</b>
                        {{condition.RightExpression}}
                    </td>
                    <td>
                        {{condition.GroupName}}
                    </td>
                    <td>
                        <i class="codicon codicon-trash"></i>
                    </td>
                </tr>
            </tbody>
        </table>
      </div>
    </div>
      <!------------------------------------>
      <!--Expand/Collapse button -->
      <!------------------------------------>
    <button type="button" class="btn-expand-collapse" role="button" data-bs-toggle="collapse" data-bs-target="#pnlMoreOption_${field.Id}" aria-expanded="false" aria-controls="pnlMoreOption_${field.Id}"
      ng-click="$.field.${field.FieldName}.isExpand=!$.field.${field.FieldName}.isExpand" ng-class="{'shine2':!$.field.${field.FieldName}.isExpand,'is-expand':$.field.${field.FieldName}.isExpand}"
      ng-if=
        "
          ($.field.${field.FieldName}.ShowConditions && $.field.${field.FieldName}.ShowConditions.length) || 
          ($.field.${field.FieldName}.FieldValues && $.field.${field.FieldName}.FieldValues.length)
        ">
        <i class="codicon" ng-class="{'codicon-expand-all':!$.field.${field.FieldName}.isExpand,'codicon-collapse-all':$.field.${field.FieldName}.isExpand}"></i>
    </button>
</div>`;
    return result;
  }
}