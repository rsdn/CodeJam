$include = "*-tests.dll", "*-tests.performance.dll", "*-tests.NUnit.dll"
$exclude = 'Experimental\\.*?\\CodeJam-Tests.Performance.dll'

$a = (gci -include $include -r | where {$_.fullname -match '\\bin\\Publish\\' -and $_.fullname -notmatch $exclude} | select -ExpandProperty FullName) #-join "' '"

&"nunit3-console" $a --result=myresults.xml;format=AppVeyor