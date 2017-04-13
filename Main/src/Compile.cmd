dotnet restore CodeJam.Main.csproj

"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" CodeJam.Main.csproj /target:Clean   /property:Configuration=Debug
"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" CodeJam.Main.csproj /target:Clean   /property:Configuration=Release
"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" CodeJam.Main.csproj /target:ReBuild /property:Configuration=Debug
"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" CodeJam.Main.csproj /target:ReBuild /property:Configuration=Release

"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" CodeJam.Main.csproj /target:Pack    /property:Configuration=Release
