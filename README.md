# Entity Framework Core Tutorial
## Introduction
This repo provides an introduction to Entity Framework Core and it's basic usage with documentation.
### What is EF Core?
As [Microsoft documentation](https://docs.microsoft.com/en-us/ef/core/) states

> Entity Framework (EF) Core is a lightweight, extensible, open source and cross-platform version of the popular Entity Framework data access technology. EF Core can serve as an object-relational mapper (O/RM), enabling .NET developers to work with a database using .NET objects, and eliminating the need for most of the data-access code they usually need to write.

## Documentation
This documentation mainly has a similar categorization as done in Microsoft's official documentation. I've gathered my notes with some examples as follows:

- [Fundamentals](Doc/Fundamentals.md)
- [Relationships](Doc/Relationships.md)
- [CRUD Operations](Doc/CrudOperations.md)
- [Testing](Doc/Testing.md)

## Repository
A sample EF Core repository is provided. It is tested on Windows OS. SQLite database provider is used. If you want to access database samples, I suggest using `DB Browser for SQLite`. Click [here](https://sqlitebrowser.org/) for details. 

The repository is categorized based on the relationship types and corresponding unit tests are provided. Some might notice that `DbContext` classes are designed as to be parametric based on being `Optional` or `Required`. This is not an ideal way designing `DbContext` classes for production and is only provided for training purposes.

One can run each relationship type example as console application by commenting out the corresponding code snippet in `Program.cs`. I've tried to demonstrate each `CRUD` operation and their side effect for `Optional` and `Required` relationships where possible.

Unit tests are also provided to demonstrate some of the `CRUD` behaviors and their side effects in an isolated way. In-memory SQLite database provider is used for testing purposes. Nunit framework is used.

> Runing all unit tests at once using `Test Explorer` might not work well. You might have to run the unit tests one by one where required.

`RunMigration.ps1` Powershell script may not work correctly. I'll fix it soon.

### Requirements
- .Net Core 3.1
- .Net Core CLI
- NUnit