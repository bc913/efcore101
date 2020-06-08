using System;
using System.Collections.Generic;

namespace Db.Sqlite.Entities
{
    public class Book
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<BookAuthorLink> BookAuthorLinks { get; set; }
    }

    public class Author
    {
        public Guid Id { get; set; }

        public string FullName { get; set; }

        public List<BookAuthorLink> BookAuthorLinks { get; set; }

    }

    public class BookAuthorLink
    {
        public Guid BookId { get; set; }
        public Book Book { get; set; }

        public Guid AuthorId { get; set; }
        public Author Author { get; set; }
    }
}
