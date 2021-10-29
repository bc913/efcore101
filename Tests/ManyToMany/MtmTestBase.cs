using Bcan.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Bcan.Tests.ManyToMany
{
    public abstract class MtmTestBase
    {
        protected readonly DbContextOptions ContextOptions;
        public MtmTestBase(DbContextOptions options)
        {
            ContextOptions = options;
        }

        protected IList<BookAuthorLink> GenerateData()
        {
            var bookAuthorJoin = new List<BookAuthorLink>();

            var book1 = new Book { Name = "Hydroelasticity of Ships" };
            var author11 = new Author { FullName = "R.E.D. Bishop" };
            var author12 = new Author { FullName = "W. G. Price" };
            bookAuthorJoin.Add(new BookAuthorLink { Book = book1, Author = author11 });
            bookAuthorJoin.Add(new BookAuthorLink { Book = book1, Author = author12 });

            var book2 = new Book { Name = "What Went Wrong?" };
            var author2 = new Author { FullName = "Bernard Lewis" };
            bookAuthorJoin.Add(new BookAuthorLink { Book = book2, Author = author2 });

            return bookAuthorJoin;
        }
    }

    public abstract class InMemorySqliteTests : MtmTestBase, IDisposable
    {
        private readonly DbConnection _connection;

        public InMemorySqliteTests() : base(new DbContextOptionsBuilder<Bcan.Domain.Contexts.MtmContext>()
            .UseSqlite(CreateInMemoryDatabase())
            .Options)
        {
            _connection = RelationalOptionsExtension.Extract(ContextOptions).Connection;
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }
        public void Dispose() => _connection.Dispose();
    }
}
