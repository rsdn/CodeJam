SET MSBUILD="%ProgramFiles(x86)%\MSBuild\15.0\Bin\MSBuild.exe"

cd ..\src
%MSBUILD% CodeJam.PerfTests.csproj /target:Clean /property:Configuration=Release
%MSBUILD% CodeJam.PerfTests.csproj /target:Rebuild /property:Configuration=Release
cd ..\nuget

cd ..\src-NUnit
%MSBUILD% CodeJam.PerfTests.NUnit.csproj /target:Clean /property:Configuration=Release
%MSBUILD% CodeJam.PerfTests.NUnit.csproj /target:Rebuild /property:Configuration=Release
cd ..\nuget

cd ..\src-XUnit
%MSBUILD% CodeJam.PerfTests.XUnit.csproj /target:Clean /property:Configuration=Release
%MSBUILD% CodeJam.PerfTests.XUnit.csproj /target:Rebuild /property:Configuration=Release
cd ..\nuget

cd ..\src-MSTest
%MSBUILD% CodeJam.PerfTests.MSTest.csproj /target:Clean /property:Configuration=Release
%MSBUILD% CodeJam.PerfTests.MSTest.csproj /target:Rebuild /property:Configuration=Release
cd ..\nuget

del *.nupkg

NuGet Pack CodeJam.PerfTests.Core.nuspec
NuGet Pack CodeJam.PerfTests.NUnit.nuspec
NuGet Pack CodeJam.PerfTests.MSTest.nuspec
NuGet Pack CodeJam.PerfTests.xUnit.nuspec