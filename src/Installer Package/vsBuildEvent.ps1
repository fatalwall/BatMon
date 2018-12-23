param (
    [string]$SolutionDir,
    [string]$ConfigurationName
)
$working = $SolutionDir + 'Installer Package'
$source = $SolutionDir + 'Installer Package\BatMon Installer.nsi'
$workingDir = $SolutionDir + 'Installer Package\bin\' + $ConfigurationName + '\'
$workingFile = $workingDir + 'BatMon Installer.nsi'

set-location "$working"

<#
#	Copy NSIS Script into directory matching VS Build option
#>
copy "$source"  -Destination (New-Item "$workingDir" -Type container -force) -Container -force
#strExplode.nsh
copy "..\..\..\NSIS_strExplode\strExplode.nsh"  -Destination (New-Item "$workingDir" -Type container -force) -Container -force
#DotNetVersion.nsh
copy "..\..\..\NSIS_DotNetVersion\DotNetVersion.nsh"  -Destination (New-Item "$workingDir" -Type container -force) -Container -force
#Licence File
copy "..\..\LICENSE"  -Destination (New-Item "$workingDir" -Type container -force) -Container -force


<#
#	Insert Assembly Varables
#	BatMan.exe		Windows Service
#>
# BatMan.exe Version
$args = $SolutionDir + 'BatMon\bin\'+ $ConfigurationName + '\BatMon.exe' + ' Version';
$output = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{BatMon.AssemblyVersion}', "$output" | Out-File "$workingFile"

# BatMan.exe CompanyName
$args = $SolutionDir + 'BatMon\bin\'+ $ConfigurationName + '\BatMon.exe' + ' CompanyName';
$output = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{BatMon.AssemblyCompany}', "$output" | Out-File "$workingFile"

# BatMan.exe ProductName
$args = $SolutionDir + 'BatMon\bin\'+ $ConfigurationName + '\BatMon.exe' + ' ProductName';
$output = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
if ($ConfigurationName -eq 'Release') {
	(gc "$workingFile") -replace '&{BatMon.AssemblyTitle}', "$output" | Out-File "$workingFile"
}else{
	(gc "$workingFile") -replace '&{BatMon.AssemblyTitle}', "$output (Pre Release)" | Out-File "$workingFile"
}

<#
#	Insert Assembly Varables
#	Plugins
#>
# BatMon.AllPlugins
$args = $SolutionDir + 'BatMon.AllPlugins\bin\'+ $ConfigurationName + '\BatMon.AllPlugins.dll' + ' Version';
$output = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{BatMon.AllPlugins.AssemblyVersion}', "$output" | Out-File "$workingFile"

# BatMon.ScheduledTasks
$args = $SolutionDir + 'BatMon.ScheduledTasks\bin\'+ $ConfigurationName + '\BatMon.ScheduledTasks.dll' + ' Version';
$output = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{BatMon.ScheduledTasks.AssemblyVersion}', "$output" | Out-File "$workingFile"


<#
#	Compile NSIS Script
#>
$output = cmd /c "`"C:\Program Files (x86)\NSIS\makensis.exe`" `"$workingFile`"" 2`>`&1
Write $output