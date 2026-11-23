// `import = require` keeps this on jQuery's CommonJS/UMD build, where the export is the callable jQuery function.
import $ = require('jquery');
(window as any).jQuery = (window as any).$ = $;