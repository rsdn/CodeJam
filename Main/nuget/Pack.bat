cd ..\src
call Compile.cmd

cd ..\nuGet

del *.nupkg

NuGet Pack CodeJam.Main.nuspec
