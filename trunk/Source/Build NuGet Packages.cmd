@echo off
if not exist ..\Binaries mkdir ..\Binaries
if not exist ..\Binaries\NuGet mkdir ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.PInvokeInterop\Nito.KitchenSink.PInvokeInterop.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.FileSystemPath\Nito.KitchenSink.FileSystemPaths.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.GuidDecoding\Nito.KitchenSink.GuidDecoding.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.HashAlgorithms\Nito.KitchenSink.HashAlgorithms.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.GetPropertyName\Nito.KitchenSink.GetPropertyName.nuspec -o ..\Binaries\NuGet
pause