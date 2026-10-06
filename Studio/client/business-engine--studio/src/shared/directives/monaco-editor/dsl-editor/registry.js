/**
 * Monaco DSL Editor Registry
 *
 * Acts as a bridge between individual directives and the
 * global completion provider.
 *
 * Every editor instance is registered under a unique id and
 * carries its own `suggestions` object.
 */

// Holds per-editor metadata
const editorRegistry = new Map();

// Built-in functions are shared across all editors, so keep them
// in a dedicated slot instead of mixing them into the editor map.
let builtinFunctionsList = [];

// Has the language already been registered with Monaco?
let isLanguageRegistered = false;

/**
 * Register an editor instance in the registry.
 *
 * @param {string} editorId - Unique identifier for the editor
 * @param {object} editorInstance - The Monaco editor instance
 * @param {object} [suggestions] - Editor-specific suggestions object
 * @param {string[]} [builtinFunctions] - Shared list of built-in functions
 * @param {object} [moreSuggestions] - Extra suggestions merged on top
 */
export function registerEditor(
    editorId,
    editorInstance,
    suggestions,
    builtinFunctions,
    moreSuggestions
) {
    const mergedSuggestions = moreSuggestions
        ? { ...suggestions, ...moreSuggestions }
        : (suggestions || {});

    editorRegistry.set(editorId, {
        editor: editorInstance,
        suggestions: mergedSuggestions,
    });

    if (builtinFunctions) {
        builtinFunctionsList = builtinFunctions;
    }
}

/**
 * Remove an editor from the registry (cleanup).
 *
 * @param {string} editorId
 */
export function unregisterEditor(editorId) {
    editorRegistry.delete(editorId);
}

/**
 * Update the suggestions of a specific editor.
 *
 * @param {string} editorId
 * @param {object} newSuggestions
 */
export function updateSuggestions(editorId, newSuggestions) {
    const entry = editorRegistry.get(editorId);
    if (entry) {
        entry.suggestions = newSuggestions || {};
    }
}

/**
 * Find the suggestions object for the editor whose model
 * autocomplete is currently being triggered on.
 *
 * Lookup is done by comparing model URIs.
 *
 * @param {object} model - Monaco model
 * @returns {object} The matching suggestions object (empty if none)
 */
export function getSuggestionsForModel(model) {
    const modelUri = model.uri.toString();
    for (const entry of editorRegistry.values()) {
        const editorModel = entry.editor?.getModel?.();
        if (editorModel && editorModel.uri.toString() === modelUri) {
            return entry.suggestions;
        }
    }
    return {};
}

/**
 * Get the shared list of built-in functions.
 *
 * @returns {string[]}
 */
export function getBuiltinFunctions() {
    return builtinFunctionsList;
}

/**
 * Get a specific editor's registry entry.
 *
 * @param {string} editorId
 * @returns {object|undefined}
 */
export function getEditorEntry(editorId) {
    return editorRegistry.get(editorId);
}

/**
 * Check whether the language has already been registered.
 *
 * @returns {boolean}
 */
export function isRegistered() {
    return isLanguageRegistered;
}

/**
 * Mark the language as registered.
 */
export function markAsRegistered() {
    isLanguageRegistered = true;
}
