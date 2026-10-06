/**
 * Monaco DSL Formatter & Auto-Indentation
 *
 * Indentation rules of this DSL:
 * - `begin` and `end` are always at the same level
 * - Content between `begin` and `end` is indented one level deeper
 * - `else` is at the same level as its corresponding `begin`/`end`
 * - `if` does not create indentation itself (the following `begin` does)
 *
 * Correct example:
 *   if User.Age > 18
 *   begin
 *       if User.IsActive == true
 *       begin
 *           Products = _ServiceResult.Items
 *           TotalCount = _ServiceResult.TotalCount
 *       end
 *       else
 *       begin
 *            Products = ParseJsonToArray("[]")
 *       end
 *   end
 */

import * as monaco from 'monaco-editor/esm/vs/editor/editor.api.js';

// ─── Constants ───
const INDENT_SIZE = 4;
const INDENT_CHAR = ' ';
const INDENT_UNIT = INDENT_CHAR.repeat(INDENT_SIZE);

// ─── Language Configuration (Auto-Indent) ───
export const languageConfiguration = {
    brackets: [
        ['begin', 'end'],
        ['(', ')'],
        ['[', ']'],
        ['{', '}'],
    ],
    autoClosingPairs: [
        { open: '(', close: ')' },
        { open: '[', close: ']' },
        { open: '{', close: '}' },
        { open: '"', close: '"', notIn: ['string'] },
        { open: "'", close: "'", notIn: ['string'] },
    ],
    surroundingPairs: [
        { open: '(', close: ')' },
        { open: '[', close: ']' },
        { open: '{', close: '}' },
        { open: '"', close: '"' },
        { open: "'", close: "'" },
    ],
    comments: {
        lineComment: '//',
        blockComment: ['/*', '*/'],
    },
    folding: {
        markers: {
            start: /^\s*begin\s*$/i,
            end: /^\s*end\s*$/i,
        },
    },
    // ─── Indentation rules ───
    indentationRules: {
        //Only `begin` increases the indent (note: `if` does not)
        increaseIndentPattern: /^\s*begin\s*$/i,
        //`end` and `else` decrease the indent (aligned with `begin`)
        decreaseIndentPattern: /^\s*(end|else)\s*$/i,
        unIndentedLinePattern: /^\s*\/\/.*/,
    },

    // ─── Enter-key rules ───
    onEnterRules: [
        // After `begin` → add indent
        {
            beforeText: /^\s*begin\s*$/i,
            afterText: /^$/,
            action: {
                indentAction: monaco.languages.IndentAction.Indent,
            },
        },
        // Between `begin` and `end` → indent + outdent
        {
            beforeText: /^\s*begin\s*$/i,
            afterText: /^\s*end\s*$/i,
            action: {
                indentAction: monaco.languages.IndentAction.IndentOutdent,
            },
        },

        //After `if` → no extra indent
        //    (the next-line `begin` handles it)
        //After `else` → no extra indent
        //    (the next-line `begin` handles it)
    ],
    wordPattern: /(-?\d*\.\d\w*)|([^\`\~\!\@\#\%\^\&\*\(\)\-\=\+\[\{\]\}\\\|\;\:\'\"\,\.\<\>\/\?\s]+)/g,
};

// ─── Document Formatting Provider (Alt+Shift+F) ───
export const documentFormattingProvider = {
    provideDocumentFormattingEdits(model, options) {
        const originalText = model.getValue();
        const formattedText = formatDSLCode(originalText, options);
        if (originalText === formattedText) return [];
        return [{
            range: model.getFullModelRange(),
            text: formattedText,
        }];
    },
};

// ─── Range Formatting Provider ───
export const rangeFormattingProvider = {
    provideDocumentRangeFormattingEdits(model, range, options) {
        const text = model.getValueInRange(range);
        const formatted = formatDSLCode(text, options);
        if (text === formatted) return [];

        return [{ range, text: formatted }];
    },
};

// ─── On-Type Formatting Provider ───
export const onTypeFormattingProvider = {
    autoFormatTriggerCharacters: ['d', 'n'],
    provideOnTypeFormattingEdits(model, position, ch, options) {
        const lineContent = model.getLineContent(position.lineNumber);
        const trimmed = lineContent.trim().toLowerCase();

        // When `end` or `begin` was just typed → compute correct indent
        if (trimmed === 'end' || trimmed === 'begin') {
            const correctIndent = calculateIndentForLine(model, position.lineNumber);
            const newLine = correctIndent + trimmed;
            if (newLine !== lineContent) {
                return [{
                    range: {
                        startLineNumber: position.lineNumber,
                        startColumn: 1,
                        endLineNumber: position.lineNumber,
                        endColumn: lineContent.length + 1,
                    },
                    text: newLine,
                }];
            }
        }
        return [];
    },
};

// ══════════════════════════════════════════════
//  Core formatting engine
// ══════════════════════════════════════════════
/**
 * Fully format DSL code.
 *
 * Main rule:
 * - `begin` / `end` / `else` are always at the same level
 * - Only the content inside `begin`...`end` is indented one level deeper
 * - `if` sits at the same level as the following `begin`
 *
 * @param {string} code - Raw source code
 * @param {object} options - Formatting options
 * @returns {string} Formatted code
 */
function formatDSLCode(code, options) {
    if (!code || code.trim() === '') return code;
    const tabSize = options?.tabSize ?? INDENT_SIZE;
    const insertSpaces = options?.insertSpaces ?? true;
    const indentStr = insertSpaces ? ' '.repeat(tabSize) : '\t';
    const lines = code.split('\n');
    const result = [];
    let indentLevel = 0;
    for (const line of lines) {
        const trimmed = line.trim();
        // Empty line → collapse consecutive blank lines to one
        if (trimmed === '') {
            if (result.length > 0 && result[result.length - 1].trim() === '') {
                continue;
            }
            result.push('');
            continue;
        }

        const trimmedLower = trimmed.toLowerCase();

        // ──────────────────────────────────────
        // Step 1: Before emitting the line,
        //         should we decrease indent?
        // ──────────────────────────────────────
        // `end` → go back to the `begin` level
        // `else` → go back to the `begin`/`end` level
        if ((trimmedLower === 'end' || trimmedLower === 'else') && indentLevel > 0) {
            indentLevel--;
        }

        // ──────────────────────────────────────
        // Step 2: Emit the line with current indent
        // ──────────────────────────────────────
        const formattedLine = indentStr.repeat(indentLevel) + formatLineContent(trimmed);
        result.push(formattedLine);

        // ──────────────────────────────────────
        // Step 3: After emitting the line,
        //         should we increase indent?
        // ──────────────────────────────────────
        //Only `begin` increases indent.
        //    `if` and `else` do NOT create indentation themselves
        //    (`else` only outdents; the subsequent `begin` handles indenting).
        if (trimmedLower === 'begin') {
            indentLevel++;
        }
    }

    // Strip trailing empty lines
    while (result.length > 0 && result[result.length - 1].trim() === '') {
        result.pop();
    }

    return result.join('\n');
}

/**
 * Compute the correct indent for a specific line.
 * (Used by on-type formatting.)
*/
function calculateIndentForLine(model, lineNumber) {
    let indentLevel = 0;
    for (let i = 1; i < lineNumber; i++) {
        const content = model.getLineContent(i).trim().toLowerCase();
        if (content === '') continue;

        // `end` / `else` → outdent
        if ((content === 'end' || content === 'else') && indentLevel > 0) {
            indentLevel--;
        }

        // `begin` → indent (after the line)
        if (content === 'begin') {
            indentLevel++;
        }
    }

    // Current line
    const currentLine = model.getLineContent(lineNumber).trim().toLowerCase();
    if ((currentLine === 'end' || currentLine === 'else') && indentLevel > 0) {
        indentLevel--;
    }

    // `begin` should align with the preceding `if` (no extra indent),
    // so no special handling is required here.
    return INDENT_UNIT.repeat(indentLevel);
}

// ─── Content-formatting helpers ───
/**
 * Format the content of a single line (no indent changes).
*/
function formatLineContent(line) {
    if (!line) return line;

    let formatted = normalizeSpaces(line);
    formatted = formatOperators(formatted);
    formatted = formatted.replace(/,\s*/g, ', ');
    return formatted;
}

/**
 * Normalize whitespace (preserving string literals).
*/
function normalizeSpaces(line) {
    let result = '';
    let inString = false;
    let stringChar = '';
    let prevChar = '';
    for (const ch of line) {
        if ((ch === '"' || ch === "'") && prevChar !== '\\') {
            if (!inString) {
                inString = true;
                stringChar = ch;
            } else if (ch === stringChar) {
                inString = false;
            }
        }

        if (inString) {
            result += ch;
        } else if (!(ch === ' ' && prevChar === ' ')) {
            result += ch;
        }

        prevChar = ch;
    }

    return result;
}
/**
 * Add proper spacing around operators.
 */
function formatOperators(line) {
    const TWO_CHAR_OPS = new Set(['==', '!=', '>=', '<=', '&&', '||']);
    let result = '';
    let inString = false;
    let stringChar = '';
    let i = 0;
    while (i < line.length) {
        const ch = line[i];
        const next = i + 1 < line.length ? line[i + 1] : '';
        if ((ch === '"' || ch === "'") && (i === 0 || line[i - 1] !== '\\')) {
            if (!inString) {
                inString = true;
                stringChar = ch;
            } else if (ch === stringChar) {
                inString = false;
            }
        }

        if (inString) {
            result += ch;
            i++;
            continue;
        }

        // ─── Two-character operators ───
        const twoChar = ch + next;
        if (TWO_CHAR_OPS.has(twoChar)) {
            // Space before (if missing)
            if (result.length > 0 && result[result.length - 1] !== ' ') {
                result += ' ';
            }

            result += twoChar;

            // Space after (if missing)
            if (i + 2 < line.length && line[i + 2] !== ' ') {
                result += ' ';
            }

            i += 2;
            continue;
        }

        // ─── Single-character `=` operator ───
        // Only a plain `=` (not part of ==, !=, >=, <=)
        if (ch === '=' && next !== '=') {
            const prevResultChar = result.length > 0 ? result[result.length - 1] : '';
            if (prevResultChar !== '!' && prevResultChar !== '>' && prevResultChar !== '<') {
                if (prevResultChar !== ' ' && result.length > 0) {
                    result += ' ';
                }

                result += ch;
                if (next !== ' ' && next !== '') {
                    result += ' ';
                }
                
                i++;
                continue;
            }
        }

        // ─── Single-character `>` and `<` operators ───
        // Only when not followed by `=`
        if ((ch === '>' || ch === '<') && next !== '=') {
            if (result.length > 0 && result[result.length - 1] !== ' ') {
                result += ' ';
            }

            result += ch;
            if (next !== ' ' && next !== '') {
                result += ' ';
            }

            i++;
            continue;
        }

        result += ch;
        i++;
    }

    return result;
}

// ─── Provider registration ───
/**
 * Register the formatter and language configuration.
 *
 * @param {string} languageId - Identifier of the DSL language
 */
export function registerDSLFormatter(languageId) {
    // 1. Language Configuration (auto-indent + brackets)
    monaco.languages.setLanguageConfiguration(
        languageId,
        languageConfiguration
    );

    // 2. Document Formatting (Alt+Shift+F)
    monaco.languages.registerDocumentFormattingEditProvider(
        languageId,
        documentFormattingProvider
    );

    // 3. Range Formatting (format a portion of the code)
    monaco.languages.registerDocumentRangeFormattingEditProvider(
        languageId,
        rangeFormattingProvider
    );
    
    // 4. On-Type Formatting (format while typing)
    monaco.languages.registerOnTypeFormattingEditProvider(
        languageId,
        onTypeFormattingProvider
    );
}
