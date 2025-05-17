using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Nexy.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ModelProfile> ModelProfiles { get; set; }
        public DbSet<ModelPost> ModelPosts { get; set; }
        public DbSet<UserMessage> UserMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ModelProfile>()
                .HasMany(m => m.Posts)
                .WithOne(p => p.Model)
                .HasForeignKey(p => p.ModelId);

            builder.Entity<ModelProfile>()
                .HasMany(m => m.Messages)
                .WithOne(m => m.Model)
                .HasForeignKey(m => m.ModelId);
        }
    }
}