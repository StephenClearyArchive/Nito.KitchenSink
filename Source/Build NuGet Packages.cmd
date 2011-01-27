@echo off
if not exist ..\Binaries mkdir ..\Binaries
if not exist ..\Binaries\NuGet mkdir ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.PInvokeInterop\Nito.KitchenSink.PInvokeInterop.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.FileSystemPath\Nito.KitchenSink.FileSystemPaths.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.GuidDecoding\Nito.KitchenSink.GuidDecoding.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.HashAlgorithms\Nito.KitchenSink.HashAlgorithms.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.GetPropertyName\Nito.KitchenSink.GetPropertyName.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.Dynamic\Nito.KitchenSink.Dynamic.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.SimpleParsers\Nito.KitchenSink.SimpleParsers.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.OptionParsing\Nito.KitchenSink.OptionParsing.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.CRC\Nito.KitchenSink.CRC.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.BinaryData\Nito.KitchenSink.BinaryData.nuspec -o ..\Binaries\NuGet
..\Util\nuget.exe p Nito.KitchenSink.Exceptions\Nito.KitchenSink.Exceptions.nuspec -o ..\Binaries\NuGet
pause