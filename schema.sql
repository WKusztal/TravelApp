IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240725155944_InitialCreate', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241020150934_AddUsersTable', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Username');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Users] ALTER COLUMN [Username] nvarchar(max) NULL;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'PasswordHash');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Users] ALTER COLUMN [PasswordHash] nvarchar(max) NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Email');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Users] ALTER COLUMN [Email] nvarchar(max) NULL;
GO

ALTER TABLE [Users] ADD [Role] nvarchar(max) NOT NULL DEFAULT N'';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241116110526_AddRoleColumnToUsers', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [AvatarPath] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241213131313_AddAvatarPathToUser', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [Bio] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241215122231_AddBioColumn', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241215150401_UpdateUsers', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Stories] (
    [StoryId] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [ImagePath] nvarchar(max) NULL,
    [Continent] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_Stories] PRIMARY KEY ([StoryId]),
    CONSTRAINT [FK_Stories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Stories_UserId] ON [Stories] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241217142001_AddStoriesTable', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250102161611_MakeAuthorNullable', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [StoryImages] (
    [Id] int NOT NULL IDENTITY,
    [ImagePath] nvarchar(max) NOT NULL,
    [StoryId] int NOT NULL,
    CONSTRAINT [PK_StoryImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StoryImages_Stories_StoryId] FOREIGN KEY ([StoryId]) REFERENCES [Stories] ([StoryId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_StoryImages_StoryId] ON [StoryImages] ([StoryId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250110124402_AddStoryImages', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Stories] ADD [Dislikes] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Stories] ADD [Likes] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250117163211_AddLikesAndDislikesToStories', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Reactions] (
    [ReactionId] int NOT NULL IDENTITY,
    [StoryId] int NOT NULL,
    [UserName] nvarchar(max) NULL,
    [IsLike] bit NOT NULL,
    CONSTRAINT [PK_Reactions] PRIMARY KEY ([ReactionId])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250119143357_AddReactionsTable', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Follows] (
    [Id] int NOT NULL IDENTITY,
    [FollowerId] int NOT NULL,
    [FollowingId] int NOT NULL,
    CONSTRAINT [PK_Follows] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250122115209_AddFollowTable', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [FollowersCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Users] ADD [FollowingCount] int NOT NULL DEFAULT 0;
GO

CREATE TABLE [UserUser] (
    [FollowersUserId] int NOT NULL,
    [FollowingUserId] int NOT NULL,
    CONSTRAINT [PK_UserUser] PRIMARY KEY ([FollowersUserId], [FollowingUserId]),
    CONSTRAINT [FK_UserUser_Users_FollowersUserId] FOREIGN KEY ([FollowersUserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserUser_Users_FollowingUserId] FOREIGN KEY ([FollowingUserId]) REFERENCES [Users] ([UserId])
);
GO

CREATE INDEX [IX_UserUser_FollowingUserId] ON [UserUser] ([FollowingUserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250122130431_AddFollowersAndFollowingCount', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250122163339_FixFollowingId', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Tag] (
    [TagId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Tag] PRIMARY KEY ([TagId])
);
GO

CREATE TABLE [StoryTag] (
    [StoryId] int NOT NULL,
    [TagId] int NOT NULL,
    CONSTRAINT [PK_StoryTag] PRIMARY KEY ([StoryId], [TagId]),
    CONSTRAINT [FK_StoryTag_Stories_StoryId] FOREIGN KEY ([StoryId]) REFERENCES [Stories] ([StoryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_StoryTag_Tag_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tag] ([TagId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_StoryTag_TagId] ON [StoryTag] ([TagId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250124115910_AddTagsToStories', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [StoryTag] DROP CONSTRAINT [FK_StoryTag_Tag_TagId];
GO

ALTER TABLE [Tag] DROP CONSTRAINT [PK_Tag];
GO

EXEC sp_rename N'[Tag]', N'Tags';
GO

ALTER TABLE [Tags] ADD CONSTRAINT [PK_Tags] PRIMARY KEY ([TagId]);
GO

ALTER TABLE [StoryTag] ADD CONSTRAINT [FK_StoryTag_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([TagId]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250124135842_EnsureTagsTable', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Reports] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Reports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Reports_UserId] ON [Reports] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250129114805_AddReportsTable', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Reports] DROP CONSTRAINT [FK_Reports_Users_UserId];
GO

ALTER TABLE [Reports] ADD [StoryId] int NOT NULL DEFAULT 0;
GO

CREATE INDEX [IX_Reports_StoryId] ON [Reports] ([StoryId]);
GO

ALTER TABLE [Reports] ADD CONSTRAINT [FK_Reports_Stories_StoryId] FOREIGN KEY ([StoryId]) REFERENCES [Stories] ([StoryId]) ON DELETE NO ACTION;
GO

ALTER TABLE [Reports] ADD CONSTRAINT [FK_Reports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250130110732_RecreateReportsTable', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Comments] (
    [CommentId] int NOT NULL IDENTITY,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UserId] int NOT NULL,
    [StoryId] int NOT NULL,
    CONSTRAINT [PK_Comments] PRIMARY KEY ([CommentId]),
    CONSTRAINT [FK_Comments_Stories_StoryId] FOREIGN KEY ([StoryId]) REFERENCES [Stories] ([StoryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Comments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO

CREATE INDEX [IX_Comments_StoryId] ON [Comments] ([StoryId]);
GO

CREATE INDEX [IX_Comments_UserId] ON [Comments] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250131162717_AddCommentsFixed', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Comments] ADD [UserName] nvarchar(max) NOT NULL DEFAULT N'';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250201104831_AddUserNameToComments', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Articles] (
    [ArticleId] int NOT NULL IDENTITY,
    [Title] nvarchar(100) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [ImagePath] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Articles] PRIMARY KEY ([ArticleId])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250205103257_AddArticlesTable', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ArticleImage] (
    [Id] int NOT NULL IDENTITY,
    [ImagePath] nvarchar(max) NOT NULL,
    [ArticleId] int NOT NULL,
    CONSTRAINT [PK_ArticleImage] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ArticleImage_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([ArticleId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ArticleImage_ArticleId] ON [ArticleImage] ([ArticleId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250205142124_AddArticleImages', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ArticleImage] DROP CONSTRAINT [FK_ArticleImage_Articles_ArticleId];
GO

ALTER TABLE [ArticleImage] DROP CONSTRAINT [PK_ArticleImage];
GO

EXEC sp_rename N'[ArticleImage]', N'ArticleImages';
GO

EXEC sp_rename N'[ArticleImages].[IX_ArticleImage_ArticleId]', N'IX_ArticleImages_ArticleId', N'INDEX';
GO

ALTER TABLE [ArticleImages] ADD CONSTRAINT [PK_ArticleImages] PRIMARY KEY ([Id]);
GO

ALTER TABLE [ArticleImages] ADD CONSTRAINT [FK_ArticleImages_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([ArticleId]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250206185613_RenameArticleImageToArticleImages', N'8.0.7');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [UserUser];
GO

CREATE INDEX [IX_Follows_FollowerId] ON [Follows] ([FollowerId]);
GO

CREATE INDEX [IX_Follows_FollowingId] ON [Follows] ([FollowingId]);
GO

ALTER TABLE [Follows] ADD CONSTRAINT [FK_Follows_Users_FollowerId] FOREIGN KEY ([FollowerId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE;
GO

ALTER TABLE [Follows] ADD CONSTRAINT [FK_Follows_Users_FollowingId] FOREIGN KEY ([FollowingId]) REFERENCES [Users] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250210105352_FixFollowRelationsV2', N'8.0.7');
GO

COMMIT;
GO

