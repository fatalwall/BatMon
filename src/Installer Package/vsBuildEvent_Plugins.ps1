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
$ErrorCode = 0

set-location "$sourceDir"

<#
#	Copy NSIS Script into directory matching VS Build option
#>
Copy-Item -Path "$sourceFile" -Destination "$workingFile" -Force
#Licence File
copy "..\..\LICENSE"  -Destination (New-Item "$workingDir" -Type container -force) -Container -force

<#
#	Insert Assembly Varables
#>
# Version
$args = $SolutionDir + $ProjectName + '\bin\'+ $ConfigurationName + '\' + $ProjectName + '.dll' + ' Version';
$Version = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{Plugin.ProductVersion}', "$Version" | Out-File "$workingFile"
if ($ConfigurationName -eq 'Release') {
    if($Version -match '\d*.\d*.\d*') { $Version = $matches[0] }
    (gc "$workingFile") -replace '&{Plugin.AssemblyVersion}', "$Version" | Out-File "$workingFile"
}else{
    (gc "$workingFile") -replace '&{Plugin.AssemblyVersion}', "$Version" | Out-File "$workingFile"
}

# CompanyName
$args = $SolutionDir + $ProjectName + '\bin\'+ $ConfigurationName + '\' + $ProjectName + '.dll' + ' Company';
$Company = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{Plugin.AssemblyCompany}', "$Company" | Out-File "$workingFile"

# ProductName
$args = $SolutionDir + $ProjectName + '\bin\'+ $ConfigurationName + '\' + $ProjectName + '.dll' + ' Product';
$Product = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{Plugin.AssemblyTitle}', "$Product" | Out-File "$workingFile"

# Copyright
$args = $SolutionDir + $ProjectName + '\bin\'+ $ConfigurationName + '\' + $ProjectName + '.dll' + ' Copyright';
$Copyright = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{Plugin.AssemblyCopyright}', "$Copyright" | Out-File "$workingFile"

# Year
$Year = Get-Date -UFormat "%Y"
(gc "$workingFile") -replace '&{Year}', "$Year" | Out-File "$workingFile"

# Description
$args = $SolutionDir + $ProjectName + '\bin\'+ $ConfigurationName + '\' + $ProjectName + '.dll' + ' Description';
$Description = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{Plugin.AssemblyDescription}', "$Description" | Out-File "$workingFile"

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
else { $ErrorCode += -1 }
Write-Output $output

exit $ErrorCode