using Microsoft.EntityFrameworkCore;
using Bcan.Efpg.Domain.Entities;

namespace Bcan.Efpg.Domain.Contexts
{
    public class OtmContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        private readonly bool _isRequired;

        public OtmContext(DbContextOptions options, bool isRequired = false) : base(options)
        {
            _isRequired = isRequired;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // One-to-many
            // Principal: Blog
            // Dependent: Post
            var referenceBuilder = modelBuilder.Entity<Post>()
                .HasOne(p => p.Blog)
                .WithMany(b => b.Posts);

            if (_isRequired)
                referenceBuilder.IsRequired();

            // Or

            //modelBuilder.Entity<Blog>()
            //    .HasMany(b => b.Posts)
            //    .WithOne(p => p.Blog);
        }
    }
}
