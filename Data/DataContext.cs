using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace redot_api.Data
{

        public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(user => user.Role).HasDefaultValue("Regular");

            modelBuilder.Entity<User>()
                .HasMany(u => u.Posts)
                .WithOne(p => p.Poster)
                .HasForeignKey(p => p.PosterId)
                .OnDelete(DeleteBehavior.Restrict);
                

            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.Commenter)
                .HasForeignKey(c => c.CommenterId);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId);

            modelBuilder.Entity<Subredot>()
                .HasMany(s => s.Subscribers)
                .WithMany(u => u.SubredotsSubscription)
                .UsingEntity<Dictionary<string, object>>(
                    "Subscription",
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    j => j.HasOne<Subredot>().WithMany().HasForeignKey("SubredotId"),
                    j =>
                    {
                        j.Property<DateTime>("JoinedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                        j.HasKey("UserId", "SubredotId");
                    }
                );
            modelBuilder.Entity<Subredot>()
                .HasMany(s => s.Moderators)
                .WithMany(u => u.SubredotsModerators)
                .UsingEntity<Dictionary<string, object>>(
                    "Moderation",
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    j => j.HasOne<Subredot>().WithMany().HasForeignKey("SubredotId"),
                    j =>
                    {
                        j.Property<DateTime>("JoinedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                        j.HasKey("UserId", "SubredotId");
                    }
                );
            
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();

        public DbSet<Vote> Votes => Set<Vote>();
        public DbSet<Subredot> Subredots => Set<Subredot>();

    }
}