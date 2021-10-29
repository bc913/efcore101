using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;
using System;
using System.Data.Common;
using Bcan.Efpg.Domain.Entities;
using Bcan.Efpg.Domain.Contexts;
using System.Linq;

namespace Bcan.Efpg.Tests.OneToMany
{
    
    public class InMemorySqliteTests : OtmTests, IDisposable
    {
        private readonly DbConnection _connection;

        public InMemorySqliteTests() : base(new DbContextOptionsBuilder<Bcan.Efpg.Domain.Contexts.OtmContext>()
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

    #region OPTIONAL
    [TestFixture]
    public class InMemorySqliteTestsOptional : InMemorySqliteTests
    {
        protected override bool IsRequired => false;

        [SetUp]
        public void SetUp()
        {
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Database.EnsureCreated();
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Database.EnsureDeleted();
            }
        }

        #region CREATE
        // Dependent and Principal entities can be created w/ each other.
        [Test]
        public void Can_Create_Dependent_Without_Principal()
        {
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = new Post { Title = "Test Post", Content = "No content" };
                context.Posts.Add(post);
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = context.Posts.FirstOrDefault(p => p.Title == "Test Post");
                Assert.IsNotNull(post);
            }
        }

        [Test]
        public void Can_Create_Principal_Without_Dependent()
        {
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = new Blog { Url = "https://medium.com/@bc" };
                context.Blogs.Add(blog);
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = context.Blogs.FirstOrDefault(b => b.Url == "https://medium.com/@bc");
                Assert.IsNotNull(blog);
            }
        }

        #endregion

        #region UPDATE
        [Test]
        public void Can_Update_Dependent_Through_Principal_With_Include()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Update post through its principal
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = context.Blogs.Include(b => b.Posts).FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNotNull(blog);
                Assert.IsNotNull(blog.Posts);

                var post = blog.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET Core 3"));
                Assert.IsNotNull(post);

                post.Title = "Deploying .NET 5 apps as self-contained";
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET 5 apps"));
                Assert.IsNotNull(post);
            }
        }

        [Test]
        public void Can_NOT_Update_Dependent_Through_Principal_Without_Include()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Update post through its principal
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = context.Blogs.FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNotNull(blog);
                // Since it is initialized by default, dependent is not null
                Assert.IsNotNull(blog.Posts);
                Assert.AreEqual(0, blog.Posts.Count);
            }
        }

        [Test]
        public void Can_Update_Dependent_Alone()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Update post through its principal
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET Core 3"));
                Assert.IsNotNull(post);

                post.Title = "Deploying .NET 5 apps as self-contained";
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET 5 apps"));
                Assert.IsNotNull(post);
            }
        }


        #endregion

        #region DELETE 
        [Test]
        public void Deleting_Principal_FAILS_Without_Including_Dependent()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Delete fails for principal entity because it needs to have navigation property loaded to set FK to NULL.
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = context.Blogs.FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNotNull(blog);
                context.Blogs.Remove(blog);
                Assert.That(() => context.SaveChanges(), Throws.Exception);
            }
        }

        [Test]
        public void Deleting_Principal_With_Include_SUCCEED_Has_No_Effect_On_Dependent()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Update post through its principal
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = context.Blogs.Include(b => b.Posts).FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNotNull(blog);
                context.Blogs.Remove(blog);
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                // Blog is removed
                var blog = context.Blogs.FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNull(blog);

                // Post still alive
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET Core 3"));
                Assert.IsNotNull(post);

                var post2 = context.Posts.FirstOrDefault(p => p.Title == "Skipping CI in GitHub Actions Workflows");
                Assert.IsNotNull(post2);
            }
        }

        [Test]
        public void Deleting_DEPENDENT_Has_No_Effect_On_Principal()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Update post through its principal
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET Core 3"));
                Assert.IsNotNull(post);
                context.Posts.Remove(post);
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                // Post is deleted
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET Core 3"));
                Assert.IsNull(post);

                // PRincipal not removed
                var blog = context.Blogs.FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNotNull(blog);                
            }
        }
        #endregion
    }

    #endregion

    #region REQUIRED
    [TestFixture]
    public class InMemorySqliteTestsRequired : InMemorySqliteTests
    {
        protected override bool IsRequired => true;

        [SetUp]
        public void SetUp()
        {
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Database.EnsureCreated();
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Database.EnsureDeleted();
            }
        }

        #region CREATE
        [Test]
        public void Can_NOT_Create_Dependent_Without_Principal()
        {
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = new Post { Title = "Test Post", Content = "No content" };
                context.Posts.Add(post);
                Assert.That(() => context.SaveChanges(), Throws.Exception);
            }
        }

        [Test]
        public void Can_Create_Dependent_With_Principal_Only()
        {
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = new Post { Title = "Test Post", Content = "No content", Blog = new Blog { Url = "https://medium.com"} };
                context.Posts.Add(post);
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = context.Posts.Include(p=>p.Blog).FirstOrDefault(p => p.Title == "Test Post");
                Assert.IsNotNull(post);
                Assert.IsNotNull(post.Blog);
            }

        }

        [Test]
        public void Can_Create_Principal_Without_Dependent()
        {
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = new Blog { Url = "https://medium.com/@bc" };
                context.Blogs.Add(blog);
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = context.Blogs.FirstOrDefault(b => b.Url == "https://medium.com/@bc");
                Assert.IsNotNull(blog);
            }
        }

        #endregion

        #region UPDATE
        [Test]
        public void Can_Update_Dependent_Through_Principal_With_Include()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Update post through its principal
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = context.Blogs.Include(b => b.Posts).FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNotNull(blog);
                Assert.IsNotNull(blog.Posts);

                var post = blog.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET Core 3"));
                Assert.IsNotNull(post);

                post.Title = "Deploying .NET 5 apps as self-contained";
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET 5 apps"));
                Assert.IsNotNull(post);
            }
        }

        [Test]
        public void Can_NOT_Update_Dependent_Through_Principal_Without_Include()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Update post through its principal
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = context.Blogs.FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNotNull(blog);
                // Since it is initialized by default, dependent is not null
                Assert.IsNotNull(blog.Posts);
                Assert.AreEqual(0, blog.Posts.Count);
            }
        }

        [Test]
        public void Can_Update_Dependent_Alone()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Update post through its principal
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET Core 3"));
                Assert.IsNotNull(post);

                post.Title = "Deploying .NET 5 apps as self-contained";
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET 5 apps"));
                Assert.IsNotNull(post);
            }
        }


        #endregion

        #region DELETE 
        [Test]
        public void Deleting_Principal_Will_Remove_Dependent()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Update post through its principal
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = context.Blogs.FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNotNull(blog);
                context.Blogs.Remove(blog);
                context.SaveChanges();
            }

            // Dependent entities are removed
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = context.Blogs.FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNull(blog);

                var post1 = context.Posts.FirstOrDefault(p => p.Title == "Deploying .NET Core 3 apps as self-contained");
                Assert.IsNull(post1);

                var post2 = context.Posts.FirstOrDefault(p => p.Title == "Skipping CI in GitHub Actions Workflows");
                Assert.IsNull(post2);
            }
        }

        [Test]
        public void Deleting_Principal_With_Include_Will_Remove_Dependent()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Update post through its principal
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var blog = context.Blogs.Include(b => b.Posts).FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNotNull(blog);
                context.Blogs.Remove(blog);
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                // Blog is removed
                var blog = context.Blogs.FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNull(blog);

                // Post IS REMOVED
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET Core 3"));
                Assert.IsNull(post);
            }
        }

        [Test]
        public void Deleting_DEPENDENT_Has_No_Effect_On_Principal()
        {
            // Populate data
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                context.Blogs.AddRange(GenerateData());
                context.SaveChanges();
            }

            // Update post through its principal
            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET Core 3"));
                Assert.IsNotNull(post);
                context.Posts.Remove(post);
                context.SaveChanges();
            }

            using (var context = new OtmContext(ContextOptions, IsRequired))
            {
                // Post is deleted
                var post = context.Posts.FirstOrDefault(p => p.Title.Contains("Deploying .NET Core 3"));
                Assert.IsNull(post);

                // PRincipal not removed
                var blog = context.Blogs.FirstOrDefault(b => b.Url == "https://timheuer.com/blog/");
                Assert.IsNotNull(blog);
            }
        }
        #endregion

    }

    #endregion
}