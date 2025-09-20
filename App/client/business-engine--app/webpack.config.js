const path = require("path");
const TerserPlugin = require("terser-webpack-plugin");

const commonRules = [
];

const commonOptimization = {
    minimizer: [
        new TerserPlugin({
            parallel: true,
            terserOptions: { mangle: false, keep_fnames: true, keep_classnames: true },
        }),
    ],
};

const devServer = {
    allowedHosts: ["localhost:8585"],
    headers: {
        "Access-Control-Allow-Origin": "*",
        "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, PATCH, OPTIONS",
        "Access-Control-Allow-Headers": "X-Requested-With, content-type, Authorization"
    },
    static: {
        directory: path.join(__dirname, "dist")
    }
};

// 🔹 UMD خروجی برای Razor قدیمی
const umdConfig = {
    mode: "production",
    entry: path.resolve(__dirname, "./src/startup.js"),
    output: {
        filename: "business-engine.umd.js",
        path: path.resolve(__dirname, "dist"),
        clean: true,
        library: "BusinessEngineApp",  // نام global برای Razor
        libraryTarget: "umd",          // UMD خروجی
        globalObject: "this",
    },
    module: { rules: commonRules },
    externals: {}, // هیچ وابستگی خارجی نداریم
};

// 🔹 ESM خروجی برای Razor مدرن / import
const esmConfig = {
    mode: "production",
    entry: path.resolve(__dirname, "./src/startup.js"),
    output: {
        filename: "business-engine.esm.js",
        path: path.resolve(__dirname, "dist"),
        module: true,
        library: { type: "module" },
    },
    experiments: { outputModule: true },
    optimization: commonOptimization,
    module: { rules: commonRules },
    devServer: devServer,
    externals: {}, // هیچ وابستگی خارجی نداریم
};

module.exports = [esmConfig];
