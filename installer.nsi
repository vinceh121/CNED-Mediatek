!include "MUI2.nsh"
!define MUI_ABORTWARNING ; enables confirm dialog on abort

Name "MediaTek"
OutFile "mediatek-installer.exe"
Unicode True
RequestExecutionLevel user

InstallDir "$LOCALAPPDATA\MediaTek"
InstallDirRegKey HKCU "Software\MediaTek" ""

; MUI pages
!insertmacro MUI_PAGE_WELCOME

!define MUI_LICENSEPAGE_RADIOBUTTONS ; confirm radios
!insertmacro MUI_PAGE_LICENSE "LICENSE"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "French"

InstType "Complet" IT_FULL
InstType "Normal" IT_NORMAL

Section "!Installer MediaTek" installmediatek
	SectionInstType ${IT_FULL} ${IT_NORMAL} RO
	SetOutPath $INSTDIR

	File /r "bin\Release\net7.0\*"

	CreateShortcut "$DESKTOP\MediaTek.lnk" "$INSTDIR\mediatek.exe"
	CreateShortcut "$SMPROGRAMS\MediaTek.lnk" "$INSTDIR\mediatek.exe"

	WriteUninstaller "$INSTDIR\Uninstaller.exe"
SectionEnd

Section "Installer GTK" installgtk
	SectionInstType ${IT_FULL}
	SetOutPath $INSTDIR

	File /r "gtk\*"
SectionEnd

LangString DESC_installmediatek ${LANG_FRENCH} "Installer l'application MediaTek"

LangString DESC_installgtk ${LANG_FRENCH} "Installer les bibliothèques GTK, GDK, GLib, etc."

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${installmediatek} $(DESC_installmediatek)
  !insertmacro MUI_DESCRIPTION_TEXT ${installgtk} $(DESC_installgtk)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

Section "un.Uninstaller"
	Delete "$INSTDIR\Uninstaller.exe"
	RMDir /r "$INSTDIR"

	DeleteRegKey /ifempty HKCU "Software\MediaTek"
SectionEnd

