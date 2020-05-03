$include = "*.Tests.dll", "*.Tests.NUnit.dll"

mkdir "$env:APPVEYOR_BUILD_FOLDER\_Results" -ErrorAction SilentlyContinue

$wc = New-Object System.Net.WebClient

#run .net tests
$targetsDotNet = "net48;net472;net471;net47;net461;net45;net40;net35;net20" -split ";"
foreach ($target in $targetsDotNet) {
	$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\$($target)_nunit_results.xml"
	$testAssemblies = (gci -include $include -r | `
		where { $_.fullname -match "\\bin\\Release\\$($target)" } | `
		select -ExpandProperty FullName)

	echo ""
	echo "=== $target ==="
	echo "nunit3-console $testAssemblies --result=$logFileName"
	&"nunit3-console" $testAssemblies "--result=$logFileName"
	if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

	echo "UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
	$wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID", "$logFileName")
	if ($LastExitCode -ne 0) {
		echo "FAIL: UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
		$host.SetShouldExit($LastExitCode)
	}
}

#run .net core tests
$targetsDotNetCore = "netcoreapp3.0;netcoreapp2.1;netcoreapp1.1" -split ";"
foreach ($target in $targetsDotNetCore) {
	$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\$($target)_nunit_results.xml"
	$testAssemblies = (gci -include $include -r | `
		where { $_.fullname -match "\\bin\\Release\\$($target)" } | `
		select -ExpandProperty FullName)

	echo ""
	echo "=== $target ==="
	echo "dotnet vstest $testAssemblies --logger:'trx;LogFileName=$logFileName'"
	dotnet vstest $testAssemblies --logger:"trx;LogFileName=$logFileName"
	if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

	echo "UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
	$wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID", "$logFileName")
	if ($LastExitCode -ne 0) {
		echo "FAIL: UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
		$host.SetShouldExit($LastExitCode)
	}
}
