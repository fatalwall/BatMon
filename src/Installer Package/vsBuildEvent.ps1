param (
    [string]$SolutionDir,
    [string]$ConfigurationName
)
$working = $SolutionDir + 'Installer Package'
$source = $SolutionDir + 'Installer Package\BatMon Installer.nsi'
$workingDir = $SolutionDir + 'Installer Package\bin\' + $ConfigurationName + '\'
$workingFile = $workingDir + 'BatMon Installer.nsi'
$ProjectName = "BatMon"

set-location "$working"

<#
#	Copy NSIS Script into directory matching VS Build option
#>
copy "$source"  -Destination (New-Item "$workingDir" -Type container -force) -Container -force
#Licence File
copy "..\..\LICENSE"  -Destination (New-Item "$workingDir" -Type container -force) -Container -force
#Plugin Selection List Icons
copy "Local.ico"  -Destination (New-Item "$workingDir" -Type container -force) -Container -force
copy "Online.ico"  -Destination (New-Item "$workingDir" -Type container -force) -Container -force


<#
#	Insert Assembly Varables
#	BatMan.exe		Windows Service
#>
# BatMan.exe Version
$args = $SolutionDir + 'BatMon\bin\'+ $ConfigurationName + '\BatMon.exe' + ' Version';
$Version = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{BatMon.ProductVersion}', "$Version" | Out-File "$workingFile"
if ($ConfigurationName -eq 'Release') {
    if($Version -match '\d*.\d*.\d*') { $Version = $matches[0] }
    (gc "$workingFile") -replace '&{BatMon.AssemblyVersion}', "$Version" | Out-File "$workingFile"
}else{
    (gc "$workingFile") -replace '&{BatMon.AssemblyVersion}', "$Version" | Out-File "$workingFile"
}


# BatMan.exe CompanyName
$args = $SolutionDir + 'BatMon\bin\'+ $ConfigurationName + '\BatMon.exe' + ' Company';
$output = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{BatMon.AssemblyCompany}', "$output" | Out-File "$workingFile"

# BatMan.exe ProductName
$args = $SolutionDir + 'BatMon\bin\'+ $ConfigurationName + '\BatMon.exe' + ' Product';
$output = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{BatMon.AssemblyTitle}', "$output" | Out-File "$workingFile"

# Copyright
$args = $SolutionDir + $ProjectName + '\bin\'+ $ConfigurationName + '\' + $ProjectName + '.dll' + ' Copyright';
$Copyright = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{BatMon.AssemblyCopyright}', "$Copyright" | Out-File "$workingFile"

# Year
$Year = Get-Date -UFormat "%Y"
(gc "$workingFile") -replace '&{Year}', "$Year" | Out-File "$workingFile"

# Description
$args = $SolutionDir + $ProjectName + '\bin\'+ $ConfigurationName + '\' + $ProjectName + '.dll' + ' Description';
$Description = cmd /c "GetAssemblyValue.exe $args" 2`>`&1
(gc "$workingFile") -replace '&{BatMon.AssemblyDescription}', "$Description" | Out-File "$workingFile"

# &{BatMon.BuildType}
(gc "$workingFile") -replace '&{BatMon.BuildType}', "$ConfigurationName" | Out-File "$workingFile"


<#
#	Build Plugin INI file
#>
Get-ChildItem "$workingDir\Plugins" -Filter *.exe | 
Foreach-Object {
    $args = '"' + $_.FullName + '"'
    $Product = cmd /c "GetAssemblyValue.exe $args Product" 2`>`&1
    $Company = cmd /c "GetAssemblyValue.exe $args Company" 2`>`&1
    $Version = cmd /c "GetAssemblyValue.exe $args Version" 2`>`&1

    $args = '"' + $SolutionDir + 'Installer Package\bin\' + $ConfigurationName + '\Plugins.ini"' + ' "' + $Product + '" SubItem1 "' + $Company + '" SubItem2 "' + $Version + '" SubItem3 "" IconIndex 1';
    $output = cmd /c "NSISEmbeddedListBuilder.exe $args" 2`>`&1
    $output = 'Updating Plugins.ini: ' + $output
    if ($output -like '*File saved successfully:*') {}
    else { $ErrorCode += -2 }
    Write-Output $output
    Write-Output "Product: $Product"
    Write-Output "Company: $Company"
    Write-Output "Version: $Version"
}

Get-ChildItem "$workingDir\Plugins" -Filter *.exe | 
Foreach-Object {
    $args = '"' + $_.FullName + '"'
    $Product = cmd /c "GetAssemblyValue.exe $args Product" 2`>`&1
    $Company = cmd /c "GetAssemblyValue.exe $args Company" 2`>`&1
    $Version = cmd /c "GetAssemblyValue.exe $args Version" 2`>`&1

    $args = '"' + $SolutionDir + 'Installer Package\bin\' + $ConfigurationName + '\Plugins.ini"' + ' "' + $Product + '" SubItem1 "' + $Company + '" SubItem2 "' + $Version + '" SubItem3 "" IconIndex 1';
    $output = cmd /c "NSISEmbeddedListBuilder.exe $args" 2`>`&1
    $output = 'Updating Plugins.ini: ' + $output
    if ($output -like '*File saved successfully:*') {}
    else { $ErrorCode += -2 }
    Write-Output $output
    Write-Output "Product: $Product"
    Write-Output "Company: $Company"
    Write-Output "Version: $Version"
}

Get-ChildItem "$workingDir\Plugins\Required" -Filter *.exe | 
Foreach-Object {
    $args = '"' + $_.FullName + '"'
    $Product = cmd /c "GetAssemblyValue.exe $args Product" 2`>`&1
    $Company = cmd /c "GetAssemblyValue.exe $args Company" 2`>`&1
    $Version = cmd /c "GetAssemblyValue.exe $args Version" 2`>`&1

    $args = '"' + $SolutionDir + 'Installer Package\bin\' + $ConfigurationName + '\Plugins.ini"' + ' "' + $Product + '" SubItem1 "' + $Company + '" SubItem2 "' + $Version + '" SubItem3 "" IconIndex 1 Checked 1 DisableCheck 1';
    $output = cmd /c "NSISEmbeddedListBuilder.exe $args" 2`>`&1
    $output = 'Updating Plugins.ini: ' + $output
    if ($output -like '*File saved successfully:*') {}
    else { $ErrorCode += -2 }
    Write-Output $output
    Write-Output "Product: $Product"
    Write-Output "Company: $Company"
    Write-Output "Version: $Version"
}

Get-ChildItem "$workingDir\Plugins\Default" -Filter *.exe | 
Foreach-Object {
    $args = '"' + $_.FullName + '"'
    $Product = cmd /c "GetAssemblyValue.exe $args Product" 2`>`&1
    $Company = cmd /c "GetAssemblyValue.exe $args Company" 2`>`&1
    $Version = cmd /c "GetAssemblyValue.exe $args Version" 2`>`&1

    $args = '"' + $SolutionDir + 'Installer Package\bin\' + $ConfigurationName + '\Plugins.ini"' + ' "' + $Product + '" SubItem1 "' + $Company + '" SubItem2 "' + $Version + '" SubItem3 "" IconIndex 1 Checked 1';
    $output = cmd /c "NSISEmbeddedListBuilder.exe $args" 2`>`&1
    $output = 'Updating Plugins.ini: ' + $output
    if ($output -like '*File saved successfully:*') {}
    else { $ErrorCode += -2 }
    Write-Output $output
    Write-Output "Product: $Product"
    Write-Output "Company: $Company"
    Write-Output "Version: $Version"
}

Get-ChildItem "$workingDir\Plugins\Optional" -Filter *.exe | 
Foreach-Object {
    $args = '"' + $_.FullName + '"'
    $Product = cmd /c "GetAssemblyValue.exe $args Product" 2`>`&1
    $Company = cmd /c "GetAssemblyValue.exe $args Company" 2`>`&1
    $Version = cmd /c "GetAssemblyValue.exe $args Version" 2`>`&1

    $args = '"' + $SolutionDir + 'Installer Package\bin\' + $ConfigurationName + '\Plugins.ini"' + ' "' + $Product + '" SubItem1 "' + $Company + '" SubItem2 "' + $Version + '" IconIndex 1 Checked 0 SubItem3 ""';
    $output = cmd /c "NSISEmbeddedListBuilder.exe $args" 2`>`&1
    $output = 'Updating Plugins.ini: ' + $output
    if ($output -like '*File saved successfully:*') {}
    else { $ErrorCode += -2 }
    Write-Output $output
    Write-Output "Product: $Product"
    Write-Output "Company: $Company"
    Write-Output "Version: $Version"
}


<#
#	Compile NSIS Script
#>
$output = cmd /c "`"C:\Program Files (x86)\NSIS\makensis.exe`" `"$workingFile`"" 2`>`&1
if ($output -like '*Total size:*bytes*')
{ $output = 'NSIS script successfully compiled for BatMon' }
else { $ErrorCode += -1 }
Write-Output $output

exit $ErrorCode
