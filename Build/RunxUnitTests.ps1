$include = "*-tests.xUnit.dll"

$a = (gci -include $include -r | where {$_.fullname -match '\\bin\\Publish\\'} | select -ExpandProperty FullName) #-join "' '"

&"xunit.console" $a /appveyor
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode)  }