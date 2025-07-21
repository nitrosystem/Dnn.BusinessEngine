const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");
const fileName = "basic-extensions";

module.exports = (env) => {
    return {
        mode: env.production ? "production" : "development",
        devtool: env.production ? "source-map" : "eval-cheap-module-source-map",
        entry: path.resolve(__dirname, "./src/app.js"),
        output: {
            globalObject: "self",
            filename: fileName + ".bundle.js",
            path: path.resolve(__dirname, "dist"),
            clean: true,
        },
        optimization: {
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
        plugins: [new MiniCssExtractPlugin({
            filename: fileName + ".css",
        })],
        module: {
            rules: [{
                    test: /\.html$/,
                    exclude: [path.resolve(__dirname, "./node_modules")],
                    use: [{
                            loader: "ngtemplate-loader",
                        },
                        {
                            loader: "html-loader",
                        },
                    ],
                },
                {
                    test: /\.css$/,
                    use: [MiniCssExtractPlugin.loader, "css-loader"],
                },
                {
                    test: /\.(woff(2)?|ttf|eot)$/,
                    generator: {
                        filename: "./fonts/[name][ext]",
                    },
                },
            ],
        },
        externals: {
            jquery: "jQuery",
            angular: "anular",
            lodash: "_",
        },
        devServer: {
            allowedHosts: ["dnndev.me"],
            headers: {
                "Access-Control-Allow-Origin": "*",
                "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, PATCH, OPTIONS",
                "Access-Control-Allow-Headers": "X-Requested-With, content-type, Authorization",
            },
            static: {
                directory: path.join(__dirname, "dist"),
            },
        },
    };
};