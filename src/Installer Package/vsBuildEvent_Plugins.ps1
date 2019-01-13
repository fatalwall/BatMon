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
Write $output

# Insert/Update Plugins.ini
$args = '"' + $SolutionDir + 'Installer Package\bin\' + $ConfigurationName + '\Plugins.ini"' + ' "' + $Product + '" SubItem1 "' + $Company + '" SubItem2 "' + $Version + '" SubItem3 ""';
$output = cmd /c "NSISEmbeddedListBuilder.exe $args" 2`>`&1
$output = 'Updating Plugins.ini: ' + $output
if ($output -like '*File saved successfully:*') {}
else { $ErrorCode += -2 }
Write $output

exit $ErrorCode