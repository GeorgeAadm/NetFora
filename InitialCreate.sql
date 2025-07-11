﻿-- Set required options for filtered indexes
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRANSACTION;

-- Create ASP.NET Identity tables
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [DisplayName] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UserName] nvarchar(256) NOT NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

-- Use smaller key lengths to avoid the 900 byte limit warning
CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

-- Create filtered indexes with proper SET options
CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) 
WHERE [NormalizedName] IS NOT NULL;

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) 
WHERE [NormalizedUserName] IS NOT NULL;

COMMIT;
GO

-- Insert default roles
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES 
(NEWID(), 'User', 'USER', NEWID()),
(NEWID(), 'Moderator', 'MODERATOR', NEWID());
GO

-- Create Posts table
CREATE TABLE Posts (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Title nvarchar(200) NOT NULL,
    Content nvarchar(max) NOT NULL,
    AuthorId nvarchar(450) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    ModerationFlags int NOT NULL DEFAULT 0,
    
    -- Foreign key to AspNetUsers
    FOREIGN KEY (AuthorId) REFERENCES AspNetUsers(Id)
);

-- Create indexes for Posts
CREATE INDEX IX_Posts_CreatedAt ON Posts(CreatedAt DESC);
CREATE INDEX IX_Posts_AuthorId ON Posts(AuthorId);
CREATE INDEX IX_Posts_ModerationFlags ON Posts(ModerationFlags);
GO

-- Create Comments table
CREATE TABLE Comments (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Content nvarchar(max) NOT NULL,
    PostId int NOT NULL,
    AuthorId nvarchar(450) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    ModerationFlags int NOT NULL DEFAULT 0,
    
    -- Foreign keys
    FOREIGN KEY (PostId) REFERENCES Posts(Id) ON DELETE CASCADE,
    FOREIGN KEY (AuthorId) REFERENCES AspNetUsers(Id)
);

-- Create indexes for Comments
CREATE INDEX IX_Comments_PostId ON Comments(PostId);
CREATE INDEX IX_Comments_AuthorId ON Comments(AuthorId);
CREATE INDEX IX_Comments_CreatedAt ON Comments(CreatedAt);
CREATE INDEX IX_Comments_ModerationFlags ON Comments(ModerationFlags);
GO

-- Create Likes table
CREATE TABLE Likes (
    Id int IDENTITY(1,1) PRIMARY KEY,
    PostId int NOT NULL,
    UserId nvarchar(450) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Unique constraint (one like per user per post)
    CONSTRAINT UQ_Likes_PostId_UserId UNIQUE (PostId, UserId),
    
    -- Foreign keys
    FOREIGN KEY (PostId) REFERENCES Posts(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- Create index for Likes
CREATE INDEX IX_Likes_UserId ON Likes(UserId);
GO

-- Create PostStats table
CREATE TABLE PostStats (
    PostId int PRIMARY KEY,
    LikeCount int NOT NULL DEFAULT 0,
    CommentCount int NOT NULL DEFAULT 0,
    LastUpdated datetime2 NOT NULL DEFAULT GETUTCDATE(),
    Version bigint NOT NULL DEFAULT 1,
    
    -- Foreign key
    FOREIGN KEY (PostId) REFERENCES Posts(Id) ON DELETE CASCADE
);
GO

-- Create LikeEvents table
CREATE TABLE LikeEvents (
    Id bigint IDENTITY(1,1) PRIMARY KEY,
    PostId int NOT NULL,
    UserId nvarchar(450) NOT NULL,
    Action varchar(10) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    Processed bit NOT NULL DEFAULT 0,
    ProcessedAt datetime2 NULL,
    
    -- Foreign keys
    FOREIGN KEY (PostId) REFERENCES Posts(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

-- Create indexes for LikeEvents
CREATE INDEX IX_LikeEvents_Processed_CreatedAt ON LikeEvents(Processed, CreatedAt);
CREATE INDEX IX_LikeEvents_PostId ON LikeEvents(PostId);
GO

-- Create CommentEvents table
CREATE TABLE CommentEvents (
    Id bigint IDENTITY(1,1) PRIMARY KEY,
    PostId int NOT NULL,
    CommentId int NULL,
    UserId nvarchar(450) NOT NULL,
    Action varchar(20) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    Processed bit NOT NULL DEFAULT 0,
    ProcessedAt datetime2 NULL,
    
    -- Foreign keys
    FOREIGN KEY (PostId) REFERENCES Posts(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

-- Create indexes for CommentEvents
CREATE INDEX IX_CommentEvents_Processed_CreatedAt ON CommentEvents(Processed, CreatedAt);
CREATE INDEX IX_CommentEvents_PostId ON CommentEvents(PostId);
GO

-- Verify tables were created
SELECT 'Tables created successfully' AS Status;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
ORDER BY TABLE_NAME;
GO