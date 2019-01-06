#################################################################### 
##Copyright (C) 2018 Peter Varney - All Rights Reserved
## You may use, distribute and modify this code under the
## terms of the MIT license, 
##
## You should have received a copy of the MIT license with
## this file. If not, visit : https://github.com/fatalwall/BatMon
####################################################################

;Required modules
!include MUI.nsh
!include "LogicLib.nsh"


;Definitions
!define PRODUCT_NAME "&{Plugin.AssemblyTitle}"
!define PRODUCT_VERSION "&{Plugin.AssemblyVersion}"
!define PRODUCT_PUBLISHER "&{Plugin.AssemblyCompany}"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\BatMon\Plugins\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

;MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"
!define MUI_DIRECTORYPAGE_VARIABLE $INSTDIR

;MUI Installer Pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

;MUI Uninstaller Pages
!insertmacro MUI_UNPAGE_INSTFILES

;MUI Language Files
!insertmacro MUI_LANGUAGE "English"

;Settings
Name "${PRODUCT_NAME}"
OutFile "${PRODUCT_NAME} ${PRODUCT_VERSION}.exe"
RequestExecutionLevel admin
InstallDir "$PROGRAMFILES\BatMon\Plugins\${PRODUCT_NAME}"
ShowInstDetails show
ShowUnInstDetails show

############################
## Install
############################
	Section "Plugins" plgn
		SectionIn RO

        CreateDirectory "$INSTDIR"
		;Write Service Files
        File "/oname=$INSTDIR\&{Plugin.AssemblyName}.dll" "..\..\..\&{Plugin.AssemblyName}\bin\Release\&{Plugin.AssemblyName}.dll"

		SetOverwrite off ;Do not overwrite Config files
        IfFileExists "..\..\Default Config\&{Plugin.AssemblyName}.dll.config" 0 file_not_found
    	File "/oname=$INSTDIR\&{Plugin.AssemblyName}.dll.config" "..\..\Default Config\&{Plugin.AssemblyName}.dll.config"
        file_not_found:
		SetOverwrite on ;Allow Over files
	SectionEnd

    Section -Post
		WriteUninstaller "$INSTDIR\uninst.exe"
		
        WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Version" "&{Plugin.AssemblyVersion}"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
	SectionEnd

############################
## Uninstall
############################
	Function un.onUninstSuccess
		HideWindow
		;GUI Confirm Success. Skipped if Silent parameter passed
		MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer." /SD IDOK
	FunctionEnd

	Function un.onInit
		;GUI Confirm Install Request. Skipped if Silent parameter passed
		MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" /SD IDYES IDYES yes IDNO no
		no:
		Abort
		yes:
	FunctionEnd

	Section Uninstall
		############################
		## Standard Cleanup Tasks
		############################
		;Delete the the uninstaller
		Delete "$INSTDIR\uninst.exe"
		;Delete installation folder if empty
		RMDir /r /REBOOTOK "$INSTDIR"
		;Delete Add/Remove Programs registry keys
		DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
		
		SetAutoClose true
	SectionEnd