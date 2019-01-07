param (
    [string]$ProjectName,
    [string]$ConfigurationName,
    [string]$SolutionDir
)
$sourceDir = $SolutionDir + 'Installer Package\'
$sourceFileName = 'Plugin Installer.nsi'
$sourceFile = $sourceDir + $sourceFileName
$workingDir = $sourceDir + 'bin\' + $ConfigurationName + '\'
$workingFile = $workingDir + 'Plugin_' + $ProjectName + '.nsi'

set-location "$sourceDir"

<#
#	Copy NSIS Script into directory matching VS Build option
#>
Copy-Item -Path "$sourceFile" -Destination "$workingFile" -Force

<#
#	Insert Assembly Varables
#>
# Version
$args = $SolutionDir + $ProjectName + '\bin\'+ $ConfigurationName + '\' + $ProjectName + '.dll' + ' Version';
$output = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
if ($ConfigurationName -eq 'Release') {
    $output -match '\d*.\d*.\d*'
    $output = $matches[0]
    (gc "$workingFile") -replace '&{Plugin.AssemblyVersion}', "$output" | Out-File "$workingFile"
}else{
    (gc "$workingFile") -replace '&{Plugin.AssemblyVersion}', "$output" | Out-File "$workingFile"
}

# CompanyName
$args = $SolutionDir + $ProjectName + '\bin\'+ $ConfigurationName + '\' + $ProjectName + '.dll' + ' Company';
$output = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{Plugin.AssemblyCompany}', "$output" | Out-File "$workingFile"

# ProductName
$args = $SolutionDir + $ProjectName + '\bin\'+ $ConfigurationName + '\' + $ProjectName + '.dll' + ' Product';
$output = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{Plugin.AssemblyTitle}', "$output" | Out-File "$workingFile"

# Assembly File Name
(gc "$workingFile") -replace '&{Plugin.AssemblyName}', "$ProjectName" | Out-File "$workingFile"

# Build (Release or Debug)
(gc "$workingFile") -replace '&{Plugin.BuildType}', "$ConfigurationName" | Out-File "$workingFile"

<#
#	Compile NSIS Script
#>
$output = cmd /c "`"C:\Program Files (x86)\NSIS\makensis.exe`" `"$workingFile`"" 2`>`&1
if ($output -like '*Total size:*bytes*')
{ $output = 'NSIS script successfully compiled for ' + $ProjectName }
Write $output