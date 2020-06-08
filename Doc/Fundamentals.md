# Fundamentals
Some keypoints are covered here.

## Entity
It can be considered as in-memory representation of the of a database record. The type of a **DbSet** is referred as *entity* and it is included EF Core model by default. It stores data and usually does not include bussiness logic in method form. 

If one wants to exclude your 
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Ignore< <entity_type_to_ignore> >();
}
```

## Relationships
Please check [Relationships](Relationships.md) section for details.

## Keys
*Key* is the unique identifier an EF Core entity. Any property in an entity can be defined as key.

**Primary Key** is the unique identifier for a record in a relational database table. As [Microsoft documentation](<https://docs.microsoft.com/en-us/ef/core/modeling/keys?tabs=data-annotations>) states:

> By convention, a property named `Id` or `<type name>Id` will be configured as the primary key of an entity.

It can also be defined explicitly through `Data Annotation` or ` Fluent API`. 
```csharp
// Fluent API
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Student>().HasKey(u => u.Id);
}
 
```

Multiple properties can be configured as primary key which is called `composite key`. This can be done through `Fluent API`.
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Student>()
    .HasKey(s=> new {s.Id, s.Name});
}
``` 

The type of the `Primary Key` is usually selected as `int`, `System.Guid` or `byte[]`.

**Foreign Key** is an identifier that provides the link between two tables. It is usually the `Primary Key` of the other end of the relationship. It can be defined in several ways and that definition depends on the relationship. It will be explained in `Relationships` section.

## Indexes
- Better for faster queries but when it is optimized well. Not everytime.
- Requires additional storage to hold the pointer information for the table it points to
- Efficient when having an index on Foreign KEy for join situations
> **UPDATE** (EF Core 3.0 and after): The default property access mode changed from `PreferFieldDuringConstruction` to `PreferField`.

## Shadow Properties
A property can be stored in the change tracker, rather than in the entity's CLR type. This type of property is called `Shadow Property`.

## Field Only Properties
You can also create a conceptual property in your model that does not have a corresponding CLR property in the entity class, but instead uses a field to store the data in the entity. Field-only properties are commonly used when the entity class uses methods instead of properties to get/set values, or in cases where fields shouldn't be exposed at all in the domain model (e.g. primary keys). 

## Value Comparers
It is recommended that you use immutable types (classes or structs) with value converters when possible. This is usually more efficient and has cleaner semantics than using a mutable type.

## Owned Type
n order to understand how EF Core tracks these objects, it is useful to know that a primary key is created as a shadow property for the owned type. The value of the key of an instance of the owned type will be the same as the value of the key of the owner instance

Owned types need a primary key. If there are no good candidates properties on the .NET type, EF Core can try to create one. However, when owned types are defined through a collection, it isn't enough to just create a shadow property to act as both the foreign key into the owner and the primary key of the owned instance, as we do for OwnsOne: there can be multiple owned type instances for each owner, and hence the key of the owner isn't enough to provide a unique identity for each owned instance.

[Limitations](https://docs.microsoft.com/en-us/ef/core/modeling/owned-entities#limitations)

## Entity Constructors
Use no ctor defined or public or protected ctors with arguments to initialize non navigation properties with private set.
- Injecting services: Only EFCore supported services can be injected

## Resources
[Microsoft documentation](https://docs.microsoft.com/en-us/ef/core/modeling/)

 Jing Wang - [Database Index](https://www.youtube.com/watch?v=uyLy462Fmk8)

 StackOverflow [question](https://stackoverflow.com/questions/1108/how-does-database-indexing-work)

 Jimmy Farillo - [The Basics of Database Indexes For Relational Databases](https://medium.com/@jimmyfarillo/the-basics-of-database-indexes-for-relational-databases-bfc634d6bb37)


