$include = "*-tests.MSTest.dll"

$a = (gci -include $include -r | where {$_.fullname -match '\\bin\\Publish\\'} | select -ExpandProperty FullName) #-join "' '"

&"vstest.console" /logger:Appveyor $a /Platform:x86
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode)  }