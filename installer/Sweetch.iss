#ifndef AppVersion
  #define AppVersion "2.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\publish"
#endif
#ifndef ArtifactsDir
  #define ArtifactsDir "..\artifacts"
#endif

[Setup]
AppId={{CD496C86-0CF4-47DB-BFAA-2BBEA538846B}
AppName=Sweetch
AppVersion={#AppVersion}
AppVerName=Sweetch {#AppVersion}
AppPublisher=mmmorozov
AppPublisherURL=https://t.me/mmmorozov
AppSupportURL=https://github.com/krllmrzv/Sweetch/issues
DefaultDirName={localappdata}\Programs\Sweetch
DefaultGroupName=Sweetch
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
MinVersion=10.0.17763
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#ArtifactsDir}
OutputBaseFilename=Sweetch-{#AppVersion}-Setup-win-x64
SetupIconFile=..\src\Sweetch.App\Assets\Sweetch.ico
UninstallDisplayIcon={app}\Sweetch.exe
UninstallDisplayName=Sweetch
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{userprograms}\Sweetch"; Filename: "{app}\Sweetch.exe"
Name: "{userdesktop}\Sweetch"; Filename: "{app}\Sweetch.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Sweetch.exe"; Description: "{cm:LaunchProgram,Sweetch}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\Sweetch.exe"; Parameters: "--unregister"; Flags: runhidden waituntilterminated; RunOnceId: "Unregister"
