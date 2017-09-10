$include = "*-tests.xUnit.dll"

$a = (gci -include $include -r | where {$_.fullname -match '\\bin\\Publish\\'} | select -ExpandProperty FullName) #-join "' '"

&"xunit.console" $a /appveyor