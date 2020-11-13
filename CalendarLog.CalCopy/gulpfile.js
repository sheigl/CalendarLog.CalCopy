const autoprefixer = require("autoprefixer");
const gulp = require("gulp");
const plumber = require("gulp-plumber");
const postcss = require("gulp-postcss");
const rename = require("gulp-rename");
const sass = require("gulp-sass");
const cssnano = require("cssnano");
const concatCss = require('gulp-concat-css');
const csso = require('gulp-csso');
const replace = require('gulp-replace');

const config = {}
config.root = './wwwroot'
config.css = config.root + '/css';
config.assets = './assets';
config.cssAssets = config.assets + '/css';
config.scss = config.assets + '/scss';
config.nodeModules = './node_modules';
config.bootstrapCss = config.nodeModules + '/bootstrap/dist/css/bootstrap.min.css';
config.bootstrapJs = config.nodeModules + '/bootstrap/dist/js/bootstrap.bundle.min.js';
config.fontawesomeCss = config.nodeModules + '/@fortawesome/fontawesome-free/css/all.min.css';
config.fontawesomeWebfonts = config.nodeModules + '/@fortawesome/fontawesome-free/webfonts';
config.animateCss = config.nodeModules + '/animate.css/animate.min.css';

function scss() {
    return gulp
        .src(config.scss + "/**/*.scss")
        .pipe(plumber())
        .pipe(sass({ outputStyle: "expanded" }))
        .pipe(gulp.dest(config.scss))
        .pipe(rename({ suffix: ".min" }))
        .pipe(postcss([autoprefixer(), cssnano()]))
        .pipe(gulp.dest(config.scss));
}

function css() {
    return gulp
        .src([config.bootstrapCss, config.fontawesomeCss, config.animateCss, config.scss + '/site.css'])
        .pipe(plumber())
        .pipe(concatCss(config.cssAssets + '/site.css'))
        .pipe(replace('../', ''))
        .pipe(replace('@fortawesome/fontawesome-free/', ''))
        .pipe(gulp.dest('.'));
}

function minCss() {
    return gulp
        .src(config.cssAssets + '/**/*.css')
        .pipe(plumber())
        //.pipe(csso())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(config.css));
}

function copy() {
    return gulp
        .src(config.fontawesomeWebfonts + '/**/*')
        .pipe(plumber())
        .pipe(gulp.dest(config.css + '/webfonts'));
}

const defaultAction = gulp.series(scss, css, minCss, copy);

exports.default = defaultAction;
exports.scss = scss;
exports.css = css;
exports.minCss = minCss;
exports.copyFonts = copy;
