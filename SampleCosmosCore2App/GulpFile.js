/// <binding BeforeBuild='default' ProjectOpened='watch' />

var gulp = require('gulp');
var rename = require('gulp-rename');
var sass = require('gulp-sass');
var sourcemaps = require('gulp-sourcemaps');
var postcss = require('gulp-postcss');
var autoprefixer = require('autoprefixer');
var cssnano = require('cssnano');
var rollup = require('gulp-better-rollup');
var babel = require('rollup-plugin-babel');
var commonjs = require('rollup-plugin-commonjs');

// ---------- Configs --------------------------------------

var sassInput = "Content/styles/main.scss";
var sassWatch = "Content/styles/**/*.scss";
var sassOutput = "Content/styles/";
var autoprefixerOptions = {
    browsers: ['last 2 versions', '> 2%']
};
var jsSrc = "Content/scripts/src/app.js";
var jsDest = "Content/scripts/";
var jsWatch = "Content/scripts/**/*.js";

// ---------------------------------------------------------
 
gulp.task('sass', function () {
    var plugins = [
        autoprefixer(),
        cssnano()
    ];

    return gulp.src(sassInput)
        .pipe(sourcemaps.init())
        .pipe(sass().on('error', sass.logError))
        .pipe(postcss(plugins))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(sassOutput));
});
 
gulp.task('sass:watch', function () {
    gulp.watch(sassWatch, ['sass'])
        .on('change', function (event) {
            console.log('[SCSS WATCH] ' + shortenPath(event.path) + ' was ' + event.type);
        });
});

function shortenPath(longPath)
{
    return longPath.substr(__dirname.length);
}

gulp.task('js', function () {
    return gulp.src(jsSrc)
        .pipe(sourcemaps.init())
        .pipe(rollup({
            external: [
                'jquery',
                'vue'
            ],
            plugins: [
                babel()
            ]
        }, {
            format: 'umd'
        }))
        .pipe(rename(function (path) {
            path.basename += "-bundle";
        }))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(jsDest));
        
});

gulp.task('js:watch', function () {
    gulp.watch(jsWatch, ['js'])
        .on('change', function (event) {
            console.log('[JS WATCH] ' + shortenPath(event.path) + ' was ' + event.type);
        });
});

gulp.task('default', ['sass', 'js']);
gulp.task('watch', ['sass:watch', 'js:watch']);