using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToplulukYonetimSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FixMemberCommunityRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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

            migrationBuilder.Sql("""
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'Members', N'CommunityId') IS NULL
                BEGIN
                    ALTER TABLE [Members] ADD [CommunityId] int NOT NULL DEFAULT 0;
                END
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[MemberCommunities]', N'U') IS NOT NULL
                    DROP TABLE [MemberCommunities];
                """);
        }
    }
}
