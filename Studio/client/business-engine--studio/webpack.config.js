const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');
const webpack = require('webpack');

module.exports = (env) => {
    return {
        mode: 'production',
        //mode: env.production ? 'production' : 'development',
        devtool: false,
        //devtool: env.production ? 'source-map' : 'eval-cheap-module-source-map',
        entry: path.resolve(__dirname, './src/index.js'),
        output: {
            globalObject: 'self',
            filename: (pathData) => {
                // Monaco workers should get separate names.
                return pathData.chunk.name === 'main'
                    ? 'studio.bundle.js'
                    : '[name].bundle.js';
            },
            chunkFilename: '[id].chunk.js',
            path: path.resolve(__dirname, 'dist'),
            clean: true,
        },
        optimization: {
            minimize: true,
            minimizer: [
                new TerserPlugin({
                    parallel: true,
                    exclude: /[\\/]node_modules[\\/]monaco-editor[\\/]/,
                    terserOptions: {
                        mangle: false,
                        keep_fnames: true,
                        keep_classnames: true,
                    },
                })
            ],
            splitChunks: {
                cacheGroups: {
                    monaco: {
                        test: /[\\/]node_modules[\\/]monaco-editor[\\/]/,
                        name: 'monaco',
                        chunks: 'all',
                        enforce: true,
                        priority: 10,
                    },
                    default: false,
                    vendors: false,
                },
            },
            runtimeChunk: false,
        },
        // optimization: {
        //     minimizer: [
        //         new TerserPlugin({
        //             parallel: true,
        //             terserOptions: {
        //                 mangle: false,
        //                 keep_fnames: true,
        //                 keep_classnames: true,
        //             },
        //         }),
        //     ],
        // },

        plugins: [
            new MiniCssExtractPlugin({
                ignoreOrder: true,
            }),
            new MonacoWebpackPlugin({
                languages: ['html', 'css', 'sql']
            }),
            new webpack.ProvidePlugin({
                $: 'jquery',
                jQuery: 'jquery',
                'window.jQuery': 'jquery',
            }),
        ],
        module: {
            rules: [
                {
                    test: /\.html$/,
                    exclude: [path.resolve(__dirname, './node_modules')],
                    use: [
                        {
                            loader: 'ngtemplate-loader',
                        },
                        {
                            loader: 'html-loader',
                        },
                    ],
                },
                {
                    test: /\.css$/,
                    use: [MiniCssExtractPlugin.loader, 'css-loader'],
                },
                {
                    test: /\.(woff(2)?|eot)$/,
                    generator: {
                        filename: './fonts/[name][ext]',
                    },
                },
                {
                    test: /\.ttf$/,
                    include: /monaco-editor/,
                    use: 'null-loader',
                },
            ],
        },
        devServer: {
            hot: false,
            allowedHosts: ['localhost:2649'],
            headers: {
                'Access-Control-Allow-Origin': '*',
                'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, PATCH, OPTIONS',
                'Access-Control-Allow-Headers': 'X-Requested-With, content-type, Authorization',
            },
            static: {
                directory: path.join(__dirname, 'dist'),
            },
        }
    };
};