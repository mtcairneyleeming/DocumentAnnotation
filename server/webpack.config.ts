import * as webpack from 'webpack';
import * as path from 'path';
import * as HtmlWebpackPlugin from 'html-webpack-plugin';
import * as fs from 'fs';
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

const srcPath = path.join(__dirname, '/app'),
    distPath = path.join(__dirname, '/wwwroot');

// This defines all the bundles that get generated, including the .cshtml files
// that get generated so we can include assets in views.
const bundles = fs.readdirSync(path.join(__dirname, '/app/js/bundles'))
    .map((value) => value.substring(0, value.length - 3));


// `--mode development` is passed by `npm run build` (watch) and by `dotnet build` in Debug; everything else
// (`npm run build-once`, `dotnet publish -c Release`) builds in production mode (minified, no dev helpers).
export default (env: unknown, argv: { mode?: 'development' | 'production' | 'none' }): webpack.Configuration => {
const mode = argv.mode === 'development' ? 'development' : 'production';

return {
    cache: true,
    // external .map files in both modes, so production stack traces stay readable in dev tools
    devtool: 'source-map',
    context: srcPath,
    mode: mode,
    // the bundles include Konva and Bootstrap; the size-limit warning is just noise here
    performance: { hints: false },
    entry: {
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
        // same file name in both modes, so switching modes never leaves stale bundles in wwwroot (which publish would ship)
        filename: 'js/[name].min.js',
        publicPath: '/',
    },
    // Plugins (jquery-validation and so on) must attach to the jQuery that the `jquery` entry put on window, not to a
    // second copy bundled into their own entry. Only the shim that creates that global bundles jQuery itself.
    externals: ({ request, contextInfo }, callback) => {
        if (request === 'jquery' && !/jquery-shim/.test(contextInfo.issuer || '')) {
            return callback(undefined, 'root jQuery');
        }
        callback();
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js'],
        modules: ["node_modules"],
    },
    module: {
        rules: [{
            test: /\.tsx?$/,
            use: 'ts-loader',
        },
        {
            test: /\.css?$/,
            use: [MiniCssExtractPlugin.loader, "css-loader"],
        },
        {
            test: /\.(png|jpg|eot|ttf|svg|woff|woff2|gif)$/,
            type: 'asset/resource',
            generator: {
                filename: 'assets/[name].[hash].[ext]'
            },
        }
        ]
    },
    plugins: [

        new MiniCssExtractPlugin(),
        ...bundles.map((value) => {
            return new HtmlWebpackPlugin({
                filename: path.join(__dirname, '/Pages/Partials/Generated/_Gen_' + value + '_Scripts.cshtml'),
                template: path.join(__dirname, '/Pages/Partials/_ScriptsTemplate.cshtml'),
                chunks: ['jquery', 'bootstrap', value],
                inject: false,
                chunksSortMode: 'manual',
            })
        }),
        ...bundles.map((value) => {
            return new HtmlWebpackPlugin({
                filename: path.join(__dirname, '/Pages/Partials/Generated/_Gen_' + value + '_Styles.cshtml'),
                template: path.join(__dirname, '/Pages/Partials/_StylesTemplate.cshtml'),
                chunks: ['jquery', 'bootstrap', value, 'font-awesome'],
                inject: false,
                chunksSortMode: 'manual',
            })
        })
       
    ]
};
};
