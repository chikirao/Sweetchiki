$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    # Use a .NET 10 SDK even when an older one comes first on PATH: try the usual install places.
    $candidates = @(
        (Get-Command dotnet -ErrorAction SilentlyContinue).Source,
        "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe",
        "$env:LOCALAPPDATA\Microsoft\dotnet10\dotnet.exe",
        "$env:ProgramFiles\dotnet\dotnet.exe"
    ) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique
    $dotnet = $candidates | Where-Object { (& $_ --list-sdks 2>$null) -match '^10\.' } | Select-Object -First 1
    if (-not $dotnet) { throw 'Нужен .NET SDK 10: https://dotnet.microsoft.com/download/dotnet/10.0' }
    $env:DOTNET_ROOT = Split-Path $dotnet

    & $dotnet publish src/Sweetch.App/Sweetch.App.csproj -c Release -r win-x64 --self-contained -p:Version=2.0.0 -o publish
    if ($LASTEXITCODE -ne 0) { throw 'Сборка Sweetch завершилась с ошибкой.' }
    & ./scripts/bundle-pandoc.ps1 -Destination ./publish/tools
    Copy-Item LICENSE, THIRD-PARTY-NOTICES.md, README.md publish/
    Write-Output 'Готово: publish/Sweetch.exe'
}
finally { Pop-Location }
