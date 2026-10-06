/**
 * Monaco DSL Editor Directive
 *
 * An AngularJS directive that creates a Monaco editor instance
 * with custom DSL support.
 *
 * Features:
 * - Each instance uses its own `suggestions` object
 * - Compatible with Bootstrap Tabs (no cross-tab contamination)
 * - Supports runtime changes to `suggestions`
 * - Proper lifecycle management and memory-leak prevention
 *
 * Usage:
 *   <div dsl-editor
 *        ng-model="myModel"
 *        suggestions="mySuggestions"
 *        editor-id="..."
 *        data-height="300px"
 *        read-only="false">
 *   </div>
 */

import * as monaco from 'monaco-editor/esm/vs/editor/editor.api.js';
import { registerDSLLanguage, getLanguageId, getThemeId } from './language';
import { registerEditor, unregisterEditor, updateSuggestions } from './registry';

// ─── Helpers ───
/**
 * Generate a unique id for an editor instance.
 * @returns {string}
 */
function generateEditorId() {
    const random = Math.random().toString(36).slice(2, 8);
    return `dsl_editor_${Date.now()}_${random}`;
}

/**
 * Build the editor creation options object.
 * Extracted so `createEditor` stays small and readable.
 *
 * @param {object} model - Monaco text model
 * @param {object} scope - AngularJS scope
 * @param {object} attrs - AngularJS attrs
 * @returns {object}
 */
function buildEditorOptions(model, scope, attrs) {
    return {
        model,
        theme: getThemeId(),
        automaticLayout: true,
        // ─── Appearance ───
        minimap: { enabled: false },
        fontSize: 14,
        fontFamily: "'Cascadia Code', 'Fira Code', 'Consolas', monospace",
        fontLigatures: true,
        lineHeight: 22,
        padding: { top: 8, bottom: 8 },
        renderLineHighlight: 'all',
        smoothScrolling: true,
        cursorBlinking: 'smooth',
        cursorSmoothCaretAnimation: 'on',
        bracketPairColorization: { enabled: true },
        matchBrackets: 'always',
        // ─── Behavior ───
        readOnly: scope.readOnly || false,
        scrollBeyondLastLine: false,
        wordWrap: 'on',
        detectIndentation: false, // keep our tabSize/insertSpaces authoritative
        tabSize: 4,
        insertSpaces: true,
        autoClosingBrackets: 'always',
        autoClosingQuotes: 'always',
        autoIndent: 'full',
        formatOnPaste: true,
        formatOnType: true,
        // ─── Suggest widget ───
        suggest: {
            showKeywords: true,
            showSnippets: true,
            showFunctions: true,
            showVariables: true,
            preview: true,
            filterGraceful: true,
            snippetsPreventQuickSuggestions: false,
            showIcons: true,
        },
        fixedOverflowWidgets: attrs.fixedOverflowWidgets !== 'false',
        // ─── Scrollbar ───
        scrollbar: {
            vertical: 'auto',
            horizontal: 'auto',
            verticalScrollbarSize: 10,
            horizontalScrollbarSize: 10,
        },
    };
}

// ─── Directive ───
export function MonacoDslEditor($timeout) {
    return {
        restrict: 'A',
        require: 'ngModel',
        scope: {
            suggestions: '=',
            moreSuggestions: '=',
            builtinFunctions: '=',
            editorId: '@',
            readOnly: '=?',
            onReady: '&?',
        },
        link(scope, element, attrs, ngModel) {
            // ─── Internal state ───
            let editor = null;
            let tabObserver = null;
            let isDestroyed = false;
            let suppressModelSync = false;

            // Unique id for this editor instance
            const editorId = scope.editorId || attrs.id || generateEditorId();
            
            // ─── Container styles ───
            element.css({
                height: attrs.height || '300px',
                display: 'block',
                border: '1px solid #333',
                borderRadius: '4px',
                overflow: 'hidden',
            });

            // ─── Register language (once, globally) ───
            registerDSLLanguage();

            // ─── Create editor ───
            function createEditor() {
                if (isDestroyed) return;
                // Create a dedicated model with a unique URI.
                //This is the key to fixing the cross-tab issue!
                const uniqueUri = monaco.Uri.parse(`inmemory://${editorId}.dsl`);
                const existingModel = monaco.editor.getModel(uniqueUri);
                if (existingModel) {
                    existingModel.dispose();
                }
                const model = monaco.editor.createModel(
                    ngModel.$viewValue || '',
                    getLanguageId(),
                    uniqueUri
                );
                editor = monaco.editor.create(
                    element[0],
                    buildEditorOptions(model, scope, attrs)
                );

                //Register in the registry with this editor's own suggestions
                registerEditor(
                    editorId,
                    editor,
                    scope.suggestions || {},
                    scope.builtinFunctions || [],
                    scope.moreSuggestions
                );

                // ─── editor → ngModel sync ───
                editor.onDidChangeModelContent(() => {
                    if (suppressModelSync || isDestroyed) return;
                    const newValue = editor.getValue();
                    $timeout(() => {
                        ngModel.$setViewValue(newValue);
                    });
                });

                // ─── Notify ready ───
                if (typeof scope.onReady === 'function') {
                    $timeout(() => {
                        scope.onReady({ editor, editorId });
                    });
                }
            }

            // ─── ngModel → editor sync ───
            ngModel.$render = function () {
                if (!editor || isDestroyed) return;

                const value = ngModel.$viewValue || '';
                if (editor.getValue() !== value) {
                    suppressModelSync = true;
                    editor.setValue(value);
                    suppressModelSync = false;
                }
            };

            // ─── React to `suggestions` changes ───
            // When `suggestions` changes from the outside, update the registry.
            scope.$watch('suggestions', (newVal) => {
                if (newVal && !isDestroyed) {
                    updateSuggestions(editorId, newVal);
                }
            }, true);

            // ─── Bootstrap Tabs visibility handling ───
            // When the active tab changes, the editor needs a re-layout.
            scope.$on('tab-changed', () => {
                if (editor && !isDestroyed) {
                    $timeout(() => editor.layout(), 100);
                }
            });

            /**
             * Also detect visibility changes via MutationObserver,
             * watching the nearest `.tab-pane` ancestor's `class` attribute.
             */
            function setupTabObserver() {
                const tabPane = element[0].closest('.tab-pane');
                if (!tabPane) return;
                tabObserver = new MutationObserver((mutations) => {
                    for (const mutation of mutations) {
                        if (mutation.attributeName !== 'class') continue;
                        const isActive = tabPane.classList.contains('active');
                        if (isActive && editor && !isDestroyed) {
                            $timeout(() => editor.layout(), 50);
                        }
                    }
                });

                tabObserver.observe(tabPane, {
                    attributes: true,
                    attributeFilter: ['class'],
                });
            }

            // ─── Complete cleanup ───
            scope.$on('$destroy', () => {
                isDestroyed = true;

                // Remove from registry
                unregisterEditor(editorId);

                // Disconnect observer
                if (tabObserver) {
                    tabObserver.disconnect();
                    tabObserver = null;
                }

                // Dispose model and editor
                if (editor) {
                    const model = editor.getModel();
                    if (model) {
                        model.dispose();
                    }
                    
                    editor.dispose();
                    editor = null;
                }
            });

            // ─── Initial bootstrap ───
            $timeout(() => {
                createEditor();
                setupTabObserver();
            }, 0);
        },
    };
}

MonacoDslEditor.$inject = ['$timeout'];
