#################################################################### 
##Copyright (C) 2018 Peter Varney - All Rights Reserved
## You may use, distribute and modify this code under the
## terms of the MIT license, 
##
## You should have received a copy of the MIT license with
## this file. If not, visit : https://github.com/fatalwall/BatMon
####################################################################

;Required modules
!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "strExplode.nsh"
!include "DotNetVersion.nsh"
!include "LogicLib.nsh"


;Definitions
!define PRODUCT_NAME "&{BatMon.AssemblyTitle}"
!define PRODUCT_VERSION "&{BatMon.AssemblyVersion}"
!define PRODUCT_PUBLISHER "&{BatMon.AssemblyCompany}"
!define PRODUCT_WEB_SITE "https://github.com/fatalwall/BatMon"
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

Var PluginCounter
Var PluginVersion
Var PluginCurVersion
Var PluginChecked
Var PluginDisableCheck
Var PluginName
Var PluginUninstaller
Var argPlugins
Var argPlugin

ReserveFile `Plugins.ini`

############################
## File Details
############################
	VIProductVersion "&{BatMon.ProductVersion}"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "&{BatMon.AssemblyTitle}"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "&{BatMon.AssemblyCompany}"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "&{BatMon.AssemblyCopyright}"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "&{BatMon.AssemblyDescription}"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "&{BatMon.AssemblyVersion}"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "&{BatMon.AssemblyVersion}"

############################
## Install
############################
	Section "!.Net 4.7" Net4
		SectionIn RO
		SetOutPath "$PLUGINSDIR"
		SetOverwrite on

		${DotNetVersion} $0 '4' '7' '*'
		${If} $0 == "FALSE"
			File "/oname=$PLUGINSDIR\NDP47-KB3186500-Web.exe" "..\..\Dependencies\.Net 4.7 Framework\NDP47-KB3186500-Web.exe"
			DetailPrint "Installing .Net Framework 4.7 (Installation will take several minutes)"
			nsExec::Exec '"$PLUGINSDIR\NDP47-KB3186500-Web.exe" /q /norestart'
			Pop $0
			${Switch} $0
				${Case} 0
					DetailPrint "Successfully installed .Net Framework 4.7"
					${Break}
				${Case} 1602
					DetailPrint "The user canceled installation of .Net Framework 4.7"
					Abort
					${Break}
				${Case} 1603
					DetailPrint "A fatal error occurred during installation of .Net Framework 4.7"
					Abort
					${Break}
				${Case} 1641
				${Case} 3010
					DetailPrint "A restart is required before you can install BatMon"
					Abort
					${Break}
				${Case} 5100
					DetailPrint "The user's computer does not meet system requirements"
					Abort
					${Break}
				${Default}
					DetailPrint ".Net Framework 4.7 failed to install with exit code $0"
					${Break}
			${EndSwitch}
		${Else}
			DetailPrint ".Net Framework 4.7 or greater already installed"
		${EndIf}
	SectionEnd

	Section "BatMon Core Files (Required)" core
		SectionIn RO
		SetRegView 32
		SetOutPath "$INSTDIR"
		;Remove Service if it exists
		SimpleSC::ExistsService "$(^Name)"
		Pop $0 ; = 0 Exists, <> 0 Doesnt Exist
		${if} $0 = 0
			;Service Exists
			SimpleSc::StopService "$(^Name)" 1 30
			;SimpleSC::RemoveService "$(^Name)"
			nsExec::Exec '"$INSTDIR\BatMon.exe" uninstall'
		${EndIf}

		;Write Service Files
		File /r /x "*.pdb" /x "*.config" /x "*.xml" /x "log4net.dll" /x "Plugins" "..\..\..\BatMon\bin\&{BatMon.BuildType}\*"
		 
		SetOverwrite off ;Do not overwrite Config files
		File "/oname=$INSTDIR\BatMon.exe.config" "..\..\Default Config\BatMon.exe.config"
		SetOverwrite on ;Allow Over files

		;Create Service
		;SimpleSC::InstallService "$(^Name)" "BatMon (System Monitor)" "16" "2" "$INSTDIR\BatMon.exe" "" "" ""
		nsExec::Exec '"$INSTDIR\BatMon.exe" install'
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
		SetRegView 32
		SetOutPath "$PLUGINSDIR"
		;This function loops through all plugin list items and installs, uninstalls, or skips based on whats needed
		File /r "Plugins\*"
		StrCpy $PluginCounter 0
		${Do}
			IntOp $PluginCounter $PluginCounter + 1 ;Increment Conter
			;Clear Variables
			StrCpy $PluginVersion ""
			StrCpy $PluginCurVersion ""
			StrCpy $PluginChecked ""

			ReadINIStr $PluginName `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Text`
			${If} $PluginName == ``
				${ExitDo} ;End of list. Exit loop
			${EndIf}
			ReadINIStr $PluginVersion `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `SubItem2`
			ReadINIStr $PluginCurVersion `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `SubItem3`
			ReadINIStr $PluginChecked `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Checked`

			${If} $PluginChecked = 1
				;Run Plugin Installer
				nsExec::Exec `"$PLUGINSDIR\$PluginName.exe" /S /D=$INSTDIR\Plugins` 
				Pop $0
				${If} $0 = 0
					DetailPrint "$PluginName Successfully Installed"
				${Else}
					DetailPrint "$PluginName Failed to Install with Exit Code $0"
				${EndIf}
			${Else}
				${IfNot} $PluginCurVersion == ``
					;Uninstall the plugin
					ReadRegStr $PluginUninstaller HKLM "${PRODUCT_UNINST_KEY}\Plugins\$PluginName" "UninstallString"
					nsExec::Exec `"$PluginUninstaller" /S` 
					Pop $0
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
		${ifNot} "$EXEPATH" == "$INSTDIR\inst.exe"
			CopyFiles "$EXEPATH" "$INSTDIR\inst.exe"
		${EndIf}
		SetRegView 32
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "ModifyPath" "$INSTDIR\inst.exe"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
		WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\${PRODUCT_NAME}.exe"

		nsExec::Exec 'EVENTCREATE /L APPLICATION /SO "$(^Name)" /T INFORMATION /ID 1000  /D "$(^Name) ${PRODUCT_VERSION} Installed Successfully"'

		;Start Service
		DetailPrint "Starting Service"
		SimpleSC::StartService "$(^Name)" "" 30
		Pop $0
		ClearErrors
		${IfNot} $0 = 0
			DetailPrint "BatMon Service could not start"
			MessageBox MB_OK|MB_ICONSTOP "$(^Name): could not start service." /SD IDOK
		${Else}
			DetailPrint "BatMon Service started successfully"
		${EndIf}
	SectionEnd

	Function .onInit
		SetRegView 32
		InitPluginsDir
		File `/oname=$PLUGINSDIR\Plugins.ini` `Plugins.ini`

		WriteINIStr `$PLUGINSDIR\Plugins.ini` `Icons` `Icon1` `$EXEDIR\icon1.ico`
 		WriteINIStr `$PLUGINSDIR\Plugins.ini` `Icons` `Icon2` `$EXEDIR\icon2.ico`

		############################
		## /P "Comma,Seperated,List"
		############################
		${GetParameters} $R0
		${GetOptions} $R0 "/ALL" $argPlugins
		IfErrors No_PALL_Parameter 0

		;Mark all Checked Value to 0
		StrCpy $PluginCounter 0
		${Do}
			IntOp $PluginCounter $PluginCounter + 1 ;Increment Conter
			ReadINIStr $PluginName `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Text`
			${If} $PluginName == ``
				${ExitDo} ;End of list. Exit loop
			${EndIf}
			WriteINIStr `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Checked` `1`
		${Loop}
		No_PALL_Parameter:
		############################
		## /P "Comma,Seperated,List"
		############################
		${GetParameters} $R0
		${GetOptions} $R0 "/P" $argPlugins
		${If} ${Errors}
			Goto No_Plugins_Parameter
		${EndIf}

		;Mark all Checked Value to 0
		StrCpy $PluginCounter 0
		${Do}
			IntOp $PluginCounter $PluginCounter + 1 ;Increment Conter
			ReadINIStr $PluginName `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Text`
			${If} $PluginName == ``
				${ExitDo} ;End of list. Exit loop
			${EndIf}
			ReadINIStr $PluginDisableCheck `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `DisableCheck`
			${If} $PluginDisableCheck <> 1
				WriteINIStr `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Checked` `0`
			${EndIf}
		${Loop}

		${strExplode} $1  "," "$argPlugins"
		${If} $1 > 0
			${do}
				Pop $argPlugin
				${If} ${Errors}
					ClearErrors
					${ExitDo}
				${EndIf}
				;Update Checked Value to 1
				StrCpy $PluginCounter 0
				${Do}
					IntOp $PluginCounter $PluginCounter + 1 ;Increment Conter
					ReadINIStr $PluginName `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Text`
					${If} $PluginName == ``
						ClearErrors
						GoTo ExitDoOneLeve ;End of list. Exit loop
					${EndIf}
					${If} $PluginName == $argPlugin
						WriteINIStr `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `Checked` `1`
					${EndIf}
				${Loop}
				ExitDoOneLeve:
			${loop}
		${EndIf}
		No_Plugins_Parameter:
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
				WriteINIStr `$PLUGINSDIR\Plugins.ini` `Item $PluginCounter` `SubItem3` `$PluginCurVersion`
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
			WriteINIStr `$PLUGINSDIR\Plugins.ini` `Item $R0` `Checked` `1` 
		${Loop}
	FunctionEnd

############################
## Uninstall
############################
	Function un.onUninstSuccess
		HideWindow
		;GUI Confirm Success. Skipped if Silent parameter passed
		nsExec::Exec 'EVENTCREATE /L APPLICATION /SO "$(^Name)" /T INFORMATION /ID 1000  /D "$(^Name) ${PRODUCT_VERSION} Uninstalled Successfully"'
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
		SimpleSC::ServiceIsRunning "$(^Name)"
		Pop $0
		${If} $0 = 0
			Pop $1
			${If} $1 = 1
				SimpleSC::StopService "$(^Name)" 1 30
				Pop $0
				IntCmp $0 0 +3
				MessageBox MB_OK|MB_ICONSTOP "$(^Name) uninstall failed: could not stop service." /SD IDOK
				Abort
			${EndIf}
		${EndIf}
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
		RMDir /REBOOTOK "$INSTDIR"
		;Delete Add/Remove Programs registry keys
		DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
		
		SetAutoClose true
	SectionEnd