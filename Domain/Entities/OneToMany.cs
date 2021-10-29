using System;
using System.Collections.Generic;
using System.Text;

namespace Bcan.Efpg.Domain.Entities
{
    //In this example the shadow foreign key is BlogId because prepending the navigation name would be redundant.
    public class Blog
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public List<Post> Posts { get; private set; } = new List<Post>();
    }
    public class Post
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Blog Blog { get; set; } // This is optional
    }
}
