using Microsoft.EntityFrameworkCore;
using ToplulukYonetimSistemi.Models;

namespace ToplulukYonetimSistemi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Community> Communities { get; set; }

        public DbSet<CommunityEvent> Events { get; set; }

        public DbSet<Announcement> Announcements { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<MemberCommunity> MemberCommunities { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MemberCommunity>()
                .HasKey(mc => new { mc.MemberId, mc.CommunityId });

            modelBuilder.Entity<MemberCommunity>()
                .HasOne(mc => mc.Member)
                .WithMany(m => m.MemberCommunities)
                .HasForeignKey(mc => mc.MemberId);

            modelBuilder.Entity<MemberCommunity>()
                .HasOne(mc => mc.Community)
                .WithMany(c => c.MemberCommunities)
                .HasForeignKey(mc => mc.CommunityId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
