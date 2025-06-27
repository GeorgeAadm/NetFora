# NetFora
ASP.Net Web Forum API 

## Development Setup

### Docker Compose:

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
If you get SSL errors when running the containers, ensure your development certificate is trusted:
```bash
dotnet dev-certs https --check --trust

# Generate HTTPS certificate for development: first-time setup (required once per machine)
dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p password

# If not trusted, run:
dotnet dev-certs https --trust
```