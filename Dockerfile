# Single Dockerfile that builds either API or EventProcessor
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG PROJECT_NAME=NetFora.Api
WORKDIR /src

# Copy all project files
COPY ["src/NetFora.Api/NetFora.Api.csproj", "src/NetFora.Api/"]
COPY ["src/NetFora.EventProcessor/NetFora.EventProcessor.csproj", "src/NetFora.EventProcessor/"]
COPY ["src/NetFora.Application/NetFora.Application.csproj", "src/NetFora.Application/"]
COPY ["src/NetFora.Domain/NetFora.Domain.csproj", "src/NetFora.Domain/"]
COPY ["src/NetFora.Infrastructure/NetFora.Infrastructure.csproj", "src/NetFora.Infrastructure/"]
COPY ["NetFora.sln", "./"]

# Restore dependencies for the target project
RUN dotnet restore "src/${PROJECT_NAME}/${PROJECT_NAME}.csproj"

# Copy all source code
COPY . .

# Build the target project
WORKDIR "/src/src/${PROJECT_NAME}"
RUN dotnet build "${PROJECT_NAME}.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG PROJECT_NAME=NetFora.Api
RUN dotnet publish "${PROJECT_NAME}.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ARG PROJECT_NAME=NetFora.Api
WORKDIR /app
COPY --from=publish /app/publish .

# Create a script to run the appropriate dll
RUN echo '#!/bin/sh' > /app/entrypoint.sh && \
    echo 'exec dotnet '$PROJECT_NAME'.dll' >> /app/entrypoint.sh && \
    chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]