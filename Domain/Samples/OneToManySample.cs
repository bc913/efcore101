using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Db.Sqlite.Entities;
using Db.Sqlite.Contexts;

namespace Db.Sqlite.Samples
{
    public class OneToMany
    {
        public static void Run(DbContextOptions options, bool isRequired = false)
        {
            Console.WriteLine("----- CREATE -----");
            using (var context = new OtmContext(options, isRequired))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                
                //                
                Console.Write("Creating a new blog w/o posts");
                var blog1 = new Blog { Url = "http://blogs.msdn.com/adonet" };
                context.Blogs.Add(blog1);
                context.SaveChanges();
                Console.WriteLine("==> Done.");

                Console.Write("Creating a new blog with posts");
                var blog2 = new Blog { Url = "https://wordpress.com" };
                blog2.Posts.AddRange(
                    new List<Post>
                    {
                        new Post { Title = "My first app", Content = "I wrote an app using EF Core 1" },
                        new Post { Title = "My second app", Content = "I wrote an app using EF Core 2" }
                });
                context.Blogs.Add(blog2);
                context.SaveChanges();
                Console.WriteLine("==> Done");

                //
                Console.Write("Creating post w/o blog");
                var post3 = new Post { Title = "EF Core Tutorial", Content = "Getting started with EF Core" };
                context.Posts.Add(post3);
                try
                {
                    context.SaveChanges();
                    Console.WriteLine("==> Done");
                }
                catch (Exception)
                {
                    context.Entry(post3).State = EntityState.Detached;
                    context.Posts.Remove(post3);
                    Console.WriteLine("==> FAILED: Can not save post w/o blog. Provide a required relationship.");
                }
                
                //
                Console.Write("Creating a post w/ blog");
                var post4 = new Post 
                { 
                    Title = "Swift tutorial",
                    Content = "Learn how to code Swift.", 
                    Blog = new Blog { Url = "https://medium.com/@bc/swift"} 
                };
                context.Posts.Add(post4);
                try
                {
                    context.SaveChanges();
                    Console.WriteLine("==> Done");
                }
                catch (Exception)
                {
                    context.Entry(post4).State = EntityState.Detached;
                    context.Posts.Remove(post4);
                    Console.WriteLine("==>FAILED: Can not save the post w/ blog");
                }
            }

            Console.WriteLine("----- QUERY -----");
            using (var context = new OtmContext(options, isRequired))
            {
                Console.Write("Querying for a blog w/o post");
                var foundBlog1 = context.Blogs.First(b => b.Url == "http://blogs.msdn.com/adonet");
                if (foundBlog1 != null)
                    Console.WriteLine("==> Done: Blog1 with Url{0} found", "http://blogs.msdn.com/adonet");
                else
                    Console.WriteLine("==> Failed: Can not find Blog with Url: {0}", "http://blogs.msdn.com/adonet");

                //
                Console.Write("Querying for a blog w posts");
                var foundBlog2 = context.Blogs.FirstOrDefault(b => b.Url == "https://wordpress.com");
                if(foundBlog2 == null)
                {
                    Console.WriteLine("Not found. Querying with .Include() method.");
                    foundBlog2 = context.Blogs.Include(b => b.Posts).FirstOrDefault(b => b.Url == "https://wordpress.com");
                }
                Console.WriteLine("==> Done: Blog w/ Posts with Url{0} found", foundBlog2.Url);

                //
                Console.Write("Querying for post w/o blog");
                var foundPost1 = context.Posts.FirstOrDefault(p => p.Title == "EF Core Tutorial");
                if (foundPost1 != null)
                    Console.WriteLine("==> Done: Post w/ blog is found. Title: {0}", foundPost1.Title);
                else
                    Console.WriteLine("==> Failed: Can not find post w/ blog.");
                
                //
                Console.Write("Querying for a post w blog");
                var foundPost2 = context.Posts.FirstOrDefault(p => p.Blog.Url == "https://medium.com/@bc/swift");
                if(foundPost2 == null)
                {
                    Console.WriteLine("Can not find the post w/ blog. Using .Include()");
                    foundPost2 = context.Posts.Include(p => p.Blog).FirstOrDefault(p => p.Blog.Url == "https://medium.com/@bc/swift");
                }
                Console.WriteLine("==> Done: Post w blog is found");               
            }

            Console.WriteLine("----- UPDATE -----");
            using(var context = new OtmContext(options, isRequired))
            {
                Console.Write("Update a blog w/o post by adding a post");
                var foundBlog1 = context.Blogs.FirstOrDefault(b => b.Url == "http://blogs.msdn.com/adonet");
                foundBlog1.Posts.Add(
                    new Post { Title = "How to create Hello, World app in C++", Content = "This is a very very long story" }
                    );
                context.SaveChanges();
                Console.Write("==>Done");

                Console.Write("Update a blog's post");
                // Need to include navigation property
                var foundBlog2 = context.Blogs.Include(b => b.Posts).FirstOrDefault(b => b.Url == "https://wordpress.com");
                foundBlog2.Posts.Add(new Post { Title = "Smart pointers in C++", Content = "Smart pointers are sometimes life-savers...." });
                context.SaveChanges();
                Console.WriteLine("==> Done");

                Console.Write("Updating a post w/o blog by adding a blog");
                var foundPost1 = context.Posts.FirstOrDefault(p => p.Title == "EF Core Tutorial");
                if (foundPost1 == null)
                    Console.WriteLine("==> FAILED: Can not find the post w/o a blog record. Make sure the relationship is optional");
                else
                {
                    foundPost1.Blog = new Blog { Url = "https://medium.com/@someone/do-coding" };
                    context.SaveChanges();
                    Console.WriteLine("==> Done: Post w/o blog is found and updated");
                }
                    

                Console.Write("Updating a post w/ blog");
                var foundPost2 = context.Posts.Include(p => p.Blog).FirstOrDefault(p => p.Blog.Url == "https://medium.com/@bc/swift");
                if (foundPost2 == null)
                    Console.WriteLine("==> FAILED: Can not find a post w/ blog Url: {0}", "https://medium.com/@bc/swift");
                else
                {
                    foundPost2.Title = "Do some Swift stuff";
                    context.SaveChanges();
                    Console.WriteLine("==> Done: The post w/ blog is found and updated");
                }
                    
             
            }

            Console.WriteLine("----- DELETE -----");
            using(var context = new OtmContext(options, isRequired))
            {
                // Delete
                Console.Write("Delete a blog w/ post");
                var foundBlog = context.Blogs.Include(b => b.Posts).FirstOrDefault(b => b.Posts.Any(p => p.Title == "How to create Hello, World app in C++"));
                if (foundBlog == null)
                    Console.WriteLine("==>FAILED: Can not find a blog w/ post titled: {0}", "How to create Hello, World app in C++");
                else
                {
                    // this will remove the dependent posts if the relationship is required.
                    context.Blogs.Remove(foundBlog);
                    context.SaveChanges();
                    Console.WriteLine("==> Done: Blog is removed");
                }

                Console.Write("Delete a post w/blog");
                // since post is dependent, it does not affect its principal entity
                var foundPost = context.Posts.FirstOrDefault(p => p.Title == "My first app");
                if (foundPost == null)
                    Console.WriteLine("==> Can not find a post w/ blog titled My first app");
                else
                {
                    context.Posts.Remove(foundPost);
                    context.SaveChanges();
                    Console.WriteLine("==> Done: The post titled My first app is removed from the database");
                }
                 
            }

        }
    }
}
