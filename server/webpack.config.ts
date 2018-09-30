import * as webpack from 'webpack';
import * as path from 'path';
import * as ExtractTextPlugin from 'extract-text-webpack-plugin';
import * as HtmlWebpackPlugin from 'html-webpack-plugin';
import * as UglifyJsPlugin from 'uglifyjs-webpack-plugin';
import * as fs from 'fs';

const srcPath = path.join(__dirname, '/app'),
    distPath = path.join(__dirname, '/wwwroot');

const isDevelopment = true;

// This defines all the bundles that get generated, including the .cshtml files
// that get generated so we can include assets in views.
const bundles = fs.readdirSync(path.join(__dirname, '/app/js/bundles'))
    .map((value) => value.substring(0, value.length - 3));


const config: webpack.Configuration = {
    cache: true,
    devtool: 'source-map',
    context: srcPath,
    entry: {
        'polyfills': [
            'core-js',
        ],
        'jquery': [
            // this shim exports $ and jQuery to window, because when you include
            // jQuery directly in Webpack it doesn't set the variables on
            // the window object by default.
            './js/jquery-shim',
        ],
        'bootstrap': [
            'bootstrap/dist/js/bootstrap',
            'bootstrap/dist/css/bootstrap.css',
        ],
        ...bundles.reduce((map, value) => {
                // builds a JS hashmap like {'page-login': 'js/page-login.ts', 'page-default': 'js/page-default.ts', ...}
                map[value] = [
                    './js/bundles/' + value + '.ts',
                ];
                return map;
            },
            {})
    },
    output: {
        path: distPath,
        filename: 'js/[name].[chunkhash].' + (isDevelopment ? 'dev' : 'min') + '.js',
        publicPath: '/',
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js'],
        modules: ["node_modules"],
    },
    module: {
        rules: [{
            test: /\.tsx?$/,
            use: [
                {
                    loader: 'awesome-typescript-loader',
                    options: {
                        configFile: path.join(__dirname, '/tsconfig.webpack.json'),
                        silent: true,
                    }
                }
            ]
        },
            {
                test: /\.css?$/,
                use: ExtractTextPlugin.extract({
                    loader: 'css-loader',
                    options: {minimize: !isDevelopment},
                })
            },
            {
                test: /\.(png|jpg|eot|ttf|svg|woff|woff2|gif)$/,
                use: [
                    {
                        loader: "file-loader",
                        options: {
                            outputPath: 'assets/',
                            name: '[name].[hash].[ext]',
                        }
                    }
                ]
            }
        ]
    },
    plugins: [
        new ExtractTextPlugin('css/[name].[md5:contenthash:hex:20].' + (isDevelopment ? 'dev' : 'min') + '.css'),
        ...bundles.filter(x => x.startsWith("page-")).map((value) => {
            return new HtmlWebpackPlugin({
                filename: path.join(__dirname, '/Pages/Partials/Generated/_Gen_' + value + '_Scripts.cshtml'),
                template: path.join(__dirname, '/Pages/Partials/Generated/_ScriptsTemplate.cshtml'),
                chunks: ['polyfills', 'jquery', 'bootstrap', value,],
                inject: false,
                chunksSortMode: 'manual',
            })
        }),
        ...bundles.filter(x => x.startsWith("page-")).map((value) => {
            return new HtmlWebpackPlugin({
                filename: path.join(__dirname, '/Pages/Partials/Generated/_Gen_' + value + '_Styles.cshtml'),
                template: path.join(__dirname, '/Pages/Partials/Generated/_StylesTemplate.cshtml'),
                chunks: ['polyfills', 'jquery', 'bootstrap', value, 'font-awesome'],
                inject: false,
                chunksSortMode: 'manual',
            })
        }),
        ...bundles.filter(x => !x.startsWith("page-")).map((value) => {
            return new HtmlWebpackPlugin({
                filename: path.join(__dirname, '/Pages/Partials/Generated/_Gen_' + value + '_Scripts.cshtml'),
                template: path.join(__dirname, '/Pages/Partials/Generated/_ScriptsTemplate.cshtml'),
                chunks: [value],
                inject: false,
                chunksSortMode: 'manual',
            })
        }),
        ...bundles.filter(x => !x.startsWith("page-")).map((value) => {
            return new HtmlWebpackPlugin({
                filename: path.join(__dirname, '/Pages/Partials/Generated/_Gen_' + value + '_Styles.cshtml'),
                template: path.join(__dirname, '/Pages/Partials/Generated/_StylesTemplate.cshtml'),
                chunks: [value],
                inject: false,
                chunksSortMode: 'manual',
            })
        }),
        ...(isDevelopment ? [] : [
            new UglifyJsPlugin({
                sourceMap: true,
                // Needed for TinyMCE, see https://www.tinymce.com/docs/advanced/usage-with-module-loaders/#minificationwithuglifyjs2
                uglifyOptions: {
                    output: {
                        ascii_only: true,
                    }
                }
            }),
        ])
    ]
};

export default config;