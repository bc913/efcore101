# Relationships
- [Introduction](#introduction)
- [Optional vs. Required Relationships](#optional-vs.-required-relationships)
- [Fully Defined vs. Ambiguous Relationships](#fully-defined-vs.-ambiguous-relationships)
- [Manual Configuration](#manual-configuration)
- [Relationship Types](#relationship-types)
    - [One-to-one](#one-to-one)
    - [One-to-Many](#one-to-many)
    - [Many-to-Many](#many-to-many)
    - [Many-to-One](#many-to-one)
- [Resources](#resources)

## Introduction

*Relationship* stands for the interaction of a field of a record in a table with another field in another table. It also defines the way how the data is stored in a relational database and this is achieved with **Foreign Key** constraint. In order to understand relationships in EF Core context, it'd be better first to grasp [Definitions of Terms](<https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#definition-of-terms>) and 
[Conventions](<https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#conventions>) through *Microsoft*'s documentations.

> Conventions can be considered as default understanding of EF Core. THey can be overriden using `Configurations`.
## [Optional vs. Required Relationships](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#required-and-optional-relationships)
This controls whether the `Foreign Key` is required or optional. By default, the relationships are optional.

**Optional Relationship**:
- `Dependent` entites can be created and saved w/o `Principal`.
- When a `Principal` record is removed from the database, the corresponding `Dependent` records will not be removed from the database and the corresponding `Foreign Key`(s) will be set to `NULL`.

**Required Relationship**:
- To make relationship required, use `.IsRequired()`.
-  `Dependent` entities can be created w/o `Principal` but can NOT be saved to database w/o it.
- - When a `Principal` record is removed from the database, the corresponding `Dependent` records will be removed from the database.

To change the delete behavior of the dependent entity use `OnDelete()`

## Fully Defined vs. Ambiguous Relationships

### Fully defined relationships (unambigious)
If the relationships between two entities are unambigous, then there is no need to configure the relationships via Fluent API or Data Annotations. It can be in one way (single navigation) or bidirectional. Fully defined relationships can be achieved defining `navigation property` and `foreign key` at the same time in the dependent class.

```csharp
public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; }

    public List<Post> Posts { get; set; }
}

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public int BlogId { get; set; }
    public Blog Blog { get; set; }
}
```
However, this is not always required. EF Core can still infer using several types of conventions.
> Defining relationships in this way are always valid for any type of relationships.

#### [One way (Single navigation)](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#single-navigation-property)

```csharp
public class User
{
    public int Id { get; set;}
    public string Name { get; set;}
    public string LastName { get; set;}
    public List<Contact> ContactsCreated { get; set;}
}

public class Contact
{
    public int Id { get; set;}
    public string Name { get; set;}
    public string LastName { get; set;}
    public string PhoneNumer{get; set;}
}
```
You can also have a single navigation property and a foreign key property.

#### [Birdirectional Navigation](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#fully-defined-relationships)

This relationship is bidirectional between two entites through one `navigation property` for each entity. It is usually ( not neccesserly) in the following form:

```csharp
public class User
{
    public int Id { get; set;}
    public string Name { get; set;}
    public string LastName { get; set;}
    public List<Contact> ContactsCreated { get; set;}
}

public class Contact
{
    public int Id { get; set;}
    public string Name { get; set;}
    public string LastName { get; set;}
    public string PhoneNumer{get; set;}
    // detects the foreign key implicitly as User's primary key (id)
    public User CreatedBy { get; set;} 
}
```

In this case, the relationship is clear. `User` is **parent** or **principal** entity, while `Contact` is **child** or **dependent** entity. Their relationship is clear. `User.ContactsCreated` (collection) and `Contact.CreatedBy` ( created ) are navigational properties between each other. 

-  `User.ContactsCreated` and `Contact.CreatedBy` are inverse property of each other(by default).

There is no need to specify `Foreign Key` here because it is done implicity. To specify it manually, one can choose one of the following. For details, see [this](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#fully-defined-relationships). (again, this is not required for this relationship):

### Ambiguous relationships
As mentioned above, the relationsship is considered as unambigous ( fully defined) if there is only one pair of `navigation property` that points to each other. If there is more than one pair of navigation property that points each other for two entites then this is an ambigious relationship.

To resolve this, manual configuration is required.

## Manual configuration
It can be done through Fluent API ( advised) or Data Annotations. `Fluent API` style will be described here for several reasons:
- It helps to keep entity classes clean and independent of database provider type.
- Perfectly fits to the `Clean Code Architecture` concept.


## Relationship Types
Each relationship type can be defined using `conventions` or `manual configuration`. Convention style definitions can also be using manual configuration using `Fluent API`. Manual configurations with Fluent API is used for additional adjustments of relationships.
### One to one
When a single field in Table A is related to **only one** field in Table B. This can be considered as an answer of the question:
> How a single field in Table A is represented in Table B?

Features:
1. Dependent is always optional.
2. Dependent must always contain information about the principal end.

Check out [this](OneToOne.md) for different implementations (One-way vs. Bidirectional navigation) of One-to-One relationships.

#### CRUD Operations and consequences for One-to-One relationships

**Create**:

| Create    | Relationship Type | Navigation property definition required | Result |
| ------    | ----------------- | --------------------------------------- | ------ |
| Dependent | Optional          |  NO                                     | Can be saved to database w/ or w/o it's `Principal` (navigation) defined |
| Principal | Optional          |  NO                                     | Can be saved to database w/ or w/o it's `Dependent` (navigation) defined |
| Dependent | Required          |  YES                                    | Can NOT be saved to database w/o its `Principal` defined.|
| Principal | Required          |  NO                                     | Can be saved to database w/ or w/o it's `Dependent` (navigation) defined|

**Query**

There is no special exception here. Just query the desired model using common syntax. If one wants to query with `navigation property`, use `.Include()` to have it loaded into memory.

**Update**

There is no special exception here, too. Just query the desired model and update it accordingly. To update an existing `navigation` property on a model, you have to use `.Include()` to have an access to it. To generate a new value for `navigation`, no need to query with `.Include()`.
 
**Delete**


| Delete    | Relationship Type | Query           | Result |
| ------    | ----------------- | --------        | ------ |
| Dependent | Optional          |  w/o .Include() | Removes from database. No effect on Principal|
| Principal | Optional          |  **w/ .Include()**  | Removes from database. Sets FK value to `NULL` for the corresponding `Dependent` record(if exists any) - (ClientSetNull) |
| Dependent | Required          |  w/o .Include() | Removes from database. No effect on its `Principal`|
| Principal | Required          |  w/o .Include() | Removes from database. Also, removes the corresponding record for `Dependent` (if exists any) - (Cascade)|

To change the default delete behavior, use `OnDelete()` method in `OnModelCreating()`. Check out [Microsoft's documentation](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#cascade-delete) for details.


### One to Many
A single record in Table A is related to one or multiple records in Table B. See the following example:
Customer Table:

|Id |Name |
|---|-----|
| 1 |John |
| 2 |Julia|

Video Game Rental Table: 

| Id | Name|CheckOutDate|CustomerId(FK)|
| --- |----|------------|--------------|
|1| Call of Duty | 23032020 | 1|
|2| Batman | 05012020 | 1|
|3| NBA Live | 01012020 | 2|

A customer can check out multiple video games but one video game can be checked out by ONLY one customer. 

Let's start coding. I'll be following the one-to-many example available in Microsoft's documentation with some tweaks. We can define `one-to-many` relationship in different ways. I'll be showing two of them.

[**Foreign Key on dependent and no navigation property**](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#without-navigation-property)
```csharp
//==================
// Convention
//==================
public class Blog // Parent
{
    public Guid Id { get; set; }
    public string Url { get; set; }
}
public class Post // Child
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public Guid BlogId { get; set;}
}
//==================
// Manual Configuration
//==================
public class OtmContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>()
           .HasOne<Blog>()
           .WithMany()
           .HasForeignKey(p=>p.BlogId);
    }
}
```

[**Single navigation property on principal**](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#single-navigation-property-1)

Child needs NO references back to the parent. EF Core understands 1:* relationship.

```csharp
//==================
// Convention
//==================
public class Blog // Parent
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public List<Post> Posts { get; private set; }
}
public class Post // Child
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    // No navigation property
}

// At this point, the relationship is optional. Use Fluent API for required relationship.

//==================
// Manual Configuration
//==================
/*
*/
public class OtmContext : DbContext

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>()
           .HasMany(b => b.Posts)
           .WithOne()
           .IsRequired();
    }
}
```
[**No Foreign Key (BiDirectional navigation - properties on both ends)**](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#manual-configuration)

If no foreign key property is found, a shadow foreign key property will be introduced with the name `<navigation property name><principal key property name> or <principal entity name><principal key property name>` if no navigation is present on the dependent type.
```csharp
//==================
// Convention
//==================
public class Blog //Parent
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public List<Post> Posts { get; private set; } = new List<Post>();
}
public class Post //Child
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public Blog Blog { get; set; }
}
// At this point, the relationship is optional. Use Fluent API for required relationship.

//==================
// Manual Configuration
//==================
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Add the shadow property to model
    // modelBuilder.Entity<Post>()
    //     .Property<Guid>("BlogForeignKey");

    modelBuilder.Entity<Post>()
        .HasOne(p => p.Blog)
        .WithMany(b => b.Posts)
        /*.HasForeignKey("BlogForeignKey") // if shadow property is added*/
        .IsRequired();
}
```



[**Foreign Key and bidirectional navigation**](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#manual-configuration)
```csharp
// Entities
public class Blog //Parent
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    // Always initialize collections to an empty one.
    public List<Post> Posts { get; private set; } = new List<Post>();
}
public class Post //Child
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    // Navigation property. EF Core allows you to navigate
    // from child (Post) to parent (Blog)
    public Blog Blog { get; set; }
    public Guid BlogId { get; set; } // Foreign Key (Parent's primary key)
}

// Context
public class OtmContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Blog)
            .WithMany(b => b.Posts);
            .HasForeignKey(p=>p.BlogId);// This is optional if Foregin key name matches the pattern <principal_entity>_Id. Use this if you have custom names foreign key
        // Or
        //modelBuilder.Entity<Blog>()
        //    .HasMany(b => b.Posts)
        //    .WithOne(p => p.Blog);
    }
}

```
### Many to Many
One record in Table A is related to one or multiple records in Table B and vice versa. In order to achieve this behavior, a `join` or `link` table should be provided.
- **Before EF Core 5.0**:
```csharp
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

// Join entity
public class BookAuthorLink
{
    public Guid BookId { get; set; }
    public Book Book { get; set; }

    public Guid AuthorId { get; set; }
    public Author Author { get; set; }
}
```

As this code snippet states, a `Book` can have multiple `Author`s and an `Author` can author multiple `Book` s. In order to setup Many-to-Many relationship, following Fluent API configuration should be done.

```csharp
public class MtmContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }

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
```
The `Foreign Key` s are defined in `join` entity. For this relationship, I've not seen any usage of `Required` or `Optional` relationship configuration.

- **After EF Core 5.0**:
    1. Default Convention (Skip Navigation): There is no requirement to create a seperate class for join.

```csharp
public class Book
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public List<Author> Authors { get; set; } = new List<Author>();
}

public class Author
{
    public Guid Id { get; set; }

    public string FullName { get; set; }

    public List<Book> Books { get; set; } = new List<Book>();
}

// EF Core generates the join table under the hood.
public class MtmContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
}
```

2. Explicit definition of Join Table with additional data (Payload)

```csharp
public class Book
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public List<Author> Authors { get; set; } = new List<Author>();
}

public class Author
{
    public Guid Id { get; set; }

    public string FullName { get; set; }

    public List<Book> Books { get; set; } = new List<Book>();
}
// Explicit definition
public class BookAuthor
{
    public Guid BookId { get; set; }
    public Guid AuthorId { get; set;}
    // Payload
    public DateTime DateJoined { get; set; }
}

// When additional payload exists, the default construction of many-to-many relationship does not occur. You have to define explicitly.
public class MtmContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Author>
            .HasMany(a => a.Books)
            .WithMany(b => b.Authors)
            .UsingEntity<BookAuthor>
            (
                ba=>ba.HasOne<Author>().WithMany(),
                ba=>ba.HasOne<Book>().WithMany()
            )
            .Property(ba=>ba.DateJoined)
            .HasDefaultValueSql("getdate()");
    }
}


```


### Many to One
```csharp
//==================
// Convention
//==================
public class Student //dependent
{
    private Student() { }
    
    // Ctor arguments should have the same name with the properties
    // Otherwise EF Core will throw
    public Student(string name, string email, Course favoriteCourse)
    {
        Name = name;
        Email = email;
        FavoriteCourse = favoriteCourse;
    }

    public long Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public Course FavoriteCourse { get; private set; }
}

public class Course //Principal
{
    public long Id { get; set; }
    public string Name { get; set; }
}

// At this point Convention leads to one to one relationship so Manual Configuration
// is needed for many to one
//==================
// Manual Configuration
//==================
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Student>(s =>
    {
        s.ToTable("Student").HasKey(k => k.Id);
        s.HasOne(p => p.FavoriteCourse)
            .WithMany();
            //.IsRequired() for required relationships
    });
}

/*
If the relationship optional, deleting the course will mark the course id in student table row as null.
if the relationship is required, deleting the course will remove the enrolled students, too.
*/
```
## Misc.
- If non-nullable foreign key types (i.e. `int`) is used, then the instance of the container class (dependent) can NOT persist without the corresponding independent class.
```csharp
public class Parent
{
    public int Id { get; private set; }
    // other properties & fields
}

public class Child
{
    public int Id { get; private set; }
    // Some other properties or fields

    // int non-nullable so Child can NOT persist without Parent instance
    public int ParentId { get; set; }
}
```
- If parent is defined as navigation property, then Child instance can persist without the parent because the type Parent is nullable.
## References
https://github.com/dotnet/efcore/issues/10084#issuecomment-336947129

https://docs.microsoft.com/en-us/ef/core/get-started/?tabs=visual-studio

https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-composite-key%2Csimple-key#other-relationship-patterns

//https://blog.oneunicorn.com/2017/09/25/many-to-many-relationships-in-ef-core-2-0-part-1-the-basics/




