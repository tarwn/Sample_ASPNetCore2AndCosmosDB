/// <binding BeforeBuild='default' ProjectOpened='watch' />

var gulp = require('gulp');
var sass = require('gulp-sass');
var sourcemaps = require('gulp-sourcemaps');
var postcss = require('gulp-postcss');
var autoprefixer = require('autoprefixer');
var cssnano = require('cssnano');

// ---------- Configs --------------------------------------

var sassInput = "Content/styles/main.scss";
var sassWatch = "Content/styles/**/*.scss";
var sassOutput = "Content/styles/";
var autoprefixerOptions = {
    browsers: ['last 2 versions', '> 2%']
};

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

gulp.task('default', ['sass']);
gulp.task('watch', ['sass:watch'])