-- NetFora Test Data Setup Script
-- Run this after creating test users via Postman to set up roles and sample data

-- 1. Grant Moderator role to test moderator
-- Replace 'moderator_XXXXX' with actual username from your test run
DECLARE @modUsername NVARCHAR(256) = 'moderator_XXXXX'; -- UPDATE THIS!

DECLARE @modUserId NVARCHAR(450);
DECLARE @modRoleId NVARCHAR(450);

SELECT @modUserId = Id FROM AspNetUsers WHERE UserName = @modUsername;
SELECT @modRoleId = Id FROM AspNetRoles WHERE Name = 'Moderator';

IF @modUserId IS NOT NULL AND @modRoleId IS NOT NULL
BEGIN
    -- Check if already assigned
    IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @modUserId AND RoleId = @modRoleId)
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@modUserId, @modRoleId);
        PRINT 'Moderator role assigned to ' + @modUsername;
    END
    ELSE
    BEGIN
        PRINT 'User already has Moderator role';
    END
END
ELSE
BEGIN
    PRINT 'ERROR: User or Moderator role not found. Check username.';
END

-- 2. Create sample data for testing
DECLARE @sampleUserId NVARCHAR(450);

-- Create a sample user if needed
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 'sampleuser')
BEGIN
    -- Note: This is a simplified insert. In production, use proper Identity creation
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, 
                           EmailConfirmed, DisplayName, CreatedAt, SecurityStamp, 
                           ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, 
                           LockoutEnabled, AccessFailedCount)
    VALUES (NEWID(), 'sampleuser', 'SAMPLEUSER', 'sample@example.com', 'SAMPLE@EXAMPLE.COM',
            1, 'Sample User', GETUTCDATE(), NEWID(), NEWID(), 0, 0, 1, 0);
    
    SELECT @sampleUserId = Id FROM AspNetUsers WHERE UserName = 'sampleuser';
    
    -- Add to User role
    DECLARE @userRoleId NVARCHAR(450);
    SELECT @userRoleId = Id FROM AspNetRoles WHERE Name = 'User';
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@sampleUserId, @userRoleId);
END
ELSE
BEGIN
    SELECT @sampleUserId = Id FROM AspNetUsers WHERE UserName = 'sampleuser';
END

-- 3. Create sample posts with different stats
DECLARE @postId1 INT, @postId2 INT, @postId3 INT;

-- Popular post
INSERT INTO Posts (Title, Content, AuthorId, CreatedAt, ModerationFlags)
VALUES ('Welcome to NetFora!', 'This is a sample post with many likes and comments.', 
        @sampleUserId, DATEADD(day, -7, GETUTCDATE()), 0);
SET @postId1 = SCOPE_IDENTITY();

INSERT INTO PostStats (PostId, LikeCount, CommentCount) 
VALUES (@postId1, 25, 10);

-- Flagged post
INSERT INTO Posts (Title, Content, AuthorId, CreatedAt, ModerationFlags)
VALUES ('Controversial Topic', 'This post has been flagged as misleading.', 
        @sampleUserId, DATEADD(day, -3, GETUTCDATE()), 1);
SET @postId2 = SCOPE_IDENTITY();

INSERT INTO PostStats (PostId, LikeCount, CommentCount) 
VALUES (@postId2, 5, 3);

-- Recent post with no interaction
INSERT INTO Posts (Title, Content, AuthorId, CreatedAt, ModerationFlags)
VALUES ('Just Posted!', 'A brand new post with no likes or comments yet.', 
        @sampleUserId, GETUTCDATE(), 0);
SET @postId3 = SCOPE_IDENTITY();

INSERT INTO PostStats (PostId, LikeCount, CommentCount) 
VALUES (@postId3, 0, 0);

-- 4. Add sample comments
INSERT INTO Comments (Content, PostId, AuthorId, CreatedAt, ModerationFlags)
VALUES 
    ('Great post! Thanks for sharing.', @postId1, @sampleUserId, DATEADD(day, -6, GETUTCDATE()), 0),
    ('I disagree with this point.', @postId1, @sampleUserId, DATEADD(day, -5, GETUTCDATE()), 0),
    ('This information is incorrect.', @postId2, @sampleUserId, DATEADD(day, -2, GETUTCDATE()), 2);

-- 5. Verification queries
PRINT '';
PRINT 'Test Data Summary:';
PRINT '==================';

SELECT 'Total Users' as Metric, COUNT(*) as Count FROM AspNetUsers
UNION ALL
SELECT 'Total Posts', COUNT(*) FROM Posts
UNION ALL
SELECT 'Flagged Posts', COUNT(*) FROM Posts WHERE ModerationFlags > 0
UNION ALL
SELECT 'Total Comments', COUNT(*) FROM Comments
UNION ALL
SELECT 'Flagged Comments', COUNT(*) FROM Comments WHERE ModerationFlags > 0
UNION ALL
SELECT 'Users with Moderator Role', COUNT(*) 
FROM AspNetUserRoles ur 
JOIN AspNetRoles r ON ur.RoleId = r.Id 
WHERE r.Name = 'Moderator';

-- 6. Show recent posts with stats
PRINT '';
PRINT 'Recent Posts with Stats:';
SELECT TOP 5 
    p.Id,
    p.Title,
    p.CreatedAt,
    u.UserName as Author,
    ISNULL(ps.LikeCount, 0) as Likes,
    ISNULL(ps.CommentCount, 0) as Comments,
    CASE 
        WHEN p.ModerationFlags = 0 THEN 'Clean'
        WHEN p.ModerationFlags = 1 THEN 'Misleading'
        WHEN p.ModerationFlags = 2 THEN 'False'
        ELSE 'Multiple Flags'
    END as ModerationStatus
FROM Posts p
JOIN AspNetUsers u ON p.AuthorId = u.Id
LEFT JOIN PostStats ps ON p.Id = ps.PostId
ORDER BY p.CreatedAt DESC;

-- 7. Show event processing status
PRINT '';
PRINT 'Event Processing Status:';
SELECT 
    'Unprocessed Like Events' as EventType, 
    COUNT(*) as Count 
FROM LikeEvents 
WHERE Processed = 0
UNION ALL
SELECT 
    'Unprocessed Comment Events', 
    COUNT(*) 
FROM CommentEvents 
WHERE Processed = 0;

-- 8. Cleanup old test data (optional - uncomment to use)
/*
-- Remove test users and their data (CASCADE will handle related records)
DELETE FROM AspNetUsers 
WHERE Email LIKE '%test.com' 
   OR Email LIKE '%@example.com'
   OR UserName LIKE 'test%'
   OR UserName LIKE 'user%_' -- matches user1_123, user2_456, etc.
   
PRINT 'Test data cleaned up';
*/