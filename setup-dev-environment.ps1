# NetFora Development Environment Setup Script (Simplified)
# Prerequisites: Docker Desktop must be installed and running

param(
    [string]$ProjectPath = (Get-Location).Path,
    [switch]$RebuildImages,
    [switch]$ResetDatabase,
    [switch]$SkipCertificate
)

# Configuration
$DockerComposeFile = Join-Path $ProjectPath "docker-compose.yml"
$SqlScriptPath = Join-Path $ProjectPath "InitialCreate-Fixed.sql"
if (-not (Test-Path $SqlScriptPath)) {
    $SqlScriptPath = Join-Path $ProjectPath "InitialCreate.sql"
}

Write-Host "NetFora Development Environment Setup" -ForegroundColor Yellow
Write-Host "======================================" -ForegroundColor Yellow

# Check if Docker is running
Write-Host "`nChecking Docker status..." -ForegroundColor Cyan
try {
    $null = docker version 2>&1
    Write-Host "[OK] Docker is running" -ForegroundColor Green
}
catch {
    Write-Host "Docker is not running. Please start Docker Desktop and try again." -ForegroundColor Red
    exit 1
}

# Check if docker-compose file exists
if (-not (Test-Path $DockerComposeFile)) {
    Write-Host "docker-compose.yml not found at $DockerComposeFile" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] docker-compose.yml found" -ForegroundColor Green

# Setup HTTPS certificate for development
if (-not $SkipCertificate) {
    Write-Host "`nSetting up HTTPS development certificate..." -ForegroundColor Cyan
    
    $certPath = "$env:USERPROFILE\.aspnet\https"
    if (-not (Test-Path $certPath)) {
        New-Item -ItemType Directory -Path $certPath -Force | Out-Null
    }
    
    $certFile = Join-Path $certPath "aspnetapp.pfx"
    if (-not (Test-Path $certFile) -or $RebuildImages) {
        try {
            $null = dotnet dev-certs https --clean 2>&1
            $null = dotnet dev-certs https -ep $certFile -p password 2>&1
            $null = dotnet dev-certs https --trust 2>&1
            
            Write-Host "[OK] HTTPS certificate created and trusted" -ForegroundColor Green
        }
        catch {
            Write-Host "[WARNING] Could not create HTTPS certificate. Continuing without HTTPS..." -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "[OK] HTTPS certificate already exists" -ForegroundColor Green
    }
}

# Stop existing containers
Write-Host "`nStopping existing containers..." -ForegroundColor Cyan
Start-Process -FilePath "docker-compose" -ArgumentList "-f", $DockerComposeFile, "down" -Wait -NoNewWindow
Write-Host "[OK] Existing containers stopped" -ForegroundColor Green

# Remove volumes if reset requested
if ($ResetDatabase) {
    Write-Host "`nResetting database volumes..." -ForegroundColor Cyan
    Start-Process -FilePath "docker-compose" -ArgumentList "-f", $DockerComposeFile, "down", "-v" -Wait -NoNewWindow
    Write-Host "[OK] Database volumes removed" -ForegroundColor Green
}

# Build images
Write-Host "`nBuilding Docker images..." -ForegroundColor Cyan
if ($RebuildImages) {
    Start-Process -FilePath "docker-compose" -ArgumentList "-f", $DockerComposeFile, "build", "--no-cache" -Wait -NoNewWindow
}
else {
    Start-Process -FilePath "docker-compose" -ArgumentList "-f", $DockerComposeFile, "build" -Wait -NoNewWindow
}
Write-Host "[OK] Docker images built" -ForegroundColor Green

# Start services
Write-Host "`nStarting services..." -ForegroundColor Cyan
Start-Process -FilePath "docker-compose" -ArgumentList "-f", $DockerComposeFile, "up", "-d" -Wait -NoNewWindow

# Give SQL Server time to start up
Write-Host "`nWaiting for SQL Server to initialize (60 seconds)..." -ForegroundColor Cyan
Start-Sleep -Seconds 60

# Wait for SQL Server to be ready
Write-Host "`nChecking SQL Server readiness..." -ForegroundColor Cyan
$maxAttempts = 30
$attempt = 0
$ready = $false

while ($attempt -lt $maxAttempts -and -not $ready) {
    $attempt++
    Write-Host "  Attempt $attempt of $maxAttempts..." -NoNewline
    
    Start-Sleep -Seconds 2
    
    # Use a simpler approach - just try to execute a command
    $testCmd = @"
docker exec netfora-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrongPassword123!" -C -Q "SELECT 1" -b
"@
    
    try {
        $output = Invoke-Expression $testCmd 2>&1
        if ($LASTEXITCODE -eq 0) {
            $ready = $true
            Write-Host " Connected!" -ForegroundColor Green
        }
        else {
            Write-Host " Not ready" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host " Not ready" -ForegroundColor Yellow
    }
}

if (-not $ready) {
    Write-Host "SQL Server failed to start within timeout period" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] SQL Server is ready" -ForegroundColor Green

# Create database
Write-Host "`nCreating database..." -ForegroundColor Cyan
$createDbCmd = @"
docker exec netfora-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrongPassword123!" -C -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'NetForaDb') CREATE DATABASE NetForaDb"
"@
Invoke-Expression $createDbCmd
Write-Host "[OK] Database created (or already exists)" -ForegroundColor Green

# Run initialization script
if (Test-Path $SqlScriptPath) {
    Write-Host "`nRunning database initialization script..." -ForegroundColor Cyan
    
    # Copy SQL script to container
    docker cp $SqlScriptPath netfora-sqlserver:/tmp/InitialCreate.sql
    
    # Execute the script
    $runSqlCmd = @"
docker exec netfora-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrongPassword123!" -C -d NetForaDb -i /tmp/InitialCreate.sql -b
"@
    
    try {
        Invoke-Expression $runSqlCmd
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] Database initialization completed" -ForegroundColor Green
        }
        else {
            Write-Host "[WARNING] Database initialization may have failed (tables might already exist)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "[WARNING] Database initialization may have failed (tables might already exist)" -ForegroundColor Yellow
    }
}
else {
    Write-Host "[WARNING] InitialCreate.sql not found at $SqlScriptPath" -ForegroundColor Yellow
}

# Display service status
Write-Host "`nService Status:" -ForegroundColor Cyan
Start-Process -FilePath "docker-compose" -ArgumentList "-f", $DockerComposeFile, "ps" -Wait -NoNewWindow

# Display connection information
Write-Host "`nConnection Information:" -ForegroundColor Cyan
Write-Host "  API URL: https://localhost:5001 (HTTPS) or http://localhost:5000 (HTTP)" -ForegroundColor Green
Write-Host "  SQL Server: localhost,1433" -ForegroundColor Green
Write-Host "  Database: NetForaDb" -ForegroundColor Green
Write-Host "  SQL User: sa" -ForegroundColor Green
Write-Host "  SQL Password: YourStrongPassword123!" -ForegroundColor Green

# Display helpful commands
Write-Host "`nUseful Commands:" -ForegroundColor Cyan
Write-Host "  View logs: docker-compose logs -f [service-name]" -ForegroundColor Yellow
Write-Host "  Stop all: docker-compose down" -ForegroundColor Yellow
Write-Host "  Reset database: .\setup-dev-environment.ps1 -ResetDatabase" -ForegroundColor Yellow
Write-Host "  Rebuild images: .\setup-dev-environment.ps1 -RebuildImages" -ForegroundColor Yellow

Write-Host "`n[OK] Development environment setup complete!" -ForegroundColor Green