$include = "*.Tests.dll", "*.Tests.NUnit.dll"
$includePerfTests = "*.Tests.Performance.dll"
$exclude = "Experimental\\.*?\\CodeJam.Tests.Performance.dll"

mkdir "$env:APPVEYOR_BUILD_FOLDER\_Results" -ErrorAction SilentlyContinue

$wc = New-Object System.Net.WebClient

$targetsDotNet = "net472","net461","net45","net40","net35","net20"
foreach ($target in $targetsDotNet) {
	#run .net tests
	$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\$($target)_nunit_results.xml"
	$a = (gci -include $include -r | `
		where { $_.fullname -match "\\bin\\Release\\$($target)" -and $_.fullname -notmatch $exclude } | `
		select -ExpandProperty FullName)
	$framework = $target.Substring(0, $target.Length - 2) + "-" + $target.Substring($target.Length - 2, 1) + "." + $target.Substring($target.Length - 1)
	echo "nunit3-console $a --result=$logFileName"
	&"nunit3-console" $a "--result=$logFileName"
	if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
	echo "UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
	$wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID", "$logFileName")
	if ($LastExitCode -ne 0) {
		echo "FAIL: UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
		$host.SetShouldExit($LastExitCode)
	}
}

#run .net perftests
#$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\net_perftest_nunit_results.xml"
#$a = (gci -include $includePerfTests -r | `
#	where { $_.fullname -match "\\bin\\Release\\net\d" -and $_.fullname -notmatch $exclude } | `
#	select -ExpandProperty FullName)
#echo "nunit3-console $a --result=$logFileName" --agents=1
#&"nunit3-console" $a "--result=$logFileName" --agents=1
#if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
#echo "UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
#$wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID", "$logFileName")
#if ($LastExitCode -ne 0) {
#	echo "FAIL: UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
#	$host.SetShouldExit($LastExitCode)
#}

#run .net core tests
$targetsDotNetCore = "netcoreapp2.0"
$a = (gci -include $include -r | `
	where { $_.fullname -match "\\bin\\Release\\netcore" -and $_.fullname -notmatch $exclude } | `
	select -ExpandProperty FullName)

$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\netcore_nunit_results.xml"
echo "dotnet vstest $a --logger:'trx;LogFileName=$logFileName'"
dotnet vstest $a --logger:"trx;LogFileName=$logFileName"
if ($LastExitCode -ne 0) {
	echo "FAIL: dotnet vstest $a --logger:'trx;LogFileName=$logFileName'"
	$host.SetShouldExit($LastExitCode)
}
echo "UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
$wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID", "$logFileName")
if ($LastExitCode -ne 0) {
	echo "FAIL: UploadFile: https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID from $logFileName"
	$host.SetShouldExit($LastExitCode)
}
