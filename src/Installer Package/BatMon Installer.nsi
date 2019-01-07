#################################################################### 
##Copyright (C) 2018 Peter Varney - All Rights Reserved
## You may use, distribute and modify this code under the
## terms of the MIT license, 
##
## You should have received a copy of the MIT license with
## this file. If not, visit : https://github.com/fatalwall/BatMon
####################################################################

;Required modules
!include MUI2.nsh
!include "strExplode.nsh"
!include "DotNetVersion.nsh"
!include "LogicLib.nsh"


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
Page Custom PluginsShow PluginsLeave
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

Var PluginCounter
Var PluginVersion
Var PluginCurVersion
Var PluginChecked
Var PluginName
Var PluginUninstaller

ReserveFile `Plugins.ini`


############################
## Install
############################
	Section "!.Net 4.7" Net4
		SectionIn RO
		SetOverwrite on

		${DotNetVersion} $0 '4' '7' '*'
		${If} $0 == "FALSE"
			File "/oname=$EXEDIR\.Net 4.7 Framework\NDP47-KB3186500-Web.exe" "..\..\Dependencies\.Net 4.7 Framework\NDP47-KB3186500-Web.exe"
			DetailPrint "Installing .Net Framework 4.7 (Installation will take several minutes)"
			nsExec::Exec '"$EXEDIR\.Net 4.7 Framework\NDP47-KB3186500-Web.exe" /q /norestart'
		${Else}
			DetailPrint ".Net Framework 4.7 or greater already installed"
		${EndIf}
	SectionEnd

	Section "BatMon Core Files (Required)" core
		SectionIn RO
		;Check if Upgrade/Repair
		;FIXME

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
		DetailPrint "BatMon Service Installed"

		SetShellVarContext all
		CreateShortCut '$desktop\${PRODUCT_NAME} Dashboard.lnk' 'http://localhost:7865' '' "$INSTDIR\BatMon.exe" 2 SW_SHOWMAXIMIZED
		DetailPrint "Desktop shortcut to Dashboard created"
	SectionEnd

	Section "Plugins" plgn
		SectionIn RO
		;Silent option setting
		IfSilent 0 plgn_Process
			;FIXME
		plgn_Process:
		;This function loops through all plugin list items and installs, uninstalls, or skips based on whats needed
		SetOutPath "$EXEDIR\Plugins"
		File /r "Plugins\*"
		StrCpy $PluginCounter 0
		${Do}
			IntOp $PluginCounter $PluginCounter + 1 ;Increment Conter

			ReadINIStr $PluginName `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Text`
			${If} $PluginName == ``
				${ExitDo} ;End of list. Exit loop
			${EndIf}
			ReadINIStr $PluginVersion `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `SubItem1`
			ReadINIStr $PluginCurVersion `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `SubItem2`
			ReadINIStr $PluginChecked `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Checked`

			${If} $PluginChecked = 1
				;Run Plugin Installer
				ExecWait `"$EXEDIR\$PluginName $PluginVersion.exe" /S /D=$INSTDIR\Plugins` $0
				${If} $0 = 0
					DetailPrint "$PluginName Successfully Installed"
				${Else}
					DetailPrint "$PluginName Failed to Install with Exit Code $0"
				${EndIf}
			${Else}
				${IfNot} $PluginCurVersion == ``
					;Uninstall the plugin
					ReadRegStr $PluginUninstaller HKLM "${PRODUCT_UNINST_KEY}\Plugins\$PluginName" "UninstallString"
					ExecWait `"$PluginUninstaller" /S` $0
					${If} $0 = 0
						DetailPrint "$PluginName Successfully Uninstalled"
					${Else}
						DetailPrint "$PluginName Failed to Uninstall with Exit Code $0"
					${EndIf}
				${EndIf}
			${EndIf}
		${Loop}
	SectionEnd

	Section -Post
		WriteUninstaller "$INSTDIR\uninst.exe"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\${PRODUCT_NAME}.exe"

		;Start Service
		SimpleSC::StartService "$(^Name)" "" 30
		Pop $0
		IntCmp $0 0 +3
		MessageBox MB_OK|MB_ICONSTOP "$(^Name) installation failed: could not start service." /SD IDOK
		Abort
		DetailPrint "BatMon Service started successfully"
	SectionEnd

	Function .onInit
		InitPluginsDir
		File `/oname=$PLUGINSDIR\Plugins.ini` `Plugins.ini`
	FunctionEnd

############################
## Page Custom Plugins
############################
	Function PluginsShow
		!insertmacro MUI_HEADER_TEXT "Available Plugins"  "Select plugins to install."
		StrCpy $PluginCounter 0
		;Identify plugins already installed
		${Do} 
			IntOp $PluginCounter $PluginCounter + 1 ;Increment Conter

			ReadINIStr $PluginName `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Text`
			${If} $PluginName == ``
				${ExitDo} ;End of list. Exit loop
			${EndIf}
		
			ReadRegStr $PluginCurVersion HKLM "${PRODUCT_UNINST_KEY}\Plugins\$PluginName" "Version"
			${IfNot} $PluginCurVersion == ``
				WriteINIStr `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `SubItem2` `$PluginCurVersion`
				WriteINIStr `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Checked` `1` 
			${EndIf}
		${Loop}
		;Populate plugin list
		EmbeddedLists::Dialog `$PLUGINSDIR\Plugins.ini`
		Pop $R0
	FunctionEnd

	Function PluginsLeave
		;Clear Ini Check Value
		StrCpy $PluginCounter 0
		${Do}
			IntOp $PluginCounter $PluginCounter + 1 ;Increment Conter

			ReadINIStr $PluginName `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Text`
			${If} $PluginName == ``
				${ExitDo} ;End of list. Exit loop
			${EndIf}
			WriteINIStr `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Checked` `0` 
		${lOOP}

		;Set Ini Check Value for selected
		${Do} 
			Pop $R0
			${If} $R0 == /END
				${ExitDo} ;End of list. Exit loop
			${EndIf}
			WriteINIStr `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Checked` `1` 
		${Loop}
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

		;Uninstall All Plugins I know of 
		StrCpy $PluginCounter 0
		${Do}
			IntOp $PluginCounter $PluginCounter + 1 ;Increment Conter

			ReadINIStr $PluginName `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Text`
			${If} $PluginName == ``
				${ExitDo} ;End of list. Exit loop
			${EndIf}
			ReadRegStr $PluginUninstaller HKLM "${PRODUCT_UNINST_KEY}\Plugins\$PluginName" "UninstallString"
			ExecWait `"$PluginUninstaller" /S` $0
			${If} $0 = 0
				DetailPrint "$PluginName Successfully Uninstalled"
			${Else}
				DetailPrint "$PluginName Failed to Uninstall with Exit Code $0"
			${EndIf}
		${lOOP}
		;Uninstall Any Plugins I do not know of
		FindFirst $0 $1 "$INSTDIR\Plugins\*.*"
		loop:
		StrCmp $1 "" done
		${If} ${FileExists} "$INSTDIR\Plugins\$1\*.exe"
			DetailPrint "Subdir found: $INSTDIR\$1"
			ExecWait `"$INSTDIR\Plugins\$1\uninst.exe" /S` $0
			${If} $0 = 0
				DetailPrint "$PluginName Successfully Uninstalled"
			${Else}
				DetailPrint "$PluginName Failed to Uninstall with Exit Code $0"
			${EndIf}
		${EndIf}
		FindNext $0 $1
		Goto loop
		done:
		FindClose $0

		;Delete Desktop Shortcut
		SetShellVarContext all
		Delete "$desktop\${PRODUCT_NAME} Dashboard.lnk"

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