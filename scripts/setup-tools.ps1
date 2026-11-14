$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$projectRoot = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $projectRoot '.runtime'
New-Item -ItemType Directory -Force $runtime | Out-Null
$sdkZip = Join-Path $runtime 'dotnet-sdk.zip'
$mysqlZip = Join-Path $runtime 'mysql.zip'
Write-Host 'Descargando herramientas oficiales dentro de .runtime, sin instalacion global...'
if (!(Test-Path -LiteralPath (Join-Path $runtime 'dotnet/dotnet.exe'))) {
    Write-Host 'Descargando SDK .NET...'; & curl.exe --fail --location --retry 3 --connect-timeout 30 --max-time 900 --output $sdkZip 'https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.403/dotnet-sdk-10.0.403-win-x64.zip'
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo descargar .NET' }
    if ((Get-FileHash -LiteralPath $sdkZip -Algorithm SHA512).Hash.ToLowerInvariant() -ne '7a114c318fa7f1d9822991b534b2814d0e78386ceda816b371f5141acefe741d87ccb606ede06cadac8f9ac5070e32b708a67bbd1619ff5e5a7858a27874ef48') { throw 'Checksum de .NET incorrecto' }
    Write-Host 'Extrayendo SDK .NET...'; New-Item -ItemType Directory -Force (Join-Path $runtime 'dotnet') | Out-Null
    & tar.exe -xf $sdkZip -C (Join-Path $runtime 'dotnet')
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo extraer .NET' }
}
if (!(Test-Path -LiteralPath (Join-Path $runtime 'mysql/mysql-8.4.13-winx64/bin/mysqld.exe'))) {
    Write-Host 'Descargando MySQL...'; & curl.exe --fail --location --retry 3 --connect-timeout 30 --max-time 900 --output $mysqlZip 'https://cdn.mysql.com/Downloads/MySQL-8.4/mysql-8.4.13-winx64.zip'
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo descargar MySQL' }
    if ((Get-FileHash -LiteralPath $mysqlZip -Algorithm MD5).Hash.ToLowerInvariant() -ne '7b94489d5524dea9b27097068bea25a8') { throw 'Checksum de MySQL incorrecto' }
    Write-Host 'Extrayendo MySQL...'; New-Item -ItemType Directory -Force (Join-Path $runtime 'mysql') | Out-Null
    & tar.exe -xf $mysqlZip -C (Join-Path $runtime 'mysql')
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo extraer MySQL' }
}
Write-Host 'Herramientas listas. Ejecutar F5 o npm test.'
