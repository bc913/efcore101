using Microsoft.EntityFrameworkCore;
using Bcan.Domain.Entities;

namespace Bcan.Domain.Contexts
{    public class MtmContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }

        public MtmContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Requires Primary keys to be defined on join table
            modelBuilder.Entity<BookAuthorLink>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            // Define Foreign keys on join table
            modelBuilder.Entity<BookAuthorLink>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthorLinks)
                .HasForeignKey("BookId");

            modelBuilder.Entity<BookAuthorLink>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthorLinks)
                .HasForeignKey("AuthorId");
        }
    }
}
