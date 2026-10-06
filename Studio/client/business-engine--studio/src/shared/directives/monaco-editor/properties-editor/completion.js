import * as monaco from 'monaco-editor/esm/vs/editor/editor.api.js';
import { BPropertiesContext } from './editor.directive';

export function registerBPropertiesCompletion() {
    monaco.languages.registerCompletionItemProvider('bProperties', {
        triggerCharacters: ['.', '(', '['],
        provideCompletionItems(model, position) {
            const context = BPropertiesContext.get(model.uri.toString());
            if (!context) return { suggestions: [] };
            const { suggestions, globalFunctions } = context;
            return buildCompletion(model, position, suggestions, globalFunctions);
        }
    });
}

function buildCompletion(model, position, suggestionsTree, globalFunctions) {
    const textUntilCursor = model.getValueInRange({
        startLineNumber: position.lineNumber,
        startColumn: 1,
        endLineNumber: position.lineNumber,
        endColumn: position.column
    });
    const word = model.getWordUntilPosition(position);
    const match = textUntilCursor.match(/([a-zA-Z0-9_\.\[\]]+)$/);
    const fullPath = match ? match[1] : '';
    const isMemberAccess = fullPath.includes('.');
    const pathParts = fullPath.split('.');
    const parentPath = isMemberAccess
        ? pathParts.slice(0, -1).join('.')
        : null;
    let targetObject;
    if (parentPath) {
        targetObject = resolvePath(suggestionsTree, parentPath);
    } else {
        targetObject = suggestionsTree;
    }
    const completionItems = [];
    if (targetObject && typeof targetObject === 'object') {
        if (Array.isArray(targetObject)) {
            const firstItem = targetObject[0];
            if (firstItem && typeof firstItem === 'object') {
                Object.keys(firstItem).forEach(key => {
                    completionItems.push(createItem(key, firstItem[key], position, word));
                });
            }
        } else {
            Object.keys(targetObject).forEach(key => {
                completionItems.push(createItem(key, targetObject[key], position, word));
            });
        }
    }
    // ✅ Global functions
    if (!parentPath && globalFunctions) {
        Object.entries(globalFunctions).forEach(([name, meta]) => {
            completionItems.push({
                label: name,
                kind: monaco.languages.CompletionItemKind.Function,
                insertText: `${name}(`,
                detail: 'Global Function',
                documentation: `${meta.description}\n\nArguments: (${meta.args.join(', ')})`,
                range: {
                    startLineNumber: position.lineNumber,
                    endLineNumber: position.lineNumber,
                    startColumn: word.startColumn,
                    endColumn: word.endColumn
                }
            });
        });
    }
    return { suggestions: completionItems };
}

function resolvePath(obj, path) {
    if (!path) return obj;
    const parts = path
        .replace(/\[(\d+)\]/g, '.$1')
        .split('.')
        .filter(Boolean);
    let current = obj;
    for (let part of parts) {
        if (current && part in current) {
            current = current[part];
        } else {
            return null;
        }
    }
    return current;
}

function createItem(key, value, position, word) {
    let kind = monaco.languages.CompletionItemKind.Property;
    if (typeof value === 'number')
        kind = monaco.languages.CompletionItemKind.Value;
    else if (typeof value === 'boolean')
        kind = monaco.languages.CompletionItemKind.Value;
    else if (Array.isArray(value))
        kind = monaco.languages.CompletionItemKind.Class;
    else if (typeof value === 'object')
        kind = monaco.languages.CompletionItemKind.Class;
    return {
        label: key,
        kind,
        insertText: key,
        detail: getTypeLabel(value),
        range: {
            startLineNumber: position.lineNumber,
            endLineNumber: position.lineNumber,
            startColumn: word.startColumn,
            endColumn: word.endColumn
        }
    };
}

function getTypeLabel(val) {
    if (Array.isArray(val)) return 'Array';
    if (val === null) return 'null';
    return typeof val;
}