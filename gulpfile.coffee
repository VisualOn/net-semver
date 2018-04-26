gulp = require 'gulp'
args = require('yargs').argv

msbuild = require 'gulp-msbuild'
nunit = require 'gulp-nunit-runner'
shell = require 'gulp-shell'
replace = require 'gulp-replace'
git = require 'gulp-git'
semver = require 'semver'
fs = require 'fs'

nunitPath = './packages/NUnit.Runners.2.6.3/tools/nunit-console.exe'
assemblyInfoPath = './SemanticVersioning/Properties/AssemblyInfo.cs'
assemblyInfoDir = './SemanticVersioning/Properties'
nuspecPath = './SemanticVersioning/SemanticVersioning.csproj'
configuration = if args.debug then 'Debug' else 'Release'

newTag = undefined

gulp.task 'default', ['build', 'test']

gulp.task 'restore', ->
  gulp.src '**/*.sln'
    .pipe msbuild
      toolsVersion: 15.0
      targets: ['Restore']
      logCommand: true
      errorOnFail: true
      stdout: true
      verbosity: 'minimal'
      configuration: configuration

gulp.task 'clean', ->
  gulp.src '**/*.sln'
    .pipe msbuild
      toolsVersion: 15.0
      targets: ['Clean']
      logCommand: true
      errorOnFail: true
      stdout: true
      verbosity: 'minimal'
      configuration: configuration

gulp.task 'build', ['restore'], ->
  gulp.src '**/*.sln'
    .pipe msbuild
      toolsVersion: 15.0
      targets: ['Clean', 'Build']
      logCommand: true
      errorOnFail: true
      stdout: true
      verbosity: 'minimal'
      configuration: configuration

gulp.task 'test', ->
  gulp.src ['**/bin/**/*.Tests.dll'], read: false
    .pipe nunit
      executable: nunitPath

gulp.task 'bump', ['bump-commit'], ->
  git.tag newTag, "Release #{newTag}", (err) ->
    if err then throw err

gulp.task 'pack', ['build'], ->
  gulp.src '**/*.sln'
    .pipe msbuild
      toolsVersion: 15.0
      targets: ['Pack']
      logCommand: true
      errorOnFail: true
      stdout: true
      verbosity: 'minimal'
      configuration: configuration

gulp.task 'push', ->
  version = getCurrentVersion()
  gulp.src "./SemanticVersioning.#{version}.nupkg"
    .pipe shell ["nuget push"]

gulp.task 'bump-commit', ['bump-update'], ->
  gulp.src assemblyInfoPath
    .pipe git.commit("Release #{newTag}")

gulp.task 'bump-update', ->
  inc = args.i || args.inc || args.increment
  pre = args.pre || 'beta'
  stable = args.s || args.stable

  currentVersion = getCurrentVersion()

  isPrerelease = currentVersion.indexOf('-') > 0
  bumped = currentVersion

  if isPrerelease && stable
    bumped = bumped.substr 0, bumped.indexOf '-'
    if inc
      bumped = semver.inc bumped, inc

  else if isPrerelease && inc
    pre = /.+-(\w+)(\.|\+)?/.exec(bumped)[1]
    bumped = semver.inc bumped, inc
    bumped = bumped + '-' + pre

  else if isPrerelease
    bumped = semver.inc bumped, 'pre'

  else if inc && stable
    bumped = semver.inc bumped, inc

  else
    inc = inc || 'patch'
    bumped = semver.inc bumped, inc
    bumped = bumped + '-' + pre

  short = bumped.split('-')[0].split('+')[0]
  newTag = "v#{bumped}"

  gulp.src assemblyInfoPath
    .pipe replace /(AssemblyVersion\(")(.+?)"\)/, "$1#{short}\")"
    .pipe replace /(AssemblyFileVersion\(")(.+?)"\)/, "$1#{short}\")"
    .pipe replace /(AssemblyInformationalVersion\(")(.+?)"\)/, "$1#{bumped}\")"
    .pipe gulp.dest(assemblyInfoDir)

getCurrentVersion = ->
  assemblyInfo = fs.readFileSync assemblyInfoPath, 'utf8'
  /AssemblyInformationalVersion\("(.+)"\)/.exec(assemblyInfo)[1]
