export const monacoDefaultOptions = {
    bProperties: {
        lineNumbers: "off",
        fontSize: "16px",
        lineHeight: 2.2,
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
};

export const globalFunctions = {
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