using Microsoft.EntityFrameworkCore;
using TravelApp.Models;

namespace TravelApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public required DbSet<User> Users { get; set; }
        public required DbSet<Story> Stories { get; set; }
        public required DbSet<StoryImage> StoryImages { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Report> Reports { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Article> Articles { get; set; } = null!;
        public required DbSet<ArticleImage> ArticleImages { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowingId });

            modelBuilder.Entity<Story>()
                .HasMany(s => s.Images)
                .WithOne(i => i.Story)
                .HasForeignKey(i => i.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StoryImage>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Tag>()
                .HasMany(t => t.Stories)
                .WithMany(s => s.Tags)
                .UsingEntity<Dictionary<string, object>>(
                    "StoryTag",
                    j => j.HasOne<Story>().WithMany().HasForeignKey("StoryId"),
                    j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId")
                );

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Story)
                .WithMany(s => s.Reports)
                .HasForeignKey(r => r.StoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Story)
                .WithMany(s => s.Comments)
                .HasForeignKey(c => c.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Reaction> Reactions { get; set; }

    }
}
