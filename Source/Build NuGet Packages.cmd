@echo off
if not exist ..\Binaries mkdir ..\Binaries
if not exist ..\Binaries\NuGet mkdir ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.PInvokeInterop\NET20\NET20.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack Nito.KitchenSink.FileSystemPath\Nito.KitchenSink.FileSystemPaths.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe pack Nito.KitchenSink.GuidDecoding\Nito.KitchenSink.GuidDecoding.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe pack Nito.KitchenSink.HashAlgorithms\Nito.KitchenSink.HashAlgorithms.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe pack Nito.KitchenSink.GetPropertyName\Nito.KitchenSink.GetPropertyName.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe pack Nito.KitchenSink.Dynamic\Nito.KitchenSink.Dynamic.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe pack Nito.KitchenSink.SimpleParsers\Nito.KitchenSink.SimpleParsers.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe pack -sym Nito.KitchenSink.OptionParsing\NET40\NET40.csproj -o ..\Binaries\NuGet
..\Util\nuget.exe pack Nito.KitchenSink.CRC\Nito.KitchenSink.CRC.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe pack Nito.KitchenSink.BinaryData\Nito.KitchenSink.BinaryData.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe pack Nito.KitchenSink.Exceptions\Nito.KitchenSink.Exceptions.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe pack Nito.KitchenSink.NotifyPropertyChanged\Nito.KitchenSink.NotifyPropertyChanged.nuspec -o ..\Binaries\NuGet
pause