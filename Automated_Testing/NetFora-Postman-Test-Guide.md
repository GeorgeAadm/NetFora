# NetFora - Postman Testing Guide

## Setup Instructions

### 1. Import Collections and Environment

1. Import all three files into Postman:
   - `NetFora-API.postman_collection.json` (Basic collection)
   - `NetFora-Advanced-Tests.postman_collection.json` (Advanced tests)
   - `NetFora-Local.postman_environment.json` (Environment)

2. Select "NetFora Local" as your active environment

### 2. Database Setup for Moderator Role

After running the "Create Moderator User" request, execute this SQL to grant moderator privileges:

```sql
-- Find the moderator user and role IDs
DECLARE @modUserId NVARCHAR(450);
DECLARE @modRoleId NVARCHAR(450);

-- Replace 'moderator_XXXXX' with the actual username from the test
SELECT @modUserId = Id FROM AspNetUsers WHERE UserName = 'moderator_XXXXX';
SELECT @modRoleId = Id FROM AspNetRoles WHERE Name = 'Moderator';

-- Add user to moderator role
IF @modUserId IS NOT NULL AND @modRoleId IS NOT NULL
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@modUserId, @modRoleId);
    PRINT 'Moderator role assigned successfully';
END
ELSE
BEGIN
    PRINT 'User or role not found';
END
```

## Running Tests

### Individual Test Execution

1. **Setup Users First**: Run all requests in "Setup Test Users" folder
2. **Run Scenarios**: Execute test scenarios in order within each folder

### Collection Runner

For automated testing:

1. Open Collection Runner (Runner tab in Postman)

2. Select "NetFora API - Advanced Test Scenarios"

3. Configure:
   - Environment: NetFora Local
   - Iterations: 1 ( more for volume tests )
   - Delay: 500ms ( allows event processing )

4. Run specific folders:
   - Setup Test Users (NB: run first)
   - Multi-User Interaction Tests
   - Business Rules Tests
   - Volume Testing ( set iterations to 10 initially )
   - Moderation Tests
   - Query and Filter Tests

### Volume Testing Configuration

For the "Create 10 Posts" test:
- Set iterations to 10 in Collection Runner
- This will create 10 unique posts
- Monitor response times and database performance

For "Add Multiple Comments":
- Set iterations to 20 - 50
- Tests rotation between 3 users
- Validates event processing under load

## Testing Scenarios

### 1. Multi-User Interactions
- Creates posts and tests cross-user likes/comments
- Validates like counts update correctly
- Ensures users can't like their own posts
- Tests comment threading

### 2. Business Rules
- Maximum title length (200 chars)
- Maximum comment length (2000 chars)
- Duplicate like prevention
- Invalid post/comment references

### 3. Volume Testing
- Bulk post creation
- Pagination validation
- Large result set handling
- Concurrent request handling

### 4. Moderation Workflow
- Flag posts/comments
- Role-based access control
- Filtered queries for flagged content

### 5. Query Tests
- Search functionality
- Author filtering
- Date range queries
- Sorting options
- Complex multi-parameter queries

## Performance Benchmarks

Expected response times:
- GET /posts: < 500ms
- GET /posts/{id}: < 200ms
- POST /posts: < 300ms
- POST /comments: < 300ms
- POST /likes: < 200ms

## Monitoring Event Processing

The Event Processor handles likes/comments asynchronously. To monitor:

```bash
# Watch Event Processor logs
docker logs -f netfora-processor

# Check event queues in database
SELECT COUNT(*) FROM LikeEvents WHERE Processed = 0;
SELECT COUNT(*) FROM CommentEvents WHERE Processed = 0;
```

## Troubleshooting

### Common Issues

1. **"Cannot like own post"**: Working as designed
2. **Like count not updating**: Wait 2-3 seconds for event processing
3. **403 on moderation**: User needs Moderator role (see SQL above)
4. **Duplicate key errors**: Database constraint working correctly

### Debug SQL Queries

```sql
-- Check user roles
SELECT u.UserName, r.Name as Role
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id;

-- Check post statistics
SELECT p.Id, p.Title, ps.LikeCount, ps.CommentCount, ps.LastUpdated
FROM Posts p
LEFT JOIN PostStats ps ON p.Id = ps.PostId
ORDER BY p.CreatedAt DESC;

-- Monitor event processing
SELECT TOP 10 * FROM LikeEvents ORDER BY CreatedAt DESC;
SELECT TOP 10 * FROM CommentEvents ORDER BY CreatedAt DESC;
```

## Best Practices

1. **Reset Environment**: Clear environment variables between test runs
2. **Wait for Processing**: Add delays after likes/comments for event processing
3. **Check Logs**: Monitor both API and Event Processor logs
4. **Database Cleanup**: Periodically clean test data:

```sql
-- Clean test data (be careful in production!)
DELETE FROM Comments WHERE Content LIKE '%test%';
DELETE FROM Posts WHERE Title LIKE '%Test%' OR Title LIKE '%Volume%';
DELETE FROM AspNetUsers WHERE Email LIKE '%test.com';
```