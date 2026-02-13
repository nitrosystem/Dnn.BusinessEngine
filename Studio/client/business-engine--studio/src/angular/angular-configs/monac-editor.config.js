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
    bDSLScript: {
        lineNumbers: "off",              // خط‌ها نشون داده نشه
        fontSize: 16,                    // اندازه فونت مناسب
        lineHeight: 1.9,                 // فاصله خطوط
        glyphMargin: false,              // هیچ گلیف و مارک حاشیه‌ای نیست
        lineDecorationsWidth: 0,         // پهنای دکوراسیون خط صفر
        lineNumbersMinChars: 0,
        renderLineHighlight: "none",     // هایلایت خط غیر فعال
        minimap: { enabled: false },     // مینی مپ غیرفعال
        overviewRulerLanes: 0,
        scrollbar: {
            vertical: "hidden",
            horizontal: "hidden",
            handleMouseWheel: false,
        },
        wordWrap: "off",                 // wrap کلمات غیرفعال
        tabCompletion: "off",            // تکمیل خودکار تب غیرفعال
        tabFocusMode: false,
        automaticLayout: true,           // خودکار تنظیم سایز ادیتور
        folding: true,                   // اجازه فولد بلوک‌ها (if/else) داده شود
        foldingStrategy: "indent",       // بلوک‌ها بر اساس indent فولد شوند
        fontFamily: "Fira Code, monospace", // فونت مونو با Ligature
        scrollBeyondLastLine: false,     // از آخرین خط فراتر اسکرول نشود
        renderWhitespace: "none",        // فاصله‌ها را نشان نده
        contextmenu: true,               // منوی راست کلیک فعال باشد
        readOnly: false,
        automaticClosingBrackets: "never", // چون DSL پرانتز نداره
        matchBrackets: "never",
        // Optional: highlight مخصوص کلمات DSL
        language: "bDSLScript"
    }
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