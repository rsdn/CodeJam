$include = "*-tests.MSTest.dll"

$a = (gci -include $include -r | where {$_.fullname -match '\\bin\\Publish\\'} | select -ExpandProperty FullName) #-join "' '"

&"vstest.console" /logger:Appveyor $a /Platform:x86