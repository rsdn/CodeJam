<#
  Feel my pain. Here's incomplete list of issues that prevents us just to use existing tooling:
  * https://github.com/appveyor/ci/issues/1894 - root cause. Test results are overwritten
  * https://github.com/nunit/nunit/issues/3790 - bonus issue: no support for legacy frameforks on dotnet test
  * https://github.com/microsoft/vstest/issues/4355 - dotnet vstest does not emits TFM for the trx file
  * https://github.com/microsoft/vstest/issues/880 - dotnet test merges test result for multitargeting test
  * https://github.com/nunit/nunit/pull/2724 - PR for appveyor format support (abandoned)
  *
#>


# Run .Net Framework tests using nunit3-console
## We use nunit-console here as as dotnet test does not support legacy net frameworks
$testNetFwDlls = ls -r '.artifacts\*\Debug\*.Tests.dll' | ? FullName -match '\\net\d+\\' ` | % FullName
$logFilePath = '.artifacts\nunit_netframework.xml'
nunit3-console $testNetFwDlls --result=$logFilePath

## Update test report: add TFM to the assemply name
$matchPattern = 'name="(?''name''.*?\.dll)" fullname="(?''fullname''.*?\\(?''tfm''net[^\\]*)\\[^\\]*?\.dll)"'
$replacement = 'name="${name} (${tfm})" fullname="${fullname} (${tfm})"'
(cat $logFilePath) -Replace $matchPattern, $replacement | Out-File -Encoding UTF8 $logFilePath


# Run .Net Core tests
## Emits multiple test reports
dotnet test .\CodeJam.Light.sln -p:NetCoreTests=true --no-build --no-restore `
  --results-directory '.\.artifacts\' `
  --logger 'trx;LogFilePrefix=nunit'

## Update test reports: add TFM to the assemply name
$matchPattern1 = 'storage="(?''storage''.*?\\(?''tfm''net[^\\]*)\\[^\\]*?\.dll)"'
$replacement1 = 'storage="${storage} (${tfm})"'
$matchPattern2 = 'codeBase="(?''codeBase''.*?\\(?''tfm''net[^\\]*)\\[^\\]*?\.dll)"'
$replacement2 = 'codeBase="${codeBase} (${tfm})"'
$netCoreReports = ls '.artifacts\nunit_*.trx' | % FullName
$netCoreReports | ForEach-Object {
  (cat $_) -Replace $matchPattern1, $replacement1  -Replace $matchPattern2, $replacement2 | Out-File -Encoding UTF8 $_
}


# Upload files
$wc = New-Object 'System.Net.WebClient'
$testResults = @(ls '.artifacts\nunit_*.xml' | % FullName)
$testResults | ForEach-Object {
  echo "UploadFile: https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID from $_"
  $wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$env:APPVEYOR_JOB_ID", $_)
}
$testResults = ls '.artifacts\nunit_*.trx' | % FullName
$testResults | ForEach-Object {
  echo "UploadFile: https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID from $_"
  $wc.UploadFile("https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID", $_)
}