const path = require("path");
const TerserPlugin = require("terser-webpack-plugin");
const fileName = "dnn-extensions";

module.exports = (env) => {
    return {
        mode: "production",
        devtool: false,
        entry: path.resolve(__dirname, "./src/app.js"),
        output: {
            globalObject: "self",
            filename: fileName + ".bundle.js",
            path: path.resolve(__dirname, "dist"),
            clean: true,
        },
        optimization: {
            minimize: true,
            minimizer: [
                new TerserPlugin({
                    parallel: true,
                    terserOptions: {
                        mangle: false,
                        keep_fnames: true,
                        keep_classnames: true,
                    },
                }),
            ],
        },
        module: {
            rules: [{
                test: /\.html$/,
                exclude: [path.resolve(__dirname, "./node_modules")],
                use: [{
                    loader: "ngtemplate-loader",
                },
                {
                    loader: "html-loader",
                }],
            }],
        }
    };
};