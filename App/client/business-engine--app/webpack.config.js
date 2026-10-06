const path = require('path');
const TerserPlugin = require('terser-webpack-plugin');
const config = {
    mode: 'production',
    devtool: false,
    //devtool: 'source-map',
    performance: {
        hints: false,
    },
    entry: path.resolve(__dirname, './src/startup.js'),
    output: {
        filename: 'business-engine.esm.js',
        path: path.resolve(__dirname, 'dist'),
        module: true,
        library: { type: 'module' },
        clean: true,
    },
    experiments: { outputModule: true },
    optimization: {
        minimize: true,
        usedExports: true,
        sideEffects: false,
        concatenateModules: true,
        minimizer: [
            new TerserPlugin({
                parallel: true,
                extractComments: false,
                terserOptions: {
                    ecma: 2020,
                    compress: {
                        drop_console: true,
                        drop_debugger: true,
                        passes: 2,
                    },
                    mangle: false,
                    format: {
                        comments: false,
                    },
                },
            }),
        ],
    },
    module: { rules: [] },
    externals: {},
};
module.exports = [config];
