# Nito.KitchenSink

"Everything except... well, you know..."

A miscellaneous collection of utility classes, many of which provide extension methods for existing framework types.

## NuGet Releases

The more stable parts of the KitchenSink library have been split into separate NuGet packages, which allow for more code reuse without including the entire KitchenSink library (it's past the 1 MB mark and heading quickly towards 2 MB).

Each NuGet package is fully documented (xmldoc) and instrumented with Code Contracts before release. In addition, the designs of the classes going into NuGet packages are reviewed before they enter that process.

As a result, the NuGet packages have more stability than the general KitchenSink library. Each NuGet package has its own independent [semantic version](http://semver.org/) number.

## CodePlex Releases

Due to the nature of extension methods, almost every release of this library is a potentially breaking change. Because of this, Nito.KitchenSink does not use a "major.minor" version numbering scheme; only a "major" version is used.
