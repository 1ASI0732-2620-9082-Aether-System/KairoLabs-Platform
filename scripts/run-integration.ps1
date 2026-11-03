param([ValidateSet('spec','tap')][string]$Reporter = 'spec', [ValidateSet('integration','bdd')][string]$Suite='integration', [string]$FrontendRoot='')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
Set-Location -LiteralPath $projectRoot
$runtimeRoot = Join-Path $projectRoot '.runtime'
New-Item -ItemType Directory -Force $runtimeRoot,(Join-Path $projectRoot 'evidence') | Out-Null
$workspaceTools = [IO.Path]::GetFullPath((Join-Path $projectRoot '../../.tooling'))
$dotnet = Join-Path $runtimeRoot 'dotnet/dotnet.exe'
if (!(Test-Path -LiteralPath $dotnet)) { $dotnet = Join-Path $workspaceTools 'dotnet/dotnet.exe' }
if (!(Test-Path -LiteralPath $dotnet)) {
    $dotnet = (Get-Command dotnet -ErrorAction Stop).Source
    if (!((& $dotnet --list-sdks) -match '^10\.')) { throw 'Se requiere .NET 10. Ejecutar la tarea de preparar herramientas.' }
}
$mysqlServer = Get-ChildItem -LiteralPath (Join-Path $runtimeRoot 'mysql') -Filter mysqld.exe -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
if (!$mysqlServer) { $mysqlServer = Get-ChildItem -LiteralPath (Join-Path $workspaceTools 'mysql') -Filter mysqld.exe -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName }
if (!$mysqlServer) { throw 'No se encuentra MySQL portatil. Ejecutar la tarea de preparar herramientas.' }
$mysqlClient = Join-Path (Split-Path $mysqlServer -Parent) 'mysql.exe'
$mysqlBase = Split-Path (Split-Path $mysqlServer -Parent) -Parent
$databaseName = 'kairolabs_it_' + [Guid]::NewGuid().ToString('N').Substring(0,12)
if ($databaseName -notmatch '^kairolabs_it_[a-f0-9]{12}$') { throw 'Nombre de base no permitido' }
$dataRoot = Join-Path $runtimeRoot 'mysql-data'
$apiProject = Join-Path $projectRoot 'backend/TechnoByteLambders.MediTrackSensor.Platform'
$mysqlProcess = $null
$apiProcess = $null
$createdDatabase = $false
$testExit = 1
$envKeys = @('DATABASE_URL','DOTNET_ROOT','DOTNET_CLI_HOME','DOTNET_CLI_TELEMETRY_OPTOUT','DOTNET_SKIP_FIRST_TIME_EXPERIENCE','DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE','NUGET_PACKAGES','ASPNETCORE_URLS','ASPNETCORE_ENVIRONMENT','Jwt__Secret','Jwt__Issuer','Jwt__Audience','MYSQL_PWD','KAIROLABS_IT_URL','KAIROLABS_IT_DATABASE','KAIROLABS_MYSQL_CLIENT')
$saved = @{}
foreach ($key in $envKeys) { $saved[$key] = [Environment]::GetEnvironmentVariable($key,'Process') }
function Invoke-TestSql([string]$sql) {
    & $mysqlClient --no-defaults --protocol=TCP --host=127.0.0.1 --port=33079 --user=root --batch --skip-column-names -e $sql
    if ($LASTEXITCODE -ne 0) { throw 'Fallo del cliente MySQL local' }
}
function Check-FreePort([int]$port) {
    $listener = New-Object Net.Sockets.TcpListener([Net.IPAddress]::Loopback, $port)
    try { $listener.Start() } finally { $listener.Stop() }
}
try {
    Check-FreePort 33079
    Check-FreePort 5187
    $env:DOTNET_ROOT = Split-Path $dotnet -Parent
    $env:DOTNET_CLI_HOME = Join-Path $runtimeRoot 'dotnet-home'
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
    $env:DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE = 'true'
    $env:NUGET_PACKAGES = if (Test-Path -LiteralPath $workspaceTools) { Join-Path $workspaceTools 'nuget' } else { Join-Path $runtimeRoot 'nuget' }
    $env:MYSQL_PWD = ''
    Write-Host '[1/5] Compilando backend ASP.NET Core 10...'
    & $dotnet build (Join-Path $apiProject 'TechnoByteLambders.MediTrackSensor.Platform.csproj') -c Release --nologo -v quiet *> (Join-Path $runtimeRoot 'build.log')
    [IO.File]::WriteAllText((Join-Path $projectRoot 'evidence/build-warnings.txt'), (Get-Content -LiteralPath (Join-Path $runtimeRoot 'build.log') -Raw), (New-Object Text.UTF8Encoding($false)))
    if ($LASTEXITCODE -ne 0) { throw "No compila el backend. Revisar .runtime/build.log" }
    if (!(Test-Path -LiteralPath (Join-Path $projectRoot 'frontend/node_modules/vite/bin/vite.js'))) {
        Push-Location (Join-Path $projectRoot 'frontend')
        try { & npm.cmd ci --ignore-scripts --no-audit --no-fund; if ($LASTEXITCODE -ne 0) { throw 'Fallo npm ci' } } finally { Pop-Location }
    }
    Push-Location (Join-Path $projectRoot 'frontend')
    try {
        & node node_modules/vite/bin/vite.js build --config vite.integration.config.js *> (Join-Path $runtimeRoot 'frontend-build.log')
        if ($LASTEXITCODE -ne 0) { throw 'No se compilan los gateways. Revisar .runtime/frontend-build.log' }
    } finally { Pop-Location }
    Write-Host '[2/5] Iniciando MySQL aislado en 127.0.0.1:33079...'
    if (!(Test-Path -LiteralPath (Join-Path $dataRoot 'mysql'))) {
        try {
            $ErrorActionPreference = 'Continue'
            & $mysqlServer --no-defaults --initialize-insecure "--basedir=$mysqlBase" "--datadir=$dataRoot" --console *> (Join-Path $runtimeRoot 'mysql-initialize.log')
        } finally { $ErrorActionPreference = 'Stop' }
        if ($LASTEXITCODE -ne 0) { throw 'Fallo al inicializar MySQL. Revisar .runtime/mysql-initialize.log' }
    }
    $mysqlProcess = Start-Process -FilePath $mysqlServer -ArgumentList @('--no-defaults',"--basedir=`"$mysqlBase`"","--datadir=`"$dataRoot`"",'--port=33079','--bind-address=127.0.0.1','--mysqlx=0','--skip-log-bin','--console') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtimeRoot 'mysql.stdout.log') -RedirectStandardError (Join-Path $runtimeRoot 'mysql.stderr.log')
    $ready = $false
    for ($i=0; $i -lt 80; $i++) {
        try {
            # PowerShell 5 interpreta stderr nativo como error terminante con Stop.
            # Durante la espera se comprueba el codigo de salida y se reintenta.
            $ErrorActionPreference = 'Continue'
            & $mysqlClient --no-defaults --protocol=TCP --host=127.0.0.1 --port=33079 --user=root -e 'SELECT 1' *> $null
        } finally { $ErrorActionPreference = 'Stop' }
        if ($LASTEXITCODE -eq 0) { $ready=$true; break }
        if ($mysqlProcess.HasExited) { throw 'MySQL termino al iniciar. Revisar .runtime/mysql.stderr.log' }
        Start-Sleep -Milliseconds 250
    }
    if (!$ready) { throw 'MySQL no responde' }
    Invoke-TestSql "CREATE DATABASE $databaseName CHARACTER SET utf8mb4; CREATE USER IF NOT EXISTS 'kairolabs_it'@'localhost' IDENTIFIED BY 'Local_Test_Only_2026!'; GRANT ALL ON $databaseName.* TO 'kairolabs_it'@'localhost';"
    $createdDatabase = $true
    $env:DATABASE_URL = "server=127.0.0.1;port=33079;database=$databaseName;user=kairolabs_it;password=Local_Test_Only_2026!;SslMode=Preferred"
    $env:ASPNETCORE_URLS = 'http://127.0.0.1:5187'
    $env:ASPNETCORE_ENVIRONMENT = 'Integration'
    $env:Jwt__Secret = 'KairoLabs_Integration_Only_2026_Key_Minimum_32_Characters!'
    $env:Jwt__Issuer = 'KairoLabsIntegration'
    $env:Jwt__Audience = 'KairoLabsIntegrationTests'
    Write-Host '[3/5] Iniciando API local y aplicando migraciones...'
    $apiDll = Join-Path $apiProject 'bin/Release/net10.0/TechnoByteLambders.MediTrackSensor.Platform.dll'
    $apiProcess = Start-Process -FilePath $dotnet -ArgumentList @("`"$apiDll`"") -WorkingDirectory $apiProject -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtimeRoot 'api.stdout.log') -RedirectStandardError (Join-Path $runtimeRoot 'api.stderr.log')
    $apiReady = $false
    for ($i=0; $i -lt 120; $i++) {
        try {
            $health = Invoke-RestMethod 'http://127.0.0.1:5187/health/db' -TimeoutSec 1
            if ($health.database -eq 'connected') { $apiReady=$true; break }
        } catch { }
        if ($apiProcess.HasExited) { throw 'La API termino al iniciar. Revisar logs .runtime/api.*.log' }
        Start-Sleep -Milliseconds 500
    }
    if (!$apiReady) { throw 'La API o su esquema no estan listos. Revisar .runtime/api.stdout.log' }
    Write-Host ('[4/5] Ejecutando suite ' + $Suite + ' con API y MySQL reales...')
    $env:KAIROLABS_IT_URL = 'http://127.0.0.1:5187'
    $env:KAIROLABS_IT_DATABASE = $databaseName
    $env:KAIROLABS_MYSQL_CLIENT = $mysqlClient
    if ($Suite -eq 'bdd') {
        if (!$FrontendRoot -or !(Test-Path -LiteralPath (Join-Path $FrontendRoot 'bdd/run.mjs'))) { throw 'FrontendRoot no contiene la suite BDD.' }
        Push-Location -LiteralPath $FrontendRoot
        try { & node bdd/run.mjs; $testExit=$LASTEXITCODE } finally { Pop-Location }
    } else {
        & node --test "--test-reporter=$Reporter" --test-reporter=tap --test-reporter-destination=stdout --test-reporter-destination=evidence/latest.tap tests/integration.test.mjs
    }
    if ($Suite -ne 'bdd') { $testExit = $LASTEXITCODE }
} finally {
    if (Test-Path -LiteralPath (Join-Path $runtimeRoot 'api.stdout.log')) { Copy-Item -LiteralPath (Join-Path $runtimeRoot 'api.stdout.log') -Destination (Join-Path $projectRoot 'evidence/startup-diagnostics.txt') -Force }
    Write-Host '[5/5] Cerrando procesos y eliminando exclusivamente la base temporal...'
    if ($apiProcess -and !$apiProcess.HasExited) { Stop-Process -Id $apiProcess.Id -Force }
    if ($createdDatabase -and $mysqlProcess -and !$mysqlProcess.HasExited) {
        Invoke-TestSql "DROP DATABASE $databaseName;" | Out-Null
    }
    if ($mysqlProcess -and !$mysqlProcess.HasExited) {
        $adminExe = Join-Path (Split-Path $mysqlClient -Parent) 'mysqladmin.exe'
        & $adminExe --no-defaults --protocol=TCP --host=127.0.0.1 --port=33079 --user=root shutdown *> $null
        if (!$mysqlProcess.WaitForExit(5000)) { Stop-Process -Id $mysqlProcess.Id -Force }
    }
    foreach ($key in $envKeys) { [Environment]::SetEnvironmentVariable($key,$saved[$key],'Process') }
}
exit $testExit
