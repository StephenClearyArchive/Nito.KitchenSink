@echo off
if not exist ..\Binaries mkdir ..\Binaries
if not exist ..\Binaries\NuGet mkdir ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.PInvokeInterop\NET20\NET20.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.GuidDecoding\NET35\NET35.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.HashAlgorithms\NET35\NET35.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.GetPropertyName\NET35\NET35.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.Dynamic\NET40\NET40.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.SimpleParsers\NET40\NET40.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.OptionParsing\NET40\NET40.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.CRC\NET40\NET40.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.BinaryData\NET40\NET40.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.Exceptions\NET40\NET40.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.NotifyPropertyChanged\NET35\NET35.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack Nito.KitchenSink.FileSystemPath\Nito.KitchenSink.FileSystemPaths.nuspec -o ..\Binaries\NuGet
@echo Rename Nito.KitchenSink.FileSystemPaths.*.nupkg to Nito.KitchenSink.FileSystemPaths.*.symbols.nupkg
pause
..\Util\nuget.exe pack Nito.KitchenSink.FileSystemPath\Nito.KitchenSink.FileSystemPaths.nuspec -o ..\Binaries\NuGet -Exclude **\*.pdb -Exclude **\*.cs
pause