export class ModuleDesignerService {
    constructor($compile, $q, $timeout) {
        this.$compile = $compile;
        this.$q = $q;
        this.$timeout = $timeout;

        this.fieldsTemplate = {};
        this.fieldsScripts = {};
    }

    // ==========================================================================
    // PUBLIC API
    // ==========================================================================

    /**
     * Renders the design form for a module by building panes and compiling all fields
     * into an in-memory jQuery board. Returns the board and detected panes.
     *
     * @param {Object} module - The module definition (contains LayoutTemplate).
     * @param {Array}  fields - List of field definitions to render.
     * @param {Object} $scope - Angular scope used to compile field templates.
     * @returns {Promise<{$board: jQuery, panes: Array}>}
     */
    renderDesignForm(module, fields, $scope) {
        this.module = module;
        this.fields = fields;
        this.$scope = $scope;
        this.$board = $('<div></div>');

        // Reset AddedToPane flag for all fields
        this.fields.forEach((f) => {
            f.AddedToPane = false;
        });

        // Extract panes from the module's layout template
        this.panes = [];
        const $template = $(`<div>${module.LayoutTemplate}</div>`);
        $template.find('*[data-pane]').each((index, element) => {
            const $el = $(element);
            const paneName = $el.data('pane');
            const paneTitle = $el.data('pane-title');

            this.panes.push({ paneName, paneTitle });
            this.$board.append(this._getBoardPane(paneName, paneTitle));
        });

        // Register panes for fields whose pane is not declared in the layout
        this.fields.forEach((field) => {
            const exists = this.panes.some((p) => p.paneName === field.PaneName);
            if (!exists) {
                this.panes.push({
                    paneName: field.PaneName,
                    paneTitle: field.PaneName,
                    parentId: field.ParentId,
                });
            }
        });

        // Seed buffer with one entry per pane
        this.buffer = this.panes.map((p) => ({ PaneName: p.paneName }));
        return this._processBuffer();
    }

    /**
     * Compiles a single field into a jQuery element, including nested panes
     * for group fields. Used both internally and as a public utility.
     *
     * @param {Object} field  - Field definition to render.
     * @param {Object} $scope - Angular scope for compilation.
     * @returns {Promise<jQuery>}
     */
    getFieldUITemp(field, $scope) {
        return new Promise((resolve) => {
            if (!field.FieldTypeObject) {
                field.FieldTypeObject = { FieldComponent: 'div' };
            }

            const fieldTemplate = this._getBoardFieldItem(field);
            const $fieldItem = this.$compile(fieldTemplate)($scope);

            this.$timeout(() => {
                // Non-group field is ready immediately
                if (!field.IsGroupField) {
                    resolve($fieldItem);
                    return;
                }

                // For group fields, replace inner placeholders with real panes
                $($fieldItem)
                    .find('*[data-pane]')
                    .each((index, element) => {
                        const $el = $(element);
                        const $pane = this._getBoardPane(
                            $el.data('pane'),
                            $el.data('pane-title'),
                            field.Id,
                            field.FieldName
                        );
                        $el.replaceWith($pane);
                    });

                // Second tick to let Angular finish digest, then resolve
                this.$timeout(() => resolve($fieldItem));
            });
        });
    }

    getFieldUI(field, $scope) {
        const deferred = this.$q.defer();
        if (!field.FieldTypeObject) {
            field.FieldTypeObject = { FieldComponent: 'div' };
        }
        const fieldTemplate = this._getBoardFieldItem(field);
        const $fieldItem = this.$compile(fieldTemplate)($scope);
        this.$timeout(() => {
            // Non-group field is ready immediately
            if (!field.IsGroupField) {
                deferred.resolve($fieldItem);
                return;
            }
            // For group fields, replace inner placeholders with real panes
            $($fieldItem)
                .find('*[data-pane]')
                .each((index, element) => {
                    const $el = $(element);
                    const $pane = this._getBoardPane(
                        $el.data('pane'),
                        $el.data('pane-title'),
                        field.Id,
                        field.FieldName
                    );
                    $el.replaceWith($pane);
                });
            // Second tick to let Angular finish digest, then resolve
            this.$timeout(() => deferred.resolve($fieldItem));
        });
        return deferred.promise;
    }

    // ==========================================================================
    // PRIVATE METHODS
    // ==========================================================================

    /**
     * Iteratively drains the pane buffer: for each pane it finds the matching
     * DOM container, renders its fields in order, and finalizes the board.
     * Fields whose pane cannot be located after several retries are moved
     * into a fallback "Temp Pane".
     *
     * @returns {Promise<{$board: jQuery, panes: Array}>}
     */
    _processBuffer() {
        const deferred = this.$q.defer();
        const processNext = () => {
            // Buffer drained, finalize
            if (!this.buffer.length) {
                deferred.resolve({ $board: this.$board, panes: this.panes });
                return;
            }
            const currentPane = this.buffer[0];
            const $pane = this.$board.find(`*[data-pane="${currentPane.PaneName}"]`);
            if ($pane.length) {
                // Sort fields by ViewOrder descending, then render
                const paneFields = this.fields
                    .filter((f) => f.PaneName === currentPane.PaneName && !f.AddedToPane)
                    .sort((a, b) => b.ViewOrder - a.ViewOrder);
                this._parsePaneFields($pane, paneFields).then(
                    () => {
                        this.buffer.shift();
                        processNext();
                    },
                    (err) => {
                        // On failure, still continue with next pane to avoid stalling
                        this.buffer.shift();
                        processNext();
                    }
                );
            }
            else {
                // Pane DOM not ready yet; retry up to 10 times, then fallback
                currentPane.tryForFindPane = (currentPane.tryForFindPane || 0) + 1;
                if (currentPane.tryForFindPane < 10) {
                    this.buffer.push({ ...currentPane });
                }
                else {
                    this._moveFieldsToTempPane(currentPane.PaneName);
                }
                this.buffer.shift();
                processNext();
            }
        };
        processNext();
        return deferred.promise;
    }

    /**
     * Appends a set of fields to a pane sequentially, awaiting each compilation
     * to keep rendering order deterministic.
     *
     * @param {jQuery} $pane  - Target pane DOM element.
     * @param {Array}  fields - Fields to render into the pane.
     * @returns {Promise<void>}
     */
    _parsePaneFields($pane, fields) {
        let chain = this.$q.resolve();
        for (let i = fields.length - 1; i >= 0; i--) {
            const field = fields[i];
            chain = chain.then(() => {
                return this.getFieldUI(field, this.$scope).then(($fieldItem) => {
                    $pane.append($fieldItem);
                    field.AddedToPane = true;
                });
            });
        }
        return chain;
    }

    /**
     * Creates a fallback "Temp Pane" (if not already present) and reassigns
     * any orphan fields of the given pane name to it.
     *
     * @param {string} originalPaneName - The pane that could not be resolved.
     */
    _moveFieldsToTempPane(originalPaneName) {
        if (!this.$board.find('*[data-pane="bTempPane"]').length) {
            this.$board.append(this._getBoardPane('bTempPane', 'Temp Pane'));
        }

        const tempPaneExists = this.panes.some((p) => p.paneName === 'bTempPane');
        if (!tempPaneExists) {
            this.panes.push({ paneName: 'bTempPane', paneTitle: 'Temp Pane' });
        }

        this.fields
            .filter((f) => f.PaneName === originalPaneName)
            .forEach((f) => {
                f.PaneName = 'bTempPane';
            });

        this.buffer.push({ PaneName: 'bTempPane' });
    }

    /**
     * Builds the HTML wrapper (collapsible group + droppable area) for a pane.
     *
     * @param {string} paneName  - Unique pane identifier.
     * @param {string} paneTitle - Human-readable pane title.
     * @param {string} [fieldId] - Parent field id (for nested/group panes).
     * @param {string} [fieldName=''] - Parent field name (used in drop title).
     * @returns {jQuery}
     */
    _getBoardPane(paneName, paneTitle, fieldId, fieldName = '') {
        const tempClass = paneName === 'bTempPane' ? 'b-temp-pane-warning' : '';
        const layout = `
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
        <div id="grpPane${paneName}" class="group-content collapse show ${tempClass}">
          <div class="pane-body sortable-row" data-pane="${paneName}" data-parent-id="${fieldId ?? ''}"></div> 
          <div class="pane pane-footer" data-drop="${paneName}" data-pane-title="${fieldName} ${paneTitle}" data-parent-id="${fieldId ?? ''}"></div> 
        </div> 
      </div>`;
        return $(layout);
    }

    /**
     * Generates the full Angular template string for a single field item,
     * including toolbar, inline editors, data source,
     * and the dynamic field component tag.
     *
     * @param {Object} field - Field definition.
     * @returns {string} HTML template string (to be compiled by $compile).
     */
    _getBoardFieldItem(field) {
        const { Id, FieldName, FieldType, PaneName, ParentId, FieldTypeObject } = field;
        const Component = FieldTypeObject.FieldComponent;

        return `
    <div b-field="${Id}" class="b-field-wrapper">
      <!------------------------------------>
      <!--One section for drop another field before this field-->
      <!------------------------------------>
      <div class="field-drag-panel" data-drop="${PaneName}" data-parent-id="${ParentId ?? ''}" data-field="${FieldName}"></div>
      <div data-field="${Id}" class="b-field-item" 
          ng-class="{'active':$.currentField.Id=='${Id}'}"
          ng-click="$.onFieldItemClick($event,'${Id}')"
          ng-blur="$.onFieldItemBlur($event)"
          tabindex="-1">
          <div class="field-deleted-wrapper" ng-if="$.field.${FieldName}.isDeleted">
              <button type="button" class="shine2" ng-click="$.onUndoDeleteFieldClick($event,'${Id}','${FieldName}')"
                    title="Undo Delete" bootstrap-tooltip data-bs-placement="top">
                  <i class="codicon codicon-discard"></i>
              </button>
          </div>    
          <!------------------------------------>
          <!--Field toolbar when this field is the current field-->
          <!------------------------------------>
          <div class="field-toolbar" ng-if="$.currentField.Id=='${Id}'"
              ng-click="$.onFieldToolbarClick($event)" ng-class="{'shine2':$.running=='refresh-field' || $.running=='save-field'}">
              <div class="col">
                  <ul class="toolbar-items">
                      <li class="toolbar-item">
                          <button type="button" class="field-settings-item" ng-click="$.onFieldSettingsClick($event)">
                              <i class="codicon codicon-gear" bootstrap-tooltip data-bs-placement="top" title="Field Settings"></i>
                          </button>
                      </li>
                      <li class="toolbar-item">
                          <button type="button" class="field-move handle" ng-click="$.onFieldSettingsClick($event)" data-handle="true">
                              <i class="codicon codicon-move" bootstrap-tooltip data-bs-placement="top" title="Field Movement"></i>
                          </button>
                      </li>
                      <li class="toolbar-item">
                          <button type="button" class="field-settings-item" ng-click="$.onFieldSwap($event,'up')">
                              <i class="codicon codicon-arrow-up" bootstrap-tooltip data-bs-placement="top" title="Move to up(shift key + 🠅 arrow)"></i>
                          </button>
                      </li>
                      <li class="toolbar-item">
                          <button type="button" class="field-settings-item" ng-click="$.onFieldSwap($event,'down')">
                              <i class="codicon codicon-arrow-down" bootstrap-tooltip data-bs-placement="top" title="Move to down(shift key + 🠛 arrow)"></i>
                          </button>
                      </li>
                      <li class="toolbar-item">
                          <button type="button" class="field-settings-item" ng-click="$.onShowFieldTemplateClick()">
                              <i class="codicon codicon-notebook-template" bootstrap-tooltip data-bs-placement="top" title="Field Template & Theme"></i>
                          </button>
                      </li>                    
                      <li class="toolbar-item">
                          <a class="field-settings-item" role="button" data-bs-toggle="dropdown"
                              data-bs-auto-close="outside" aria-expanded="false">
                              <i class="codicon codicon-references" bootstrap-tooltip data-bs-placement="top" title="Change Pane"></i>
                          </a>
                          <ul class="dropdown-menu">
                              <li ng-repeat="pane in $.panes">
                                  <a class="dropdown-item" href="#" ng-click="$.onFieldChangePaneClick(pane,$event)" ng-disabled="pane.paneName=='${PaneName}'">{{pane.paneTitle}}</a>
                              </li>
                          </ul>
                      </li>
                  </ul>
              </div>
              <div class="col">
                  <ul class="toolbar-items d-flex justify-content-end">
                      <li class="toolbar-item" ng-if="$.currentField.CanHaveValue">
                          <label class="b-switch switch-sm me-3" bootstrap-tooltip title="Is Required">
                              <input type="checkbox" ng-model="$.currentField.IsRequired">
                              <span class="slider"></span>
                          </label>
                      </li>
                      <li class="toolbar-item">
                          <button type="button" class="field-settings-item"ng-click="$.onFieldReloadClick($event,'${Id}')">
                              <i class="codicon codicon-refresh" bootstrap-tooltip data-bs-placement="top" title="Reload & Refresh Field(ctrl + 5)" disable-events="true"></i>
                          </button>
                      </li>
                      <li class="toolbar-item">
                          <button type="button" class="field-settings-item" ng-click="$.onSaveFieldClick($event,false,true)">
                              <i class="codicon codicon-save" bootstrap-tooltip data-bs-placement="top" title="Save Field(ctrl + s)" disable-events="true"></i>
                          </button>
                      </li>
                      <li class="toolbar-item">
                          <button type="button" class="field-settings-item" ng-click="$.onCancelFieldClick($event,'${Id}')">
                              <i class="codicon codicon-circle-slash" bootstrap-tooltip data-bs-placement="top" title="Cancel Field Changes(esc)" disable-events="true"></i>
                          </button>
                      </li>
                      <li class="toolbar-item">
                          <button type="button" class="field-actions-item" ng-click="$.onShowFieldActionsClick($event)">
                              <i class="codicon codicon-github-action" bootstrap-tooltip data-bs-placement="top" title="Goto Field Actions(ctrl + q)"></i>
                          </button>
                      </li>
                      <li class="toolbar-item">
                          <button type="button" class="field-settings-item" ng-click="$.onDeleteFieldClick($event)">
                              <i class="codicon codicon-trash" bootstrap-tooltip data-bs-placement="top" title="Delete Field(ctrl + del)"></i>
                          </button>
                      </li>
                  </ul>
              </div>
          </div>
          <!------------------------------------>
          <!--Field content and options-->
          <!------------------------------------>
          <div class="field-body">
              <div class="row mb-3" ng-if="!$.field.${FieldName}.FieldTypeObject.IsContentField">
                  <div class="col-3">
                      <div class="field-title-wrapper">
                          <label class="b-switch switch-sm" title="Show Field Text(Label)">
                              <input type="checkbox" ng-checked="!$.field.${FieldName}.Settings.IsHiddenFieldText"
                                  ng-click="$.field.${FieldName}.Settings.IsHiddenFieldText=!$.field.${FieldName}.Settings.IsHiddenFieldText"
                                  ng-disabled="$.field.${FieldName}.Settings.IsDisabledLayout" />
                              <span class="slider"></span>
                          </label>
                          <b ng-class="{'opacity-25':$.field.${FieldName}.Settings.IsHiddenFieldText}">
                            Label:
                          </b>
                          <input type="text" ng-model="$.field.${FieldName}.FieldText" class="b-input-edit"
                              ng-class="{'opacity-25 text-decoration-line-through':$.field.${FieldName}.Settings.IsHiddenFieldText}"
                              ng-readonly="$.field.${FieldName}.Settings.IsHiddenFieldText || $.currentField.Id!=='${Id}'"
                              placeholder="Enter field text(label)" autocomplete="off" />
                      </div>
                  </div>
                  <div class="col-4 pe-0">
                      <span ng-class="{'opacity-25':!$.field.${FieldName}.CanHaveValue}">
                        <b ng-if="!$.field.${FieldName}.FieldValueProperty" ng-click="$.onSetFieldPropertyValueClick('${Id}')">
                          Not Property Set
                        </b>
                        <b ng-if="$.field.${FieldName}.FieldValueProperty" ng-click="$.onSetFieldPropertyValueClick('${Id}')">
                          {{$.field.${FieldName}.FieldValueProperty}}
                        </b>
                      </span>
                  </div>
                  <div class="col-3 pe-0">
                      <div class="field-title-wrapper">
                          <b>Field Name:</b>
                          <input type="text" class="b-input-edit" ng-model="$.field.${FieldName}.FieldName"
                              ng-readonly="$.currentField.Id!=='${Id}'" placeholder="Enter field name"
                              autocomplete="off" />
                      </div>
                  </div>
                  <div class="col-2 pe-0">
                      <div class="field-title-wrapper justify-content-end">
                          <span class="field-type-name"
                              title="{{$.field.${FieldName}.FieldType}}">{{$.field.${FieldName}.FieldType}}
                            </span>
                      </div>
                  </div>
              </div>
              <${Component} field="$.field.${FieldName}" module-builder-controller="$" fields="$.fields" actions="$.actions" all-actions="$.allActions" class="d-block mb-3"></${Component}>
              <span class="mb-2 {{$.field.${FieldName}.Settings.SubtextCssClass||''}}">{{$.field.${FieldName}.Settings.Subtext}}</span>
              <!------------------------------------>
              <!--Field data source for slective fields-->
              <!------------------------------------>
              <div ng-if="$.field.${FieldName}.HasDataSource">
                  <div class="b-field mt-3" ng-if="$.field.${FieldName}.DataSource">
                      <label class="form-label">Data Source</label>
                      <div role="button" ng-click="$.onEditFieldDataSourceClick($event,'${Id}')">
                          <div class="b-notify notify mb-3">
                              <i class="codicon codicon-combine icon-sm"></i>
                              <div class="text">
                                  <span class="subtext">
                                      Type:
                                      <label ng-if="$.field.${FieldName}.DataSource.Type===0">Defined List</label>
                                      <label ng-if="$.field.${FieldName}.DataSource.Type===1">Variable Data</label>
                                  </span>
                              </div>
                          </div>
                      </div>
                  </div>
                  <div class="b-field mt-3" ng-if="!$.field.${FieldName}.DataSource">
                      <button type="button" class="b-btn btn-text-icon btn-action"
                          ng-click="$.onEditFieldDataSourceClick($event,'${Id}')">
                          <i class="codicon codicon-plus"></i>
                          Set Data Source
                      </button>
                  </div>
              </div>
              <!------------------------------------>
              <!--Field actions-->
              <!------------------------------------>
              <div class="b-field mt-3" ng-if="$.field.${FieldName}.Actions && $.field.${FieldName}.Actions.length">
                  <label class="form-label">Field Actions</label>
                  <div ng-repeat="action in $.field.${FieldName}.Actions" role="button"
                      ng-click="$.onEditActionClick(action.Id,'${FieldType}','${Id}')">
                      <div class="b-notify notify mb-3">
                          <i class="codicon codicon-github-action icon-sm"></i>
                          <div class="text">
                              <span class="subtext">{{action.ActionName}}</span>
                          </div>
                      </div>
                  </div>
              </div>
              <!------------------------------------>
              <!--Action button-->
              <!------------------------------------>
              <button type="button" ng-if="$.field.${FieldName}.CanHaveAction" class="b-btn btn-sm" ng-click="$.onAddActionClick('${field.FieldType}','${field.Id}',$event)">
                  <i class="codicon codicon-pulse"></i>
                  Add Action
              </button>              
          </div>
      </div>
    </div>`;
    }
}

