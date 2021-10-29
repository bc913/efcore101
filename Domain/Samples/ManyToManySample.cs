using Microsoft.EntityFrameworkCore;
using System;
using Bcan.Domain.Entities;
using Bcan.Domain.Contexts;
using System.Linq;

namespace Bcan.Domain.Samples
{
    public class ManyToMany
    {
        public static void Run(DbContextOptions options)
        {
            #region CREATE
            Console.WriteLine("----- CREATE -----");
            using (var context = new MtmContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                Console.Write("Creating a book w/ single author");
                var book1 = new Book { Name = "Vibration" };
                var author1 = new Author { FullName = "R.E.D. Bishop" };
                context.Add(new BookAuthorLink { Book = book1, Author = author1 });
                context.SaveChanges();
                Console.WriteLine("==> Done");
                
                Console.Write("Creating a book w/ multiple authors");
                var book2 = new Book { Name = "Hydroelasticity of Ships" };
                var author2 = new Author { FullName = "W.G. Price" };
                context.AddRange(
                    new BookAuthorLink { Book = book2, Author = author1 }, 
                    new BookAuthorLink { Book = book2, Author = author2 });
                context.SaveChanges();
                Console.WriteLine("==> Done");

                {
                    Console.Write("Creating a book w/o author");
                    var b = new Book { Name = "What Went Wrong ?" };
                    var a = new Author { FullName = "Bernard Lewis" };

                    var join1 = new BookAuthorLink { Book = b, Author = null };
                    
                    try
                    {
                        context.Add(join1);
                        context.SaveChanges();
                        Console.WriteLine("==> Done");
                    }
                    catch (Exception)
                    {
                        context.Entry(b).State = EntityState.Detached;
                        context.Entry(join1).State = EntityState.Detached;
                        Console.WriteLine("==> FAILED: Can not create a book w/o author");
                    }

                    Console.Write("Creating an author w/o book");
                    var join2 = new BookAuthorLink { Book = null, Author = a };
                    
                    try
                    {
                        context.Add(join2);
                        context.SaveChanges();
                        Console.WriteLine("==> Done");
                    }
                    catch (Exception)
                    {
                        context.Entry(a).State = EntityState.Detached;
                        context.Entry(join2).State = EntityState.Detached;
                        Console.WriteLine("==> FAILED: Can not create an author w/o book");
                    }

                    
                }

                Console.Write("Create another record");
                context.Add(new BookAuthorLink { Book = new Book { Name = "Open" }, Author = new Author { FullName = "Andre Agasi" } });
                context.SaveChanges();
                Console.WriteLine("==> Done");

                Console.Write("Create another record");
                context.Add(new BookAuthorLink { Book = new Book { Name = "What Went Wrong ?" }, Author = new Author { FullName = "Bernard Lewis" } });
                context.SaveChanges();
                Console.WriteLine("==> Done");



            }
            #endregion

            #region QUERY
            Console.WriteLine("----- QUERY -----");
            using (var context = new MtmContext(options))
            {
                {
                    Console.Write(" Query a book w/ single author\n");
                    var book = context.Books.Include(b => b.BookAuthorLinks).FirstOrDefault(b => b.Name == "Open");
                    if (book.BookAuthorLinks.Count == 1)
                    {
                        var isSameBook = book.BookAuthorLinks[0].Book == book;
                        Console.WriteLine("Same book: {0}", isSameBook);
                        Console.WriteLine("Author not loaded: {0} Use ThenInclude to have author for this book.", book.BookAuthorLinks[0].Author == null);
                        Console.WriteLine(" ==> DONE:");
                    }
                    else
                        Console.WriteLine(" ==> FAILED: More than one join record");
                }

                Console.Write("Query authors of a book - 1\n");
                {
                    
                    var book2 = context.Books
                        .Include(b => b.BookAuthorLinks)
                        .ThenInclude(l => l.Author)
                        .FirstOrDefault(b => b.Name == "Hydroelasticity of Ships");

                    if (book2.BookAuthorLinks.Count == 2)
                    {
                        var isSameBook1 = book2.BookAuthorLinks[0].Book == book2;
                        Console.WriteLine("Same book: {0}", isSameBook1);

                        var isSameBook2 = book2.BookAuthorLinks[1].Book == book2;
                        Console.WriteLine("Same book: {0}", isSameBook2);

                        var authors = book2.BookAuthorLinks.Select(l => l.Author);
                        Console.WriteLine(string.Format("Count Authors of book {0} is {1}", book2.Name, authors.Count()));
                        foreach (var a in authors)
                            Console.WriteLine("Author: {0}", a.FullName);

                        Console.WriteLine(" ==> DONE:");
                    }
                }

                Console.Write("Query authors of a book - 2\n");
                {
                    var authors2 = context.Authors
                        .Include(a => a.BookAuthorLinks)
                        .ThenInclude(l => l.Book)
                        .Where(a => a.BookAuthorLinks.Any(l => l.Book.Name == "Hydroelasticity of Ships"));

                    Console.WriteLine(string.Format("Count Authors of book {0} is {1}", "Hydroelasticity of Ships", authors2.Count()));
                    foreach (var a in authors2)
                        Console.WriteLine("Author: {0}", a.FullName);

                    Console.WriteLine(" ==> DONE:");
                }

                Console.Write("Query books of author R.E.D.Bishop - 1\n");
                {
                    var author = context.Authors
                        .Include(b => b.BookAuthorLinks)
                        .ThenInclude(l => l.Book)
                        .FirstOrDefault(a => a.FullName == "R.E.D. Bishop");

                    var books = author.BookAuthorLinks.Select(l => l.Book);
                    foreach (var b in books)
                        Console.WriteLine("Book : {0}", b.Name);

                    Console.WriteLine(" ==> DONE:");
                }

                Console.Write("Query books of author R.E.D.Bishop - 2\n");
                {
                    var books = context.Books
                        .Include(b => b.BookAuthorLinks)
                        .ThenInclude(l => l.Author)
                        .Where(b => b.BookAuthorLinks.Any(l => l.Author.FullName == "R.E.D. Bishop"));

                    foreach (var b in books)
                        Console.WriteLine("Book : {0}", b.Name);

                    Console.WriteLine(" ==> DONE:");
                }

            }


            #endregion

            #region UPDATE
            Console.WriteLine("----- UPDATE -----");
            using (var context = new MtmContext(options))
            {
                {
                    Console.Write("Update book author");
                    var book = context.Books.Include(b => b.BookAuthorLinks).ThenInclude(l => l.Author).FirstOrDefault(b => b.Name == "Open");
                    // This does not work. First, remove the author and then add a new one
                    //book.BookAuthorLinks[0].Author = new Author { FullName = "Andre Agassi" };
                    book.BookAuthorLinks[0].Author.FullName = "Andre Agassi";
                    context.SaveChanges();
                    Console.WriteLine("==> Done");
                }

                {
                    Console.Write("Update author's book name");
                    var author = context.Authors.Include(b => b.BookAuthorLinks).ThenInclude(l => l.Book).FirstOrDefault(a => a.FullName == "Andre Agassi");
                    // This does not work. First, remove the author and then add a new one
                    //book.BookAuthorLinks[0].Author = new Author { FullName = "Andre Agassi" };
                    author.BookAuthorLinks[0].Book.Name = "Open II";
                    context.SaveChanges();
                    Console.WriteLine("==> Done");
                }
            }

            #endregion


            #region DELETE
            Console.WriteLine("----- DELETE -----");
            using (var context = new MtmContext(options))
            {
                // IT removes the book and author from join table
                // But the book record stands
                Console.Write(" Remove author only");
                var author = context.Authors.FirstOrDefault(a => a.FullName == "Andre Agassi");
                context.Authors.Remove(author);
                context.SaveChanges();
                Console.WriteLine("==> Done");

                // It removes the record from the join table  and the book from table but keeps the author
                Console.Write(" Remove book only");
                var book = context.Books.Include(b=>b.BookAuthorLinks).FirstOrDefault(b => b.Name == "What Went Wrong ?");
                context.Books.Remove(book);
                context.SaveChanges();
                Console.WriteLine("==> Done");
            }
            #endregion

        }
    }
}
