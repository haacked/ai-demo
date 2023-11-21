const path = require('path')
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');
const webpack = require('webpack')

module.exports = {
    target: 'web',
    entry: {
        main: './assets/index',
        react: './assets/react/index',
        assistant: './assets/react/assistant/index'
    },
    output: {
       publicPath: "/dist/js/",
       path: path.join(__dirname, '/wwwroot/dist/js'),
       filename: '[name].js'
    },
    devtool: 'source-map',
    resolve: {
        extensions: ['.js', '.jsx', '.ts', '.tsx'],
        fallback: {
            util: require.resolve("util/"),
            assert: require.resolve("assert/"),
            process: require.resolve("process/browser")
        }
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: {
                    loader: 'ts-loader',
                    options: {
                        // Disable type checker - we will use it in fork plugin in parallel without blocking the rest of the WebPack build.
                        transpileOnly: true,
                    }
                },
                exclude: /node_modules/
            },
            {
                test: /\.m?js$/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: [
                            [
                                '@babel/preset-env',
                                {
                                    "targets": {
                                        "esmodules": true
                                    }
                                },
                            ],
                        ],
                        plugins: ['@babel/plugin-proposal-nullish-coalescing-operator']
                    }
                }
            },
            {
                test: /\.node$/,
                use: 'node-loader'
            },
            {
                test: /\.css$/i,
                use: [
                    // Creates `style` nodes from JS strings
                    MiniCssExtractPlugin.loader,
                    // Translates CSS into CommonJS
                    'css-loader',
                    // Compiles Tailwind CSS via PostCSS
                    'postcss-loader'
                ]
            }
        ]
    },
    plugins: [
        new CleanWebpackPlugin({cleanOnceBeforeBuildPatterns: ["wwwroot/dist/*"]}),
        new MiniCssExtractPlugin({
          filename: "../css/[name].css"
          /* filename: "../css/[name].[chunkhash].css" */
        }),
        // fix "process is not defined" error:
        // (do "npm install process" before running the build)
        new webpack.ProvidePlugin({
            process: 'process/browser',
        })
    ]
};
