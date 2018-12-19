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
!include DotNetVersion.nsh

;Definitions
!define PRODUCT_NAME "&{BatMon.AssemblyTitle}"
!define PRODUCT_VERSION "&{BatMon.AssemblyVersion}"
!define PRODUCT_PUBLISHER "&{BatMon.AssemblyCompany}"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

;MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"
!define MUI_DIRECTORYPAGE_VARIABLE $PluginsFolder

;MUI Installer Pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE ""
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_COMPONENTS
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
InstallDir "$PROGRAMFILES\${PRODUCT_NAME}"
ShowInstDetails show
ShowUnInstDetails show

############################
## Install
############################
	Section "BatMon Core Files (Required)" core
		SetShellVarContext all
		CreateShortCut '$desktop\${PRODUCT_NAME} Dashboard.lnk' 'http://localhost:7865' '' "$INSTDIR\BatMon.exe" 2 SW_SHOWMAXIMIZED
	SectionEND

	SectionGroup "Plugins" plgn
		Section "All Plugins" plgnAllPlugins
			SetOverwrite on
			CreateDirectory "$INSTDIR\Plugins"
			File "/oname=$INSTDIR\Plugins\BatMon.AllPlugins.dll" "..\BatMon.AllPlugins\bin\Release\BatMon.AllPlugins.dll"
			WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}\Plugins" "AllPlugins" "&{BatMon.AllPlugins.AssemblyVersion}"	
		SectionEnd
		Section "Scheduled Tasks" plgnScheduledTasks
			SetOverwrite on
			CreateDirectory "$INSTDIR\Plugins"
			CreateDirectory "$INSTDIR\Plugins\BatMon.ScheduledTasks"
			File "/oname=$INSTDIR\Plugins\BatMon.ScheduledTasks\BatMon.ScheduledTasks.dll" "..\BatMon.ScheduledTasks\bin\Release\BatMon.ScheduledTasks.dll"
			File "/oname=$INSTDIR\Plugins\BatMon.ScheduledTasks\Microsoft.Win32.TaskScheduler.dll" "..\BatMon.ScheduledTasks\bin\Release\Microsoft.Win32.TaskScheduler.dll"
			SetOverwrite off ;Do not overwrite Config files
			File "/oname=$INSTDIR\Plugins\BatMon.ScheduledTasks\BatMon.ScheduledTasks.dll.config" "Default Config\BatMon.ScheduledTasks.dll.config"
			WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}\Plugins" "ScheduledTasks" "&{BatMon.WindowsShare.AssemblyVersion}"
		SectionEnd
	SectionGroupEnd

	Section -Post
		WriteUninstaller "$INSTDIR\uninst.exe"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\${PRODUCT_NAME}.exe"
	SectionEnd

	Function .onInit
	FunctionEnd

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
		## Unique Cleanup Tasks
		############################
		;Stop Service

		;Delete Service

		;Delete Files

		;Delete Desktop Shortcut
		SetShellVarContext all
		Delete "$desktop\${PRODUCT_NAME} Dashboard.lnk"

		############################
		## Standard Cleanup Tasks
		############################
		;Delete the the uninstaller
		Delete "$INSTDIR\uninst.exe"
		;Delete installation folder if empty
		RMDir "$INSTDIR"
		;Delete Add/Remove Programs registry keys
		DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
		
		SetAutoClose true
	SectionEnd


############################
## Descriptions of Sections
############################
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${core} "Windows Service"
  !insertmacro MUI_DESCRIPTION_TEXT ${plgn} "Official plugins"
  !insertmacro MUI_DESCRIPTION_TEXT ${plgnAllPlugins} "Aggregate of all other plugin results. Allows for a single URI to pull all results from"
  !insertmacro MUI_DESCRIPTION_TEXT ${plgnScheduledTasks} "Provides code for the last run attenpts of scheudled tasks"
!insertmacro MUI_FUNCTION_DESCRIPTION_END