# Relationships
- [Introduction](#introduction)
- [Optional vs. Required Relationships](#optional-vs.-required-relationships)
- [Fully Defined vs. Ambiguous Relationships](#fully-defined-vs.-ambiguous-relationships)
- [Relationship Types](#relationship-types)
    - [One-to-one](#one-to-one)
    - [One-to-Many](#one-to-many)
    - [Many-to-Many](#many-to-many)
- [Resources](#resources)

## Introduction

*Relationship* stands for the interaction of a field of a record in a table with another field in another table. It also defines the way how the data is stored in a relational database and this is achieved with **Foreign Key** constraint. In order to understand relationships in EF Core context, it'd be better first to grasp [Definitions of Terms](<https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#definition-of-terms>) and 
[Conventions](<https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#conventions>) through *Microsoft*'s documentations.

> Conventions can be considered as default understanding of EF Core. THey can be overriden using `Configurations`.
### [Optional vs. Required Relationships](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#required-and-optional-relationships)
This controls whether the `Foreign Key` is required or optional. By default, the relationships are optional.

**Optional Relationship**:
- `Dependent` entites can be created and saved w/o `Principal`.
- When a `Principal` record is removed from the database, the corresponding `Dependent` records will not be removed from the database and the corresponding `Foreign Key`(s) will be set to `NULL`.

**Required Relationship**:
- To make relationship required, use `.IsRequired()`.
-  `Dependent` entities can be created w/o `Principal` but can NOT be saved to database w/o it.
- - When a `Principal` record is removed from the database, the corresponding `Dependent` records will be removed from the database.

To change the delete behavior of the dependent entity use `OnDelete()`

### Fully Defined vs. Ambiguous Relationships

#### Fully defined relationships (unambigious)
If the relationships between two entities are unambigous, then there is no need to configure the relationships via Fluent API or Data Annotations. It can be in one way (single navigation) or bidirectional

##### [One way (Single navigation)](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#single-navigation-property)

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

##### [Birdirectional Navigation](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#fully-defined-relationships)

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

#### Ambiguous relationships
As mentioned above, the relationsship is considered as unambigous ( fully defined) if there is only one pair of `navigation property` that points to each other. If there is more than one pair of navigation property that points each other for two entites then this is an ambigious relationship.

To resolve this, manual configuration is required.

#### Manual configuration
It can be done through Fluent API ( advised) or Data Annotations. `Fluent API` style will be described here for several reasons:
- It helps to keep entity classes clean and independent of database provider type.
- Perfectly fits to the `Clean Code Architecture` concept.


### Relationship Types
#### One to one
When a single field in Table A is related to **only one** field in Table B. This can be considered as an answer of the question:
> How a single field in Table A is represented in Table B?

In EF Core context, each entity is associated to each other through single navigation property. Let's assume we have `Student` and `Address` entities. Property `Id` is `Principal Key (PK)` for both entities by default. 
```csharp
// Entities are simplified to focus better to the topic.

public class Student
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Address Address { get; set; }

}

public class Address
{
    public Guid Id { get; set; }

    public string City { get; set; }

    public Student Student { get; set; }

}

```
In order to communicate with database, we have to define our custom `DbContext` class and provide so-called `repositories` in the form of `DbSet<entity_type>`.

```csharp
public class OtoContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public OtoContext(DbContextOptions options) : base(options) {}
}
```
Now, we have to define the relationship. I prefer using `Fluent API` to configure relationships for several reasons. First, it has more control than `Data Annotations` does and is better suited to `Clean Architecture`. Since, definition of relationship is a data access layer operation, it is always better to keep it seperate from the data.

Since each side of the relationship can be `principal` or  '`dependent`, `One-to-one` relationship between two entities can be achieved in several ways. We can assume that there are two types of `one-to-oone` relationships: `Optional` and `Required`

| Case    | Principal  Entity | Dependent Entity | Type     |
| ------- | ----------------- | ---------------- | -------- |
|    1    | Student           | Address          | Optional |
|    2    | Address           | Student          | Optional |
|  1-Req  | Student           | Address          | Required |
|  2-Req  | Address           | Student          | Required |

- Case 1:

```csharp
public class OtoContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // One to one relationship
        modelBuilder.Entity<Entities.Student>()
            .HasOne(s => s.Address)
            .WithOne(a => a.Student)
            .HasForeignKey<Address>("StudentId"); // Address has foreign key named StudentId(this is shadow)
    }
}
```
**Dependent Entity (Child)**: Address - PK(Id)


**Principal Entity (Parent)**: Student - PK(Id)


**Foreign Key** : StudentId - not defined but it is implicitly there by convention, shadow (Student + Id)


**Navigation Property on Principal entity**: Address


**Navigation Property on Dependent entity**: Student

- Case 1-Req:
```csharp
public class OtoContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // One to one relationship
        modelBuilder.Entity<Entities.Student>()
            .HasOne(s => s.Address)
            .WithOne(a => a.Student)
            .HasForeignKey<Address>("StudentId") // Address has foreign key named StudentId(this is shadow)
            .IsRequired(); // StudentId is required for Address entity to exist
    }
}
```
`Case 1-Req` has the same feature as `Case 1` has.

- Case 2:

```csharp
public class OtoContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // One to one relationship
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Address)
            .WithOne(a => a.Student)
            .HasForeignKey<Student>("AddressId"); // Student has foreign key named AddressId(this is shadow)
    }
}
```
**Dependent Entity (Child)**: Student - PK(Id)


**Principal Entity (Parent)**: Address - PK(Id)


**Foreign Key** : AddressId - not defined but it is implicitly there by convention, shadow (Address + Id)


**Navigation Property on Principal entity**: Student


**Navigation Property on Dependent entity**: Address

- Case 2-Req:

```csharp
public class OtoContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // One to one relationship
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Address)
            .WithOne(a => a.Student)
            .HasForeignKey<Student>("AddressId") // Student has foreign key named AddressId(this is shadow)
            .IsRequired(); // AddressId FK is required for Student entity to exist
    }
}
```
`Case 2-Req` has the same feature as `Case 2` has.

However, there are different consequences of `Optional` and `Required` `one-to-one` relationship on `CRUD` operations.

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


#### One to Many
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

[**Option 1: Implicit Shadow Foreign Key**](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#manual-configuration)
```csharp
// Entities
public class Blog
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public List<Post> Posts { get; private set; }
}
public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public Blog Blog { get; set; }
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
        // Or
        //modelBuilder.Entity<Blog>()
        //    .HasMany(b => b.Posts)
        //    .WithOne(p => p.Blog);
    }
}

```
[**Option 2: Explicit Shadow Foreign Key**](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#shadow-foreign-key)
```csharp
// Entities
public class Blog
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public List<Post> Posts { get; private set; }
}
public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public Blog Blog { get; set; }
}

// Context
public class OtmContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add the shadow property to model
        modelBuilder.Entity<Post>()
            .Property<Guid>("BlogForeignKey");

        // Use it as foreign key
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Blog)
            .WithMany(b => b.Posts)
            .HasForeignKey<Post>("BlogForeignKey");
    }
}
```

[**Option 3: No navigation property on dependent**](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#single-navigation-property-1)
```csharp
// Entities
public class Blog
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public List<Post> Posts { get; private set; }
}
public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    // No navigation property
}

// Context
public class OtmContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>()
           .HasMany(b => b.Posts)
           .WithOne();
    }
}
```

[**Option 4: With defined Foreign Key ( No navigation property on both ends)**](https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#without-navigation-property)
```csharp
// Entities
public class Blog
{
    public Guid Id { get; set; }
    public string Url { get; set; }
}
public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public Guid BlogId { get; set;}
}

// Context
public class OtmContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>()
           .HasOne<Blog>()
           .WithMany()
           .HasForeignKey(p=>p.BlogId);
    }
}
```

As one might have noticed, I prefer not using an additional property to define `Foreign Key` in the dependent entity. It is a database layer info and should not exist in our application logic unless we have to. I also don't find it meaningful to name identifier property as `<entity_name>Id` for obviuos reasons :). 

#### Many to Many
One record in Table A is related to one or multiple records in Table B and vice versa. In order to achieve this behavior, a `join` or `link` table should be provided.

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



### Resources
https://github.com/dotnet/efcore/issues/10084#issuecomment-336947129

https://docs.microsoft.com/en-us/ef/core/get-started/?tabs=visual-studio

https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-composite-key%2Csimple-key#other-relationship-patterns

//https://blog.oneunicorn.com/2017/09/25/many-to-many-relationships-in-ef-core-2-0-part-1-the-basics/




