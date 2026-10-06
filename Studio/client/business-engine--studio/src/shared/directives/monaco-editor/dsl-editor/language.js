/**
 * Monaco DSL Language Definition
 *
 * Includes:
 * - Monarch Tokenizer (professional syntax highlighting)
 * - Theme (custom dark theme)
 * - Completion Provider (smart, per-editor)
 * - Hover Provider
 * - Folding Provider
 */

import * as monaco from 'monaco-editor/esm/vs/editor/editor.api.js';

import { getBuiltinFunctions, getSuggestionsForModel, isRegistered, markAsRegistered } from './registry';
import { registerDSLFormatter } from './formatter';

// ─── Constants ───
const LANGUAGE_ID = 'myDSL';
const THEME_ID = 'myDSLThemePro';

// Language keywords
const KEYWORDS = [
    'if', 'else', 'begin', 'end',
    'and', 'or', 'not',
    'true', 'false', 'null',
];

// Built-in functions of the language
const BUILTIN_FUNCTIONS = [
    'GetImage', 'Log', 'ToString', 'ToInt',
    'Contains', 'StartsWith', 'EndsWith',
    'Count', 'Sum', 'Max', 'Min',
    'Format', 'Replace', 'Trim',
    'Now', 'Today', 'AddDays',
];

// Operators
const OPERATORS = [
    '==', '!=', '>=', '<=', '>', '<',
    '=', '+', '-', '*', '/',
    '!', '&&', '||',
];

// Lookup sets for O(1) membership checks
const KEYWORDS_SET = new Set(KEYWORDS);
const BUILTIN_FUNCTIONS_SET = new Set(BUILTIN_FUNCTIONS);

// ─── Monarch Tokenizer (professional syntax highlighting) ───
const monarchTokensProvider = {
    keywords: KEYWORDS,
    builtinFunctions: BUILTIN_FUNCTIONS,
    operators: OPERATORS,
    // Common regex definitions
    symbols: /[=><!~?:&|+\-*\/\^%]+/,
    escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4})/,
    tokenizer: {
        root: [
            // ═══ Comments ═══
            [/\/\/.*$/, 'comment'],
            [/\/\*/, 'comment', '@comment'],
            // ═══ Function calls ═══
            // Any identifier immediately followed by `(` is treated as a function
            [/[A-Z][\w]*(?=\s*\()/, {
                cases: {
                    '@builtinFunctions': 'builtin-function',
                    '@default': 'function-call',
                },
            }],
            // ═══ Property access (after a dot) ═══
            // e.g. `User.Name` → highlights `Name` as a property
            [/\.([A-Za-z_]\w*)/, 'property'],
            // ═══ Word identification ═══
            [/[a-zA-Z_]\w*/, {
                cases: {
                    '@keywords': 'keyword',
                    '@builtinFunctions': 'builtin-function',
                    'true': 'boolean',
                    'false': 'boolean',
                    'null': 'null-literal',
                    '@default': 'identifier',
                },
            }],
            // ═══ Numbers ═══
            [/\d+\.\d+/, 'number.float'],
            [/\d+/, 'number'],
            // ═══ Strings ═══
            [/"/, 'string.quote', '@string_double'],
            [/'/, 'string.quote', '@string_single'],
            // ═══ Operators ═══
            [/@symbols/, {
                cases: {
                    '@operators': 'operator',
                    '@default': 'delimiter',
                },
            }],
            // ═══ Brackets & delimiters ═══
            [/[{}()\[\]]/, 'bracket'],
            [/[,;]/, 'delimiter'],
            // ═══ Whitespace ═══
            [/\s+/, 'white'],
        ],
        // ── Double-quoted string ──
        string_double: [
            [/[^\\"]+/, 'string'],
            [/@escapes/, 'string.escape'],
            [/\\./, 'string.escape.invalid'],
            [/"/, 'string.quote', '@pop'],
        ],
        // ── Single-quoted string ──
        string_single: [
            [/[^\\']+/, 'string'],
            [/@escapes/, 'string.escape'],
            [/\\./, 'string.escape.invalid'],
            [/'/, 'string.quote', '@pop'],
        ],
        // ── Multi-line comment ──
        comment: [
            [/[^\/*]+/, 'comment'],
            [/\*\//, 'comment', '@pop'],
            [/[\/*]/, 'comment'],
        ],
    },
};

// ─── Professional theme (VS Code Dark+ inspired) ───
const themeDefinition = {
    base: 'vs-dark',
    inherit: true,
    rules: [
        // Keywords — purple/pink (like VS Code)
        { token: 'keyword', foreground: 'C586C0', fontStyle: 'bold' },
        // Built-in functions — golden yellow
        { token: 'builtin-function', foreground: 'DCDCAA', fontStyle: 'bold' },
        // Function calls — light yellow
        { token: 'function-call', foreground: 'DCDCAA' },
        // Properties (after a dot) — teal
        { token: 'property', foreground: '4EC9B0' },
        // Identifiers (variables) — light blue
        { token: 'identifier', foreground: '9CDCFE' },
        // Strings — warm orange
        { token: 'string', foreground: 'CE9178' },
        { token: 'string.quote', foreground: 'CE9178' },
        { token: 'string.escape', foreground: 'D7BA7D' },
        { token: 'string.escape.invalid', foreground: 'F44747' },
        // Numbers — light green
        { token: 'number', foreground: 'B5CEA8' },
        { token: 'number.float', foreground: 'B5CEA8' },
        // Booleans & null — strong blue
        { token: 'boolean', foreground: '569CD6', fontStyle: 'bold' },
        { token: 'null-literal', foreground: '569CD6', fontStyle: 'italic' },
        // Operators — light gray / white
        { token: 'operator', foreground: 'D4D4D4' },
        // Brackets — gold
        { token: 'bracket', foreground: 'FFD700' },
        // Delimiters
        { token: 'delimiter', foreground: 'BBBBBB' },
        // Comments — dark green
        { token: 'comment', foreground: '6A9955', fontStyle: 'italic' },
    ],
    colors: {
        'editor.background': '#1E1E2E',
        'editor.foreground': '#D4D4D4',
        'editor.lineHighlightBackground': '#2A2A3E',
        'editorCursor.foreground': '#AEAFAD',
        'editor.selectionBackground': '#264F78',
        'editor.inactiveSelectionBackground': '#3A3D41',
        'editorLineNumber.foreground': '#858585',
        'editorLineNumber.activeForeground': '#C6C6C6',
        'editorIndentGuide.background': '#404040',
        'editorBracketMatch.background': '#0064001A',
        'editorBracketMatch.border': '#888888',
    },
};

// ─── Completion Provider (smart, per-editor) ───
/**
 * Uses the registry to return suggestions specific
 * to the currently active editor/model.
 */
const completionProvider = {
    triggerCharacters: ['.'],
    provideCompletionItems(model, position) {
        // Key to solving the per-tab issue: pull suggestions from the registry
        const suggestions = getSuggestionsForModel(model);
        const builtinFunctions = getBuiltinFunctions();
        const textUntilPosition = model.getValueInRange({
            startLineNumber: position.lineNumber,
            startColumn: 1,
            endLineNumber: position.lineNumber,
            endColumn: position.column,
        });
        const wordInfo = model.getWordUntilPosition(position);

        // Replacement range for the suggestion insertion
        const range = {
            startLineNumber: position.lineNumber,
            endLineNumber: position.lineNumber,
            startColumn: wordInfo.startColumn,
            endColumn: wordInfo.endColumn,
        };

        // ─── Detect a dot-chain ───
        // e.g. `User.Profile.Name` — figure out how deep we are
        const dotChainMatch = textUntilPosition.match(
            /([A-Za-z_]\w*(?:\.[A-Za-z_]\w*)*)\.$/
        );

        if (dotChainMatch) {
            // The user just typed a dot — show children
            const chain = dotChainMatch[1].split('.');
            const resolvedChildren = resolveChain(suggestions, chain);
            if (resolvedChildren) {
                return {
                    suggestions: buildChildSuggestions(
                        resolvedChildren,
                        chain[chain.length - 1],
                        range
                    ),
                };
            }
            return { suggestions: [] };
        }

        // ─── Detect a dot in the middle of typing ───
        // e.g. `User.Na` (still typing)
        const partialDotMatch = textUntilPosition.match(
            /([A-Za-z_]\w*(?:\.[A-Za-z_]\w*)*)\.([A-Za-z_]\w*)$/
        );

        if (partialDotMatch) {
            const chain = partialDotMatch[1].split('.');
            const resolvedChildren = resolveChain(suggestions, chain);
            if (resolvedChildren) {
                return {
                    suggestions: buildChildSuggestions(
                        resolvedChildren,
                        chain[chain.length - 1],
                        range
                    ),
                };
            }
            return { suggestions: [] };
        }

        // ─── General case (no dot) ───
        // Show top-level variables + keywords + built-in functions
        const result = [];

        // 1. Top-level variables from suggestions
        for (const [key, value] of Object.entries(suggestions)) {
            let kind;
            let detail;
            let insertTextValue;
            if (typeof value === 'function') {
                kind = monaco.languages.CompletionItemKind.Function;
                detail = '🔧 Function';
                insertTextValue = `${key}($0)`;
            } else if (Array.isArray(value)) {
                kind = monaco.languages.CompletionItemKind.Enum;
                detail = `📋 Collection [${value.length}]`;
                insertTextValue = key;
            } else if (typeof value === 'object' && value !== null) {
                kind = monaco.languages.CompletionItemKind.Class;
                detail = `📦 Object {${Object.keys(value).length} props}`;
                insertTextValue = key;
            } else if (typeof value === 'boolean') {
                kind = monaco.languages.CompletionItemKind.Value;
                detail = '✅ Boolean';
                insertTextValue = key;
            } else if (typeof value === 'number') {
                kind = monaco.languages.CompletionItemKind.Value;
                detail = '🔢 Number';
                insertTextValue = key;
            } else if (typeof value === 'string') {
                kind = monaco.languages.CompletionItemKind.Value;
                detail = '📝 String';
                insertTextValue = key;
            } else {
                kind = monaco.languages.CompletionItemKind.Variable;
                detail = 'Variable';
                insertTextValue = key;
            }

            result.push({
                label: key,
                kind,
                insertText: insertTextValue,
                insertTextRules: insertTextValue.includes('$0')
                    ? monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                    : undefined,
                detail,
                range,
                sortText: `1_${key}`, // higher priority than keywords
            });
        }

        // 2. Language keywords
        const keywordSnippets = {
            'if': 'if ${1:condition}\nbegin\n\t$0\nend',
            'begin': 'begin\n\t$0\nend',
        };

        for (const kw of KEYWORDS) {
            const snippet = keywordSnippets[kw];
            result.push({
                label: kw,
                kind: monaco.languages.CompletionItemKind.Keyword,
                insertText: snippet || kw,
                insertTextRules: snippet
                    ? monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                    : undefined,
                detail: '🔑 Keyword',
                range,
                sortText: `2_${kw}`,
            });
        }

        // 3. Built-in functions
        for (const fn of builtinFunctions) {
            result.push({
                label: fn,
                kind: monaco.languages.CompletionItemKind.Function,
                insertText: `${fn}($0)`,
                insertTextRules:
                    monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                detail: '⚡ Built-in Function',
                range,
                sortText: `3_${fn}`,
            });
        }

        return { suggestions: result };
    },
};

// ─── Helpers ───
/**
 * Walk a dot-chain through the suggestions tree.
 * e.g. ['User', 'Profile'] → suggestions.User.Profile
 *
 * @param {object} root  - Root suggestions object
 * @param {string[]} chain - Array of keys to traverse
 * @returns {object|null} The children object, or null if unresolved
 */
function resolveChain(root, chain) {
    let current = root;
    for (const key of chain) {
        if (current === null || current === undefined) {
            return null;
        }

        // If current is an array, step into the first item's fields
        if (Array.isArray(current)) {
            if (current.length > 0) {
                current = current[0];
            } else {
                return null;
            }
        }

        if (typeof current === 'object' && Object.prototype.hasOwnProperty.call(current, key)) {
            current = current[key];
        } else {
            return null;
        }
    }
    // If the final result is an array, surface the first item's fields
    if (Array.isArray(current)) {
        if (current.length > 0 && typeof current[0] === 'object') {
            return current[0];
        }
        return null;
    }
    
    if (typeof current === 'object' && current !== null) {
        return current;
    }
    return null;
}

/**
 * Build a list of suggestions for the children of an object.
 *
 * @param {object} children - Children object
 * @param {string} parentName - Parent's name (used in `detail`)
 * @param {object} range - Replacement range
 * @returns {Array} List of suggestion items
 */
function buildChildSuggestions(children, parentName, range) {
    const result = [];
    for (const [key, value] of Object.entries(children)) {
        let kind;
        let detail;
        if (typeof value === 'function') {
            kind = monaco.languages.CompletionItemKind.Method;
            detail = `🔧 Method of ${parentName}`;
        } else if (Array.isArray(value)) {
            kind = monaco.languages.CompletionItemKind.Enum;
            detail = `📋 Collection of ${parentName}`;
        } else if (typeof value === 'object' && value !== null) {
            kind = monaco.languages.CompletionItemKind.Property;
            detail = `📦 Object in ${parentName}`;
        } else if (typeof value === 'boolean') {
            kind = monaco.languages.CompletionItemKind.Property;
            detail = `✅ Bool property of ${parentName}`;
        } else if (typeof value === 'number') {
            kind = monaco.languages.CompletionItemKind.Property;
            detail = `🔢 Number property of ${parentName}`;
        } else {
            kind = monaco.languages.CompletionItemKind.Property;
            detail = `📝 Property of ${parentName}`;
        }
        result.push({
            label: key,
            kind,
            insertText: key,
            detail,
            range,
        });
    }
    return result;
}

// ─── Hover Provider ───
const KEYWORD_HELP = {
    'if': '🔑 **if** — Conditional branch. If the condition holds, the `begin...end` block runs.',
    'else': '🔑 **else** — Alternative block executed when the condition is false.',
    'begin': '🔑 **begin** — Start of a code block.',
    'end': '🔑 **end** — End of a code block.',
    'and': '🔑 **and** — Logical AND operator.',
    'or': '🔑 **or** — Logical OR operator.',
    'not': '🔑 **not** — Logical NOT operator.',
    'true': '✅ **true** — Boolean true value.',
    'false': '❌ **false** — Boolean false value.',
    'null': '⬜ **null** — Null (empty) value.',
};

const hoverProvider = {
    provideHover(model, position) {
        const word = model.getWordAtPosition(position);
        if (!word) return null;

        const suggestions = getSuggestionsForModel(model);
        const wordText = word.word;
        const contents = [];

        // Check against keywords
        if (KEYWORDS_SET.has(wordText) && KEYWORD_HELP[wordText]) {
            contents.push({ value: KEYWORD_HELP[wordText] });
        }

        // Check against built-in functions
        if (BUILTIN_FUNCTIONS_SET.has(wordText)) {
            contents.push({
                value: `⚡ **${wordText}(...)** — Built-in system function`,
            });
        }

        // Check against user-provided suggestions (variables)
        if (Object.prototype.hasOwnProperty.call(suggestions, wordText)) {
            const val = suggestions[wordText];
            const typeStr = Array.isArray(val)
                ? 'Collection'
                : typeof val === 'object'
                    ? 'Object'
                    : typeof val;
            contents.push({
                value: `📦 **${wordText}** — ${typeStr}`,
            });

            // Show child fields
            if (typeof val === 'object' && val !== null) {
                const target = Array.isArray(val) && val.length > 0 ? val[0] : val;
                if (typeof target === 'object' && target !== null) {
                    const fields = Object.keys(target).join(', ');
                    contents.push({
                        value: `  Fields: \`${fields}\``,
                    });
                }
            }
        }

        if (contents.length === 0) return null;

        return {
            range: {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: word.startColumn,
                endColumn: word.endColumn,
            },
            contents,
        };
    },
};

// ─── Folding Provider ───
const foldingProvider = {
    provideFoldingRanges(model) {
        const ranges = [];
        const lineCount = model.getLineCount();
        const stack = [];
        for (let i = 1; i <= lineCount; i++) {
            const text = model.getLineContent(i).trim().toLowerCase();
            if (text === 'begin') {
                stack.push(i);
            } else if (text === 'end' && stack.length > 0) {
                const start = stack.pop();
                ranges.push({
                    start,
                    end: i,
                    kind: monaco.languages.FoldingRangeKind.Region,
                });
            }
        }

        return ranges;
    },
};

// ─── Main language registration (runs only once) ───
/**
 * Register the DSL language with Monaco.
 *
 * This function runs once and registers all providers globally.
 * Differences in `suggestions` between tabs are handled by the registry.
 */
export function registerDSLLanguage() {
    if (isRegistered()) return;

    monaco.languages.register({ id: LANGUAGE_ID });
    monaco.languages.setMonarchTokensProvider(
        LANGUAGE_ID,
        monarchTokensProvider
    );
    monaco.editor.defineTheme(THEME_ID, themeDefinition);
    monaco.languages.registerCompletionItemProvider(
        LANGUAGE_ID,
        completionProvider
    );
    monaco.languages.registerHoverProvider(
        LANGUAGE_ID,
        hoverProvider
    );
    monaco.languages.registerFoldingRangeProvider(
        LANGUAGE_ID,
        foldingProvider
    );

    // Register formatter and auto-indent support
    registerDSLFormatter(LANGUAGE_ID);
    markAsRegistered();
}

/**
 * Get the language identifier.
 * @returns {string}
 */
export function getLanguageId() {
    return LANGUAGE_ID;
}

/**
 * Get the theme identifier.
 * @returns {string}
 */
export function getThemeId() {
    return THEME_ID;
}

/**
 * Get the list of language keywords.
 * @returns {string[]}
 */
export function getKeywords() {
    return KEYWORDS;
}

