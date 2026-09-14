; Inno Setup script for KeyPress.
;
; Packages the net48 publish output (a single dependency-free exe — see
; KeyPress.csproj) into a standalone Setup.exe with a Start Menu shortcut
; and a normal Add/Remove Programs entry. Requires the free Inno Setup
; compiler (https://jrsoftware.org/isinfo.php) to build.
;
; Build the app first:
;   dotnet publish KeyPress\KeyPress.csproj -c Release -p:PublishProfile=FolderProfile
; Then compile this script (from the Inno Setup IDE, or via the command line):
;   ISCC.exe installer\KeyPress.iss

#define MyAppName "KeyPress"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "nathancarlmitchell"
#define MyAppURL "https://github.com/nathancarlmitchell/KeyPress"
#define MyAppExeName "KeyPress.exe"
#define PublishDir "..\KeyPress\bin\Release\V3"

[Setup]
; Fixed, random GUID identifying this app across versions — regenerating it
; would make Windows treat an upgrade as a separate, unrelated install.
AppId={{8F2B6E4C-6C1B-4C7A-9C7C-9C6A2E3E7B41}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
; Let the installer adapt to whichever the user picks in the elevation
; prompt, instead of hard-requiring admin rights just to install a small
; utility into Program Files.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
SetupIconFile=..\KeyPress\AppIcon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
OutputDir=Output
OutputBaseFilename=KeyPress-Setup

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishDir}\KeyPress.exe.config"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
