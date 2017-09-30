$include = "*-tests.dll", "*-tests.performance.dll", "*-tests.NUnit.dll"
$exclude = "Experimental\\.*?\\CodeJam-Tests.Performance.dll"

#run .net tests
$a = (gci -include $include -r | `
	where { $_.fullname -match "\\bin\\Publish\\net\d" -and $_.fullname -notmatch $exclude } | `
	select -ExpandProperty FullName)
&"nunit3-console" $a "--result=myresults.xml;format=AppVeyor"
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

#run .net core tests
$a = (gci -include $include -r | `
	where { $_.fullname -match "\\bin\\Publish\\netcore" -and $_.fullname -notmatch $exclude } | `
	select -ExpandProperty FullName)

$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\netcore_nunit_results.xml"
dotnet vstest $a --logger:"trx;LogFileName=$logFileName"
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
$wc = New-Object System.Net.WebClient
$wc.UploadFile("https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID", "$logFileName")
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
