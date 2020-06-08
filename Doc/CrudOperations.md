# CRUD Operations
`CRUD` Operations for EF Core is handled under `Query` and `Save` topics. This section provides a general introduction. The side effects of these operations might differ based on `Relationship Types` and their configuration. Please refer to [Relationship Types](https://github.com/bc913/efcore101/blob/master/Doc/Relationships.md#relationship-types) section for details.
## Query Data
### a. Loading Related Data

There are three types of loading: Eager, Explicit and Lazy loading

#### Eager Loading
The related data is loaded from the database when requested as part of the query. Unless it is requested, the navigation property ( related) data is not loaded. 

The first (closest) navigation property can be loaded within the entity using `Include()` as follows:
```csharp
var blogs = context.Blogs
    .Include(b => b.Posts)
    .ToList();
```

Further levels of related data can be loaded in a chain through `ThenInclude()`. Check [this](<https://docs.microsoft.com/en-us/ef/core/querying/related-data#including-multiple-levels>)

```csharp
var blogs = context.Blogs
    .Include(blog => blog.Posts)
        .ThenInclude(post => post.Author)
    .ToList();
```

> UPDATE(.NET Core 5.0) : [Filtered Include](<https://docs.microsoft.com/en-us/ef/core/querying/related-data#filtered-include>) is available.

#### Explicit Loading
Related entities can be loaded through `DbContext.Entry(...)`.
```csharp
var blog = context.Blogs
        .Single(b => b.BlogId == 1);
// Load blog variable's Posts
context.Entry(blog)
    .Collection(b => b.Posts)
    .Load();
```

Qerying related entities can be done w/ or w/o loading into the memory.
```csharp
var blog = context.Blogs
    .Single(b => b.BlogId == 1);

// w/o loading into the memory
var postCount = context.Entry(blog)
    .Collection(b => b.Posts)
    .Query()
    .Count();

// Loaded into memory.
var goodPosts = context.Entry(blog)
    .Collection(b => b.Posts)
    .Query()
    .Where(p => p.Rating > 3)
    .ToList();
```


#### Tips
- Always prefere eager loading

### b. Client vs Server Evaluation

EF Core framework supports partial evaluation of the queries based on where they have been done: client-side vs. server-side. This means that a part of the query can be done on the client side while some other part of the query can be evaluated on the server side. 

As long as the specific part ( or all parts) of the query can be translated into a valid query expression for the corresponding database provider, it is evaluated on the `server` side.

```csharp
public void FilteringWithServerSideEvalutaion()
{
    using(var context = new OtmContext(ContextOptions))
    {
        var blogs = context.Blogs
            .AsNoTracking()
            .Where(b => b.Url.Contains("blog"))
            .ToList();
    }
}
```

```csharp
public void AnonymousProjectionWithServerSideEvaluation()
{
    using(var context = new OtmContext(ContextOptions))
    {
        var blogs = context.Blogs
            .AsNoTracking()
            .Select(b => new
            {
                Guid = b.Id,
                Url = b.Url
            })
            .ToList();
    }
}
```

If the query expression is complex enough for the database provider to evaluate in `server` side, then the evaluation is done on the `side` as follows: Since **IsValidUrl()** is user defined methods, `server` side has no idea about how this method is implemented so EF Core would implicity fetches all the blogs to the memory and filtering will be done as in-memory. This is a performance issue.

> **UPDATE** (EF Core 3.0 and later): Following implementation will throw run time exceptions since it can not be translated into single SQL query.

```csharp
public void FilteringWithClientSideEvalutaion()
{
    using(var context = new OtmContext(ContextOptions))
    {
        var blogs = context.Blogs
            .AsNoTracking()
            .Where(b => IsValidUrl(b.Url))
            .ToList();
    }
}
```

[As stated in Microsoft documentation](<https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/#restricted-client-evaluation>): `To evaluate a predicate condition on the client as in the previous example, developers now need to explicitly switch evaluation of the query to LINQ to Objects:`
```csharp
var blogs = context.Blogs
    .Where(b=>b.Url.EndsWith("com"))
    .AsEnumerable()//LINQ to Objects
    .Where(b=>IsValidUrl(b.Url));
```

Unlike the abovementioned example, top-level projections (essentially the last call to `Select()`) will not throw exceptions but the evaluation will be handled in the client side. This is a performance issue still.

```csharp
public void AnonymousProjectionWithClientSideEvaluation()
{
    using(var context = new OtmContext(ContextOptions))
    {
        var blogs = context.Blogs
            .AsNoTracking()
            .Select(b => new
            {
                Guid = b.Id,
                Url = StandardizeUrl(b.Url)
            })
            .ToList();
    }
}
```

### c. Tracking
If the queries are made for read-only operations, then, it is better to turn off tracking during query as follows:
```csharp
var blogs = context.Blogs
    .AsNoTracking()
    .Where(b => b.Url.Contains("blog"))
    .ToList();
```

or tracking can be disabled on the instance level
```csharp
context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
var blogs = context.Blogs.ToList();
```

#### Identity resolution
No tracking queries don't use the change tracker and don't do idendity resolution so you get back new instance of entity even when the same entity is contained in the result multiple times.

#### Tracking and custom projections
Even if the result of the query isn't an entity type, EF Core will still track enitity types if the entity type is fully defined as follows:

```csharp
// 1
var blogs = context.Blogs
    .Select(b => new
    {
        Blog = b,
        PostCount = b.Posts.Count
    });

// 2
var blog = context.Blogs
    .Select(b =>
        new
        {
            Blog = b,
            Post = b.Posts.OrderBy(p => p.Rating).LastOrDefault()
        });
// 3
var blogs = context.Blogs
    .OrderByDescending(blog => blog.Rating)
    .Select(blog => new
    {
        Id = blog.BlogId,
        Url = StandardizeUrl(blog)
    })
    .ToList();
```

If the result set does not contained any entity types fully, tracking is disabled implicitly:
```csharp
var blogs = context.Blogs
    .Select(b => new
    {
        Guid = b.Id,
        Url = b.Url
    })
    .ToList();
```



### d. Query Tags
The queries can be tagged to be captured in logs for convenience.
```csharp
var blogs = context.Blogs.TagWith("Tagged Query")
            .Include(b => b.Posts)
            .ToList();
```

### e. Global Query filters
Refer to [Microsoft documentation](https://docs.microsoft.com/en-us/ef/core/querying/filters) for details.

### f. Tips
- Use projections where possible. (i.e through `Select()` LINQ statement )
- Avoid client side evaluations
- Do NOT fetch the data until needed. If there are intermediate steps before fetching data, create `IQueryable` object first, run intermediate steps and then execute query.
```csharp
var query = context.Blogs.Select(b => new
    {
        Guid = b.Id
    }).AsQueryable();
// Do some stuff
var selectedBlogs = query.ToList();

//or
IQueryable<Friend> GetNearestFriends(Point myLocation) =>
    from f in context.Friends.TagWith("GetNearestFriends")
    orderby f.Location.Distance(myLocation) descending
    select f;

IQueryable<T> Limit<T>(IQueryable<T> source, int limit) =>
    source.TagWith("Limit").Take(limit);
```
- Make projections/includes reusable( by wrapping them around Expression/IQueryable)
- Use `async/await`
- Use async versions of LINQ methods for responseviness.

### e. Resources
https://levelup.gitconnected.com/3-ways-to-improve-the-ef-core-performance-in-your-net-core-app-d9b6295188cc
http://www.samulihaverinen.com/web-development/dotnet/2018/06/04/EF-core-2-1-best-practices/
https://entityframeworkcore.com/querying-data-projection
https://benjii.me/2018/01/expression-projection-magic-entity-framework-core/

https://itnext.io/entity-framework-core-string-filter-tips-768139b55ffd
https://www.thereformedprogrammer.net/entity-framework-core-client-vs-server-evaluation/
https://medium.com/@yostane/using-entity-framework-plus-to-easily-batch-database-requests-5aaabf93ca62


## Save Data
### a. Overview
Each context instance has a `ChangeTracker` to keep track of changes that need to be written to the database when `SaveChanges` (or its `Async` equivalent) method is executed.

```csharp
using(var context = new OtmContext())
{
    // Add
    var blog1 = new Blog { Url = "http://blogs.msdn.com/adonet" };
    context.Blogs.Add(blog1);
    context.SaveChanges();

    // Update
    var blog = context.Blogs.First();
    blog.Url = "http://example.com/blog";
    context.SaveChanges();

    // Delete
     var blog = context.Blogs.First();
    context.Blogs.Remove(blog);
    context.SaveChanges();
}
```
Multiple Add/Update/Delete operations can be wrapped under single `SaveChanges()`. `SaveChanges()` is transactional for most of the database providers.

### b. Related Data
#### Add
Related entities can be added in different ways
```csharp
// 1
// Posts are also populated in the database
var blog2 = new Blog { Url = "https://www.microsoft.com" };
blog2.Posts.AddRange(
    new List<Post>
    {
        new Post { Title = "My first app", Content = "I wrote an app using EF Core 1" },
        new Post { Title = "My second app", Content = "I wrote an app using EF Core 2" }
});
context.Blogs.Add(blog2);
context.SaveChanges();

// 2
// The blog and post are populated tpgether
var blog3 = new Blog { Url = "https://www.microsoft.com" };
var post3 = new Post { Title = "EF Core Tutorial", Content = "Getting started with EF Core", Blog = blog3 };
context.Posts.Add(post3);
context.SaveChanges();

```
#### Update
During a query, by default, related entities are not loaded. To load the related data, use `Include()` and `ThenInclude()`. Updating the related data can be done as follows:

```csharp
// 1
var blog = context.Blogs.Include(b => b.Posts).First();
var post = new Post { Title = "Intro to EF Core" };

blog.Posts.Add(post);
context.SaveChanges();

// 2
var post2 = context.Posts.Include(p=>p.Blog).First();
var blog2 = new Blog {Url = "www.nba.com"};
post2.Blog = blog2;
context.SaveChanges();
```
#### Remove

```csharp
//1
// This will remove Blog record only. The related Posts (if any exist) Blog property will be save to null
var foundBlog1 = context.Blogs.First(b => b.Url == blog1.Url);
context.Blogs.Remove(foundBlog1); // foundBlog1 = null;
context.SaveChanges();

//2 
// This will only remove the Post record. The related entity, Blog, will remain in db.
var foundPost3 = context.Posts.First(p => p.Title == "EF Core Tutorial");
context.Posts.Remove(foundPost3); // foundPost3 = null
context.SaveChanges();
```

Removing whether a child or parent entity, does NOT mean removal of it's related entity unless one of the following conditions are satisfied:
- Definition of required relationship
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Post>()
        .HasOne(p => p.Blog)
        .WithMany(b => b.Posts)
        .IsRequired();
}
```
- Explicity stated `Cascade Delete` option.
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Post>()
        .HasOne(p => p.Blog)
        .WithMany(b => b.Posts)
        .OnDelete(DeleteBehavior.Cascade);
}
```
Check this [link](<https://docs.microsoft.com/en-us/ef/core/saving/cascade-delete>) for further information on `Cascade Delete`.

### c. Transactions
By default, if the database provider supports, all changes in a single call to `SaveChanges()` are applied in a transaction. Check this [link](https://docs.microsoft.com/en-us/ef/core/saving/transactions) for details and this [repo](https://docs.microsoft.com/en-us/ef/core/saving/transactions) for samples.

### d. State of the entity
One can use `DbContext` instance method `Entry` on a given entity instance to observe current state of that entity in various ways.

```csharp
using (var context = new BlogsContext())
{
    var blog = context.Blogs.First();

    blog.Title = "Two";

    foreach (var entry in context.Entry(blog).Properties)
    {
        Console.WriteLine(
            $"Property '{entry.Metadata.Name}'" +
            $" is {(entry.IsModified ? "modified" : "not modified")} " +
            $"Current value: '{entry.CurrentValue}' " +
            $"Original value: '{entry.OriginalValue}'");
    }
}
```

Check this [link](<https://github.com/dotnet/efcore/issues/9237#issuecomment-317115094>) for abovementioned sample.




