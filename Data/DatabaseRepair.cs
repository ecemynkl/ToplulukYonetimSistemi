using Microsoft.EntityFrameworkCore;

namespace ToplulukYonetimSistemi.Data
{
    public static class DatabaseRepair
    {
        public static async Task EnsureMemberCommunitySchemaAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
                IF COL_LENGTH(N'Members', N'CommunityId') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'[FK_Members_Communities_CommunityId]', N'F') IS NOT NULL
                        ALTER TABLE [Members] DROP CONSTRAINT [FK_Members_Communities_CommunityId];

                    IF EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE name = N'IX_Members_CommunityId'
                          AND object_id = OBJECT_ID(N'[Members]')
                    )
                        DROP INDEX [IX_Members_CommunityId] ON [Members];

                    ALTER TABLE [Members] DROP COLUMN [CommunityId];
                END
                """);

            await context.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID(N'[MemberCommunities]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [MemberCommunities] (
                        [MemberId] int NOT NULL,
                        [CommunityId] int NOT NULL,
                        CONSTRAINT [PK_MemberCommunities] PRIMARY KEY ([MemberId], [CommunityId]),
                        CONSTRAINT [FK_MemberCommunities_Members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [Members] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_MemberCommunities_Communities_CommunityId] FOREIGN KEY ([CommunityId]) REFERENCES [Communities] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_MemberCommunities_CommunityId] ON [MemberCommunities] ([CommunityId]);
                END
                """);
        }

        public static async Task EnsureContactMessageSchemaAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID(N'[ContactMessages]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [ContactMessages] (
                        [Id] int NOT NULL IDENTITY,
                        [FullName] nvarchar(max) NULL,
                        [Email] nvarchar(max) NULL,
                        [Message] nvarchar(max) NULL,
                        [SentDate] datetime2 NOT NULL,
                        [IsRead] bit NOT NULL,
                        CONSTRAINT [PK_ContactMessages] PRIMARY KEY ([Id])
                    );
                END
                """);
        }

        public static async Task EnsureMediaColumnsAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
                IF COL_LENGTH(N'Communities', N'CoverImagePath') IS NULL
                    ALTER TABLE [Communities] ADD [CoverImagePath] nvarchar(max) NULL;
                """);

            await context.Database.ExecuteSqlRawAsync("""
                IF COL_LENGTH(N'Members', N'ProfileImagePath') IS NULL
                    ALTER TABLE [Members] ADD [ProfileImagePath] nvarchar(max) NULL;

                IF COL_LENGTH(N'Members', N'StudentNumber') IS NULL
                    ALTER TABLE [Members] ADD [StudentNumber] nvarchar(max) NULL;

                IF COL_LENGTH(N'Members', N'RegisteredDate') IS NULL
                    ALTER TABLE [Members] ADD [RegisteredDate] datetime2 NOT NULL
                        CONSTRAINT [DF_Members_RegisteredDate] DEFAULT GETDATE();
                """);
        }

        public static async Task EnsureAnnouncementMediaSchemaAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
                IF COL_LENGTH(N'Announcements', N'CoverImagePath') IS NULL
                    ALTER TABLE [Announcements] ADD [CoverImagePath] nvarchar(max) NULL;
                """);
        }

        public static async Task EnsureEventMediaAndJoinRequestsSchemaAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
                IF COL_LENGTH(N'Events', N'CoverImagePath') IS NULL
                    ALTER TABLE [Events] ADD [CoverImagePath] nvarchar(max) NULL;
                """);

            await context.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID(N'[JoinRequests]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [JoinRequests] (
                        [Id] int NOT NULL IDENTITY,
                        [CommunityId] int NOT NULL,
                        [UserName] nvarchar(max) NOT NULL,
                        [RequestedDate] datetime2 NOT NULL,
                        [IsApproved] bit NOT NULL,
                        [IsRejected] bit NOT NULL,
                        CONSTRAINT [PK_JoinRequests] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_JoinRequests_Communities_CommunityId] FOREIGN KEY ([CommunityId]) REFERENCES [Communities] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_JoinRequests_CommunityId] ON [JoinRequests] ([CommunityId]);
                END
                """);
        }
    }
}
