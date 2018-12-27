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
!include "strExplode.nsh"
!include "DotNetVersion.nsh"


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
!define MUI_DIRECTORYPAGE_VARIABLE $INSTDIR

;MUI Installer Pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE"
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
		SectionIn RO
		;Write Service Files
		File "/oname=$INSTDIR\BatMon.exe" "..\..\..\BatMon\bin\Release\BatMon.exe"
		File "/oname=$INSTDIR\BatMon.Framework.dll" "..\..\..\BatMon\bin\Release\BatMon.Framework.dll"
		File "/oname=$INSTDIR\BatMon.Framework.Web.dll" "..\..\..\BatMon\bin\Release\BatMon.Framework.Web.dll"
		File "/oname=$INSTDIR\Nancy.dll" "..\..\..\BatMon\bin\Release\Nancy.dll"
		File "/oname=$INSTDIR\Nancy.Hosting.Self.dll" "..\..\..\BatMon\bin\Release\Nancy.Hosting.Self.dll"
		File "/oname=$INSTDIR\Newtonsoft.Json.dll" "..\..\..\BatMon\bin\Release\Newtonsoft.Json.dll"
		File "/oname=$INSTDIR\NLog.dll" "..\..\..\BatMon\bin\Release\NLog.dll"
		SetOverwrite off ;Do not overwrite Config files
		File "/oname=$INSTDIR\BatMon.exe.config" "..\..\Default Config\BatMon.exe.config"
		SetOverwrite on ;Allow Over files

		;Create Service
		SimpleSC::InstallService "$(^Name)" "BatMon (System Monitor)" "16" "2" "$INSTDIR\BatMon.exe" "" "" ""
		Pop $0
		IntCmp $0 0 +3
		MessageBox MB_OK|MB_ICONSTOP "$(^Name) installation failed: could not create service." /SD IDOK
		Abort
		
		;Start Service
		SimpleSC::StartService "$(^Name)" "" 30
		Pop $0
		IntCmp $0 0 +3
		MessageBox MB_OK|MB_ICONSTOP "$(^Name) installation failed: could not start service." /SD IDOK
		Abort

		SetShellVarContext all
		CreateShortCut '$desktop\${PRODUCT_NAME} Dashboard.lnk' 'http://localhost:7865' '' "$INSTDIR\BatMon.exe" 2 SW_SHOWMAXIMIZED
	SectionEND

	SectionGroup "Plugins" plgn
		Section "All Plugins" plgnAllPlugins
			SetOverwrite on
			CreateDirectory "$INSTDIR\Plugins"
			File "/oname=$INSTDIR\Plugins\BatMon.AllPlugins.dll" "..\..\..\BatMon.AllPlugins\bin\Release\BatMon.AllPlugins.dll"
			WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}\Plugins" "AllPlugins" "&{BatMon.AllPlugins.AssemblyVersion}"	
		SectionEnd
		Section "Scheduled Tasks" plgnScheduledTasks
			SetOverwrite on
			CreateDirectory "$INSTDIR\Plugins"
			CreateDirectory "$INSTDIR\Plugins\BatMon.ScheduledTasks"
			File "/oname=$INSTDIR\Plugins\BatMon.ScheduledTasks\BatMon.ScheduledTasks.dll" "..\..\..\BatMon.ScheduledTasks\bin\Release\BatMon.ScheduledTasks.dll"
			File "/oname=$INSTDIR\Plugins\BatMon.ScheduledTasks\Microsoft.Win32.TaskScheduler.dll" "..\..\..\BatMon.ScheduledTasks\bin\Release\Microsoft.Win32.TaskScheduler.dll"
			SetOverwrite off ;Do not overwrite Config files
			File "/oname=$INSTDIR\Plugins\BatMon.ScheduledTasks\BatMon.ScheduledTasks.dll.config" "..\..\Default Config\BatMon.ScheduledTasks.dll.config"
			SetOverwrite on ;Allow Over files
			WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}\Plugins" "ScheduledTasks" "&{BatMon.ScheduledTasks.AssemblyVersion}"
		SectionEnd
	SectionGroupEnd

	Section -Post
		WriteUninstaller "$INSTDIR\uninst.exe"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\${PRODUCT_NAME}.exe"

		;Start Service

	SectionEnd

	Function .onInit

		;Check if upgrade or repair and adjust selected sections accordingly

		;have silent install support upgradeing/repairing existing components & installing new

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
		;Service
		SimpleSC::ExistsService "$(^Name)"
		Pop $0
		IntCmp $0 0 +1 SkipService
		;Stop Service
		SimpleSC::StopService "$(^Name)" 1 30
		Pop $0
		IntCmp $0 0 +3
		MessageBox MB_OK|MB_ICONSTOP "$(^Name) uninstall failed: could not stop service." /SD IDOK
		Abort
		;Delete Service
		SimpleSC::RemoveService "$(^Name)"
		Pop $0
		IntCmp $0 0 +3
		MessageBox MB_OK|MB_ICONSTOP "$(^Name) uninstall failed: could not remove service." /SD IDOK
		Abort
		SkipService:

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