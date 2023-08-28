$include = "*.Tests.dll", "*.Tests.NUnit.dll"

mkdir "$env:APPVEYOR_BUILD_FOLDER\_Results" -ErrorAction SilentlyContinue

$wc = New-Object System.Net.WebClient

$targetsDotNet = "net47","net46","net45","net40","net35","net20"
foreach ($target in $targetsDotNet) {
	#run .net tests
	$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\$($target)_nunit_results.xml"
	$a = (gci -include $include -r | `
		where { $_.fullname -match "\\bin\\Release\\$($target)" -and $_.fullname } | `
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

#run .net core tests
$targetsDotNetCore = "netcoreapp2.0","netcoreapp1.1","netcoreapp1.0"
foreach ($target in $targetsDotNetCore) {
	$a = (gci -include $include -r | `
		where { $_.fullname -match "\\bin\\Release\\$($target)" -and $_.fullname } | `
		select -ExpandProperty FullName)

	$logFileName = "$env:APPVEYOR_BUILD_FOLDER\_Results\$($target)_nunit_results.xml"
	echo "dotnet vstest $a --logger:'trx;LogFileName=$logFileName'"
	dotnet vstest $a --logger:"trx;LogFileName=$logFileName"
	if ($LastExitCode -ne 0) {
		echo "FAIL: dotnet vstest $a --logger:'trx;LogFileName=$logFileName'"
		$host.SetShouldExit($LastExitCode)
	}
	echo "UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
	$wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID", "$logFileName")
	if ($LastExitCode -ne 0) {
		echo "FAIL: UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $logFileName"
		$host.SetShouldExit($LastExitCode)
	}
}
