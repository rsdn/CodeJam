$include = "*.Tests.MSTest.dll"

#run .net tests
$a = (gci -include $include -r | `
	where { $_.fullname -match "\\bin\\Release\\net\d" } | `
	select -ExpandProperty FullName)
echo "vstest.console /logger:Appveyor $a /Platform:x86"
&"vstest.console" /logger:Appveyor $a /Platform:x86
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

#run .net core tests
# waiting for https://github.com/Microsoft/vstest/issues/1128
#$a = (gci -include $include -r | `
#	where { $_.fullname -match "\\bin\\Release\\netcore" } | `
#	select -ExpandProperty FullName)
#
#$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\netcore_mstest_results.xml"
#echo "dotnet vstest $a --logger:'trx;LogFileName=$logFileName' --platform:x86"
#dotnet vstest $a --logger:"trx;LogFileName=$logFileName" --platform:x86
#if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
#$wc = New-Object System.Net.WebClient
#$wc.UploadFile("https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID", "$logFileName")
#if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
