using Bcan.Domain.Contexts;
using Bcan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bcan.Tests.ManyToMany
{
    [TestFixture]
    public class InMemorySqliteTestsOptional : InMemorySqliteTests 
    {
        [SetUp]
        public void SetUp()
        {
            using (var context = new MtmContext(ContextOptions))
            {
                context.Database.EnsureCreated();
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var context = new MtmContext(ContextOptions))
            {
                context.Database.EnsureDeleted();
            }
        }

        #region CREATE
        [Test]
        public void Create_Record_Succeeds()
        {
            using (var context = new MtmContext(ContextOptions))
            {
                var join = new BookAuthorLink { Book = new Book { Name = "The Guns of August" }, Author = new Author { FullName = "Barbara W. Tuchman" } };
                context.Add(join);
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var book = context.Books.FirstOrDefault(b => b.Name == "The Guns of August");
                Assert.IsNotNull(book);
            }
        }

        [Test]
        public void Create_Record_Without_Author_FAILS()
        {
            using (var context = new MtmContext(ContextOptions))
            {
                var join = new BookAuthorLink { Book = new Book { Name = "The Guns of August" }};
                context.Add(join);
                Assert.That(() => context.SaveChanges(), Throws.Exception);
            }
        }

        [Test]
        public void Create_Record_Without_Book_FAILS()
        {
            using (var context = new MtmContext(ContextOptions))
            {
                var join = new BookAuthorLink { Author = new Author { FullName = "Barbara W. Tuchman" } };
                context.Add(join);
                Assert.That(() => context.SaveChanges(), Throws.Exception);
            }
        }
        #endregion

        #region UPDATE
        [Test]
        public void Update_Book_By_Querying_Book()
        {
            using (var context = new MtmContext(ContextOptions))
            {
                context.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var book = context.Books.FirstOrDefault(b => b.Name == "What Went Wrong?");
                Assert.IsNotNull(book);
                book.Name = "What Went Wrong ?";
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var book = context.Books.FirstOrDefault(b => b.Name == "What Went Wrong ?");
                Assert.IsNotNull(book);
            }
        }

        [Test]
        public void Update_Book_By_Querying_Author_FAILS_Without_Include()
        {
            using (var context = new MtmContext(ContextOptions))
            {
                context.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var author = context.Authors.FirstOrDefault(b => b.FullName == "Bernard Lewis");
                Assert.IsNotNull(author);
                Assert.IsNull(author.BookAuthorLinks);
            }
        }

        [Test]
        public void Update_Book_By_Querying_Author_SUCCEEDS()
        {

            using (var context = new MtmContext(ContextOptions))
            {
                context.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var author = context.Authors.Include(a=>a.BookAuthorLinks).ThenInclude(l=>l.Book).FirstOrDefault(b => b.FullName == "Bernard Lewis");
                Assert.IsNotNull(author);
                Assert.IsNotNull(author.BookAuthorLinks);
                // Find the book
                var book = author.BookAuthorLinks.Select(l => l.Book).FirstOrDefault();
                Assert.IsNotNull(book);
                book.Name = "What Went Wrong ?";
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var book = context.Books.FirstOrDefault(b => b.Name == "What Went Wrong ?");
                Assert.IsNotNull(book);
            }
        }

        // Author
        [Test]
        public void Update_Author_By_Querying_Author()
        {

            using (var context = new MtmContext(ContextOptions))
            {
                context.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var author = context.Authors.FirstOrDefault(a => a.FullName == "W. G. Price");
                Assert.IsNotNull(author);
                author.FullName = "G. Price";
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var author = context.Authors.FirstOrDefault(a => a.FullName == "G. Price");
                Assert.IsNotNull(author);
            }
        }

        [Test]
        public void Update_Author_By_Querying_Book_FAILS_Without_Include()
        {

            using (var context = new MtmContext(ContextOptions))
            {
                context.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var book = context.Books.FirstOrDefault(b => b.Name == "Hydroelasticity of Ships");
                Assert.IsNotNull(book);
                Assert.IsNull(book.BookAuthorLinks);
            }
        }

        [Test]
        public void Update_Author_By_Querying_Book_SUCCEED_Without_Include()
        {

            using (var context = new MtmContext(ContextOptions))
            {
                context.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var book = context.Books.Include(b=>b.BookAuthorLinks).ThenInclude(l=>l.Author).FirstOrDefault(b => b.Name == "Hydroelasticity of Ships");
                Assert.IsNotNull(book);
                Assert.IsNotNull(book.BookAuthorLinks);

                var author = book.BookAuthorLinks.Select(l => l.Author).FirstOrDefault(a => a.FullName == "W. G. Price");
                Assert.IsNotNull(author);
                author.FullName = "G. Price";
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var author = context.Authors.FirstOrDefault(a => a.FullName == "G. Price");
                Assert.IsNotNull(author);
            }
        }

        #endregion

        #region DELETE
        [Test]
        public void Delete_Author_Does_NOT_Remove_Book()
        {
            using (var context = new MtmContext(ContextOptions))
            {
                context.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var author = context.Authors.FirstOrDefault(a => a.FullName == "Bernard Lewis");
                Assert.IsNotNull(author);
                context.Authors.Remove(author);
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var author = context.Authors.FirstOrDefault(a => a.FullName == "Bernard Lewis");
                Assert.IsNull(author);

                var book = context.Books.Include(b=>b.BookAuthorLinks).ThenInclude(l=>l.Author).FirstOrDefault(b => b.Name == "What Went Wrong?");
                Assert.IsNotNull(book);
                Assert.IsNotNull(book.BookAuthorLinks);

                var author2 = book.BookAuthorLinks.Select(l => l.Author).FirstOrDefault();
                Assert.IsNull(author2);
            }
        }

        [Test]
        public void Disassociate_Author_By_Querying_Book_Does_NOT_Remove_Book()
        {
            using (var context = new MtmContext(ContextOptions))
            {
                context.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var book = context.Books.Include(b => b.BookAuthorLinks).ThenInclude(l => l.Author).FirstOrDefault(b => b.Name == "Hydroelasticity of Ships");
                Assert.IsNotNull(book);
                Assert.IsNotNull(book.BookAuthorLinks);

                var author = book.BookAuthorLinks.Select(l => l.Author).FirstOrDefault(a => a.FullName == "W. G. Price");
                Assert.IsNotNull(author);

                var join = book.BookAuthorLinks.FirstOrDefault(l => l.Author.FullName == "W. G. Price");
                Assert.IsNotNull(join);

                book.BookAuthorLinks.Remove(join);
                context.SaveChanges();
                // The author is deassociated from the book. IT is not deleted
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var author = context.Authors.FirstOrDefault(a => a.FullName == "W. G. Price");
                // It is not removed from the database.
                Assert.IsNotNull(author);

                var book = context.Books.Include(b => b.BookAuthorLinks).ThenInclude(l => l.Author).FirstOrDefault(b => b.Name == "Hydroelasticity of Ships");
                Assert.IsNotNull(book);
                Assert.IsNotNull(book.BookAuthorLinks);
                Assert.AreEqual(1, book.BookAuthorLinks.Count);

                var author2 = book.BookAuthorLinks.Select(l => l.Author).FirstOrDefault(a => a.FullName == "W. G. Price");
                Assert.IsNull(author2);
            }
        }

        // book
        [Test]
        public void Delete_Book_Does_NOT_Remove_Author()
        {
            using (var context = new MtmContext(ContextOptions))
            {
                context.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var book = context.Books.FirstOrDefault(b => b.Name == "What Went Wrong?");
                Assert.IsNotNull(book);
                context.Books.Remove(book);
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var book = context.Books.FirstOrDefault(b => b.Name == "What Went Wrong?");
                Assert.IsNull(book);

                var author = context.Authors.Include(b => b.BookAuthorLinks).ThenInclude(l => l.Book).FirstOrDefault(a => a.FullName == "Bernard Lewis");
                Assert.IsNotNull(author);
                Assert.IsNotNull(author.BookAuthorLinks);
                Assert.AreEqual(0, author.BookAuthorLinks.Count);

                var book2 = author.BookAuthorLinks.Select(l => l.Book).FirstOrDefault();
                Assert.IsNull(book2);
            }
        }

        [Test]
        public void Disassociate_Book_Does_NOT_Remove_Author()
        {
            using (var context = new MtmContext(ContextOptions))
            {
                context.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var author = context.Authors.Include(b => b.BookAuthorLinks).ThenInclude(l => l.Book).FirstOrDefault(a => a.FullName == "Bernard Lewis");
                Assert.IsNotNull(author);
                Assert.IsNotNull(author.BookAuthorLinks);
                Assert.AreEqual(1, author.BookAuthorLinks.Count);

                var join = author.BookAuthorLinks.FirstOrDefault();
                Assert.IsNotNull(join);

                author.BookAuthorLinks.Remove(join);
                context.SaveChanges();
            }

            using (var context = new MtmContext(ContextOptions))
            {
                var book = context.Books.FirstOrDefault(b => b.Name == "What Went Wrong?");
                Assert.IsNotNull(book);

                var author = context.Authors.Include(b => b.BookAuthorLinks).ThenInclude(l => l.Book).FirstOrDefault(a => a.FullName == "Bernard Lewis");
                Assert.IsNotNull(author);
                Assert.IsNotNull(author.BookAuthorLinks);
                Assert.AreEqual(0, author.BookAuthorLinks.Count);

                var book2 = author.BookAuthorLinks.Select(l => l.Book).FirstOrDefault();
                Assert.IsNull(book2);
            }
        }
        #endregion
    }
}
