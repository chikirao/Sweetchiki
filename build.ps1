$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    dotnet publish src/Sweetch.App/Sweetch.App.csproj -c Release -r win-x64 --self-contained -p:Version=2.0.0 -o publish
    if ($LASTEXITCODE -ne 0) { throw 'Сборка Sweetch завершилась с ошибкой.' }
    & ./scripts/bundle-pandoc.ps1 -Destination ./publish/tools
    Copy-Item LICENSE, THIRD-PARTY-NOTICES.md, README.md publish/
    Write-Output 'Готово: publish/Sweetch.exe'
}
finally { Pop-Location }
