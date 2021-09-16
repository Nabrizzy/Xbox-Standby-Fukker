@echo off

set directory=%cd%
set command=New-Service -Name 'Xbox Standby Fukker' -BinaryPathName '%directory%\XboxStandbyFukker.exe'

call powershell.exe -command %command%