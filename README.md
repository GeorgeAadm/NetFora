# NetFora
ASP.NET Core Web Forum API

## Introduction

NetFora is a modern forum API built with ASP.NET Core 8 that provides a robust platform for community discussions and content sharing. The platform enables users to create accounts, publish posts, engage through comments, and express appreciation via likes. With built-in moderation capabilities, NetFora ensures content quality while fostering meaningful community interactions.

### Features
- **User Management** - Secure Registration and JWT-based Authentication
- **Content Creation** - Post and Comment system with full CRUD operations
- **Engagement** - Like/unlike functionality to promote quality content
- **Moderation** - Flag-based content moderation with configurable policies
- **Real-time Stats** - Denormalized counters for likes and comments with eventual consistency
- **Scalable Design** - Event-driven architecture supporting horizontal scaling

This project follows Clean Architecture principles with Domain-Driven Design (DDD) patterns, ensuring maintainability, testability, and separation of concerns.

### Project Structure
```
src/
├── NetFora.Api/           # Web API layer (Controllers, Authentication, Configuration)
├── NetFora.Application/   # Application layer (Services, DTOs, Interfaces)
├── NetFora.Domain/        # Domain layer (Entities, Events, Business Logic)
├── NetFora.Infrastructure/# Infrastructure layer (Data Access, External Services)
├── NetFora.EventProcessor/# Background service for event processing
└── NetFora.Tests/         # Unit and integration tests
```

### Architectural Patterns

#### **Clean Architecture Layers**
- **Domain Layer**: Contains business entities, domain events, and core business rules
- **Application Layer**: Orchestrates business workflows, handles DTOs and cross-cutting concerns
- **Infrastructure Layer**: Implements data persistence, external API calls, and technical concerns
- **Presentation Layer**: Exposes HTTP endpoints and handles request/response formatting

#### **Event-Driven Architecture**
- **Event Sourcing**: User actions (likes, comments) generate events stored in database
- **Background Processing**: Dedicated event processor updates denormalized stats
- **Eventual Consistency**: Stats may be slightly delayed but guaranteed to converge

#### **Repository Pattern**
- Abstracts data access logic from business logic
- Enables easy unit testing through dependency injection
- Supports multiple data sources and caching strategies

#### **CQRS (Command Query Responsibility Segregation)**
- Separate read and write operations for optimal performance
- Query parameters support complex filtering and sorting
- Stats tables provide optimized read paths for high-traffic scenarios

### Technical Stack
- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer tokens with ASP.NET Core Identity
- **Logging**: Built-in ILogger with structured logging support
- **Testing**: xUnit with Moq for unit tests
- **Containerization**: Docker with docker-compose for development (See Environment Setup section)

### Design Decisions

#### **Denormalized Statistics**
Posts maintain separate `PostStats` entities with like and comment counts to avoid expensive aggregate queries during high-traffic scenarios.

#### **Event-Driven Stats Updates**
Like and comment operations publish events that are processed asynchronously, ensuring UI responsiveness while maintaining data consistency.

#### **Moderation Flags**
Uses bitwise flags for flexible content moderation policies, allowing multiple moderation states per content item.

#### **Rate Limiting**
Implements sliding window rate limiting for authentication endpoints and token bucket limiting for general API usage.

## Future Enhancements

### Planned Improvements
- **Message Queues**: Replace database events with Redis/RabbitMQ for better scalability
- **Caching Layer**: Add Redis caching for frequently accessed data (posts, user profiles)
- **Enhanced Validation**: Implement FluentValidation for complex business rule validation
- **Error Handling**: Global exception handling with structured error responses
- **Audit Logging**: Track all user actions for compliance and debugging
- **Search**: Full-text search capabilities using Elasticsearch or SQL Server FTS
- **File Uploads**: Support for images and attachments in posts
- **Real-time Notifications**: SignalR integration for live updates

### Scalability Considerations
- **Database Sharding**: Partition data by user or community for horizontal scaling
- **CQRS with Separate Databases**: Split read/write databases for optimal performance
- **CDN Integration**: Static content delivery for global performance
- **Microservices**: Split into bounded contexts (Users, Content, Moderation)

## Environment Setup

### Prerequisites
- **.NET 8 SDK** - Download from [Microsoft .NET](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop** - For containerized development environment
- **SQL Server** - Local instance or use Docker container
- **Git** - For source control


### PowerShell 
**Convenience** Included is a script for setting up a local environment, including certificate management, database initialization, and service orchestration.
```powershell
# Basic Setup
.\setup-dev-environment.ps1

# Clean start
.\setup-dev-environment.ps1 -ResetDatabase

# Rebuild Images
.\setup-dev-environment.ps1 -RebuildImages

# Full Reset
.\setup-dev-environment.ps1 -RebuildImages -ResetDatabase
```

#### Docker Compose
```bash
# Build and start all services
docker-compose up --build

# Run in detached mode
docker-compose up -d --build

# View logs
docker-compose logs -f netfora-api

# Stop services
docker-compose down

# Stop and remove volumes (clears database)
docker-compose down -v
```

#### Manual Setup
If you prefer running without Docker:
```powershell
# Update connection string in appsettings.Development.json
# Set up local SQL Server instance

# Run database migrations
dotnet ef database update --project src/NetFora.Infrastructure --startup-project src/NetFora.Api

# Start the API
dotnet run --project src/NetFora.Api

# Start the Event Processor (in separate terminal)
dotnet run --project src/NetFora.EventProcessor
```

#### SSL Certificate Setup (First-time only)
**For Windows (PowerShell):**
```powershell
# Check for certificates
dotnet dev-certs https --check --trust

# Generate development HTTPS certificate
dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\aspnetapp.pfx -p password

# Trust the certificate
dotnet dev-certs https --trust
```

**For Linux/macOS:**
```bash
# Generate development HTTPS certificate
dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p password

# Trust the certificate
dotnet dev-certs https --trust
```

**Note:** The PowerShell command uses `$env:USERPROFILE` instead of `${HOME}` which is the Windows equivalent for the user's home directory path.


### Service Endpoints
- **API**: https://localhost:5001 (HTTPS) / http://localhost:5000 (HTTP)
- **SQL Server**: localhost:1433 (when using Docker)
- **Swagger UI**: https://localhost:5001/swagger

### Default Credentials
- **Database**: SA password is `YourStrongPassword123!` (configured in docker-compose.yml)

## API Testing Guide

### Authentication Setup

#### 1. User Registration
```http
POST https://localhost:5001/api/auth/register
Content-Type: application/json

{
    "email": "testuser@example.com",
    "password": "TestPassword123!",
    "displayName": "Test User",
    "userName": "testuser"
}
```

**Expected Response:**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "testuser@example.com",
    "displayName": "Test User",
    "expiresAt": "2024-01-02T10:30:00Z"
}
```

#### 2. User Login
```http
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
    "email": "testuser@example.com",
    "password": "TestPassword123!"
}
```

#### 3. Setup Authorization
Copy the `token` from the response and add to all subsequent requests:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Core API Workflows

#### Creating and Managing Posts

**Create a Post:**
```http
POST https://localhost:5001/api/posts
Authorization: Bearer {your-token}
Content-Type: application/json

{
    "title": "My First Post",
    "content": "This is the content of my first post on NetFora!"
}
```

**Get All Posts:**
```http
GET https://localhost:5001/api/posts?page=1&pageSize=10&sortBy=CreatedDate&sortDirection=Descending
```

**Get Specific Post:**
```http
GET https://localhost:5001/api/posts/1
```

**Search Posts:**
```http
GET https://localhost:5001/api/posts?searchTerm=NetFora&minLikes=5&hasComments=true
```

#### Comments and Engagement

**Add Comment:**
```http
POST https://localhost:5001/api/posts/1/comments
Authorization: Bearer {your-token}
Content-Type: application/json

{
    "postId": 1,
    "content": "Great post! Thanks for sharing."
}
```

**Get Comments:**
```http
GET https://localhost:5001/api/posts/1/comments?page=1&pageSize=20&sortBy=CreatedDate
```

**Like a Post:**
```http
POST https://localhost:5001/api/posts/1/likes
Authorization: Bearer {your-token}
```

**Unlike a Post:**
```http
DELETE https://localhost:5001/api/posts/1/likes
Authorization: Bearer {your-token}
```

**Get Like Count:**
```http
GET https://localhost:5001/api/posts/1/likes/count
```

### Testing Scenarios

#### Scenario 1: Complete User Journey
1. Register new user account
2. Login and obtain JWT token
3. Create a new post
4. Add comments to the post
5. Like the post
6. Search for posts by keyword
7. View post with comments and stats

#### Scenario 2: Multiple Users Interaction
1. Register User A and User B
2. User A creates a post
3. User B comments on the post
4. User B likes the post
5. Verify stats are updated correctly
6. Test that User A cannot like their own post

#### Scenario 3: Error Handling
1. Test invalid login credentials
2. Test posting without authentication
3. Test liking non-existent post
4. Test rate limiting by making rapid requests

### Rate Limiting Notes
- **Authentication endpoints**: 5 requests per 15 minutes per IP
- **Registration endpoint**: 3 requests per hour per IP
- **General endpoints**: No limit in development (commented out)

### Troubleshooting

#### Common Issues

**SSL Certificate Issues:**

**For Linux/macOS:**
```bash
# Clear and regenerate certificates
dotnet dev-certs https --clean
dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p password
dotnet dev-certs https --trust
```

**For Windows (PowerShell):**
```powershell
# Clear and regenerate certificates
dotnet dev-certs https --clean
dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\aspnetapp.pfx -p password
dotnet dev-certs https --trust
```

**Database Connection Issues:**
```bash
# Check if SQL Server container is running
docker ps

# Check SQL Server logs
docker logs netfora-sqlserver

# Reset database
docker-compose down -v
docker-compose up -d sqlserver
```

**Event Processing Issues:**
```bash
# Check event processor logs
docker logs netfora-processor

# Verify events are being created
# Check LikeEvents and CommentEvents tables in database
```

**API Not Responding:**
```bash
# Check API logs
docker logs netfora-api

# Verify all required environment variables are set
# Check appsettings.json configuration
```

### Development Tools
- **Postman Collection**: Import the provided collection for complete API testing
- **Swagger UI**: Available at https://localhost:5001/swagger for interactive API exploration
- **Database Browser**: Use SQL Server Management Studio or Azure Data Studio to inspect data
- **Logs**: Check Docker logs or application logs in the console for debugging

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test src/NetFora.Tests/
```

## Testing

A comprehensive Postman test suite that includes scenarios for:
- Multi-user interaction
- Business rule validation
- Volume/performance testing
- Moderation workflows
- Complex query testing

**Note** See the NetFora-Postman-Test-Guide for more detais and Testing Instructions.