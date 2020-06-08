using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Db.Sqlite.Entities;

namespace Db.Tests.OneToMany
{
    public abstract class OtmTests
    {
        protected readonly DbContextOptions ContextOptions;
        protected virtual bool IsRequired { get; }
        public OtmTests(DbContextOptions options)
        {
            ContextOptions = options;
        }

        protected IList<Blog> GenerateData()
        {
            // Populate all blogs with at least one posts
            var blogList = new List<Blog>();

            // Blog 1
            var blog1 = new Blog { Url = "https://swift.org/blog/" };
            blog1.Posts.AddRange(
                new List<Post>
                {
                    new Post{Title="Swift 5.2 Released!", Content="Swift 5.2 is now officially released!"},
                    new Post{Title="Announcing ArgumentParser", Content="We’re delighted to announce ArgumentParser."}
                });
            blogList.Add(blog1);

            var blog2 = new Blog { Url = "https://devblogs.microsoft.com/dotnet" };
            blog2.Posts.Add(new Post
            {
                Title = "EF Core Tutorial",
                Content = "Getting started with EF Core"
            });
            blogList.Add(blog2);

            var blog3 = new Blog { Url = "https://blog.afach.de/" };
            blog3.Posts.Add(new Post
            {
                Title = "A simple, lock-free object-pool",
                Content = "Apache Thrift is an RPC for calling functions on other ends of networks and across different languages."
            });
            blogList.Add(blog3);

            var blog4 = new Blog { Url = "https://www.hanselman.com/blog/SelfcontainedNETCoreApplications.aspx" };
            blog4.Posts.Add(new Post
            {
                Title = "Self-contained .NET Core Applications",
                Content = "You can now deploy .Net Core applications as self contained."
            });
            blogList.Add(blog4);

            var blog5 = new Blog { Url = "https://timheuer.com/blog/" };
            blog5.Posts.AddRange(
                new List<Post>
                {
                    new Post
                    {
                        Title="Deploying .NET Core 3 apps as self-contained",
                        Content="Yay! .NET Core 3.0 is now available!  You now are migrating your apps and want to get it to your favorite cloud hosting solution"
                    },
                    new Post
                    {
                        Title="Skipping CI in GitHub Actions Workflows",
                        Content="One of the things that I like about Azure DevOps Pipelines is the ability to make minor changes to your code/branch but not have full CI builds happening."
                    }
                });

            blogList.Add(blog5);

            return blogList;
        }
    }

}
