$include = "*-tests.xUnit.dll"

#run .net tests
$a = (gci -include $include -r | `
	where { $_.fullname -match "\\bin\\Publish\\net\d" } | `
	select -ExpandProperty FullName)
&"xunit.console.clr4" $a /appveyor
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

#run .net core tests
$a = (gci -include $include -r | `
	where { $_.fullname -match "\\bin\\Publish\\netcore" } | `
	select -ExpandProperty FullName)

$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\netcore_xunit_results.xml"
dotnet vstest $a --logger:"trx;LogFileName=$logFileName"
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
$wc = New-Object System.Net.WebClient
$wc.UploadFile("https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID", "$logFileName")
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
