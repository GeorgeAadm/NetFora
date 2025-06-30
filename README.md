# NetFora
ASP.Net Web Forum API 

## Environment Setup


### PowerShell 
Included is a script for setting up the your environment, including certificate management, database initialization, and service orchestration.
```PowerShell
# Basic Setup
.\setup-dev-environment.ps1

# Clean start
.\setup-dev-environment.ps1 -ResetDatabase

# Rebuild Images
.\setup-dev-environment.ps1 -RebuildImages

# Full Reset
.\setup-dev-environment.ps1 -RebuildImages -ResetDatabase
```


### Docker Compose

```bash
# Build and start all services
docker-compose up --build

# Run in detached mode
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes (clears database)
docker-compose down -v
```


## Troubleshooting HTTPS
If you get SSL errors when running the containers, ensure your development certificate is trusted.
```bash
dotnet dev-certs https --check --trust

# Generate HTTPS certificate for development: first-time setup (required once per machine)
dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p password

# If not trusted, run:
dotnet dev-certs https --trust
```

## Testing

A comprehensive Postman test suite that includes scenarios for:
- Multi-user interaction
- Business rule validation
- Volume/performance testing
- Moderation workflows
- Complex query testing
