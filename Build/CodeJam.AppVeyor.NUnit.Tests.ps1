$include = "*-tests.dll", "*-tests.NUnit.dll"
$includePerfTests = "*-tests.performance.dll"
$exclude = "Experimental\\.*?\\CodeJam-Tests.Performance.dll"

$wc = New-Object System.Net.WebClient

#run .net tests
$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\net_nunit_results.xml"
$a = (gci -include $include -r | `
	where { $_.fullname -match "\\bin\\Publish\\net\d" -and $_.fullname -notmatch $exclude } | `
	select -ExpandProperty FullName)
echo "nunit3-console $a --result=$logFileName"
&"nunit3-console" $a "--result=$logFileName"
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
$wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID", "$logFileName")
if ($LastExitCode -ne 0) { 
	echo "FAIL: UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
	$host.SetShouldExit($LastExitCode)
}

#run .net perftests
$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\net_perftest_nunit_results.xml"
$a = (gci -include $includePerfTests -r | `
	where { $_.fullname -match "\\bin\\Publish\\net\d" -and $_.fullname -notmatch $exclude } | `
	select -ExpandProperty FullName)
echo "nunit3-console $a --result=$logFileName" --agents=1
&"nunit3-console" $a "--result=$logFileName" --agents=1
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
$wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID", "$logFileName")
if ($LastExitCode -ne 0) { 
	echo "FAIL: UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
	$host.SetShouldExit($LastExitCode)
}

#run .net core tests
$a = (gci -include $include -r | `
	where { $_.fullname -match "\\bin\\Publish\\netcore" -and $_.fullname -notmatch $exclude } | `
	select -ExpandProperty FullName)

$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\netcore_nunit_results.xml"
echo "dotnet vstest $a --logger:'trx;LogFileName=$logFileName'"
dotnet vstest $a --logger:"trx;LogFileName=$logFileName"
if ($LastExitCode -ne 0) {
	echo "FAIL: dotnet vstest $a --logger:'trx;LogFileName=$logFileName'"
	$host.SetShouldExit($LastExitCode)
}
$wc.UploadFile("https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID", "$logFileName")
if ($LastExitCode -ne 0) { 
	echo "FAIL: UploadFile: https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID from $logFileName"
	$host.SetShouldExit($LastExitCode)
}
