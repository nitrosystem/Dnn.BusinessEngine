export const monacoDefaultOptions = {
    bProperties: {
        lineNumbers: "off",
        fontSize: "16px",
        lineHeight: 1.9,
        glyphMargin: false,
        lineDecorationsWidth: 0,
        lineNumbersMinChars: 0,
        renderLineHighlight: "none",
        minimap: {
            enabled: false,
        },
        overviewRulerLanes: 0,
        scrollbar: {
            vertical: "hidden",
            horizontal: "hidden",
            handleMouseWheel: false,
        },
        wordWrap: "off",
        tabCompletion: 'off',       // مهم: نذار خودش تکمیل کنه
        tabFocusMode: false
    },
    bConditionalExpressions: {
        lineNumbers: "off",
        lineHeight: 1.8,
        glyphMargin: false,
        lineDecorationsWidth: 0,
        lineNumbersMinChars: 0,
        minimap: {
            enabled: false,
        },
        overviewRulerLanes: 0,
    },
};

export const globalFunctions = {
    New: {
        fn: () => { },
        args: [],
        description: "Create new instance from object."
    },
    Length: {
        fn: (arr) => arr.length,
        args: ["array"],
        description: "Returns the length of the given array."
    },
    Random: {
        fn: (min, max) => Math.floor(Math.random() * (max - min + 1)) + min,
        args: ["min", "max"],
        description: "Returns a random number between min and max."
    },
    Today: {
        fn: () => new Date().toISOString().split("T")[0],
        args: [],
        description: "Returns today's date."
    }
};

