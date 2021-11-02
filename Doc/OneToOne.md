# One-to-One Relationship
- The requiredness of the relationship is defined by the existence of the foreign key:
    1. When dependent entity has a foregin key of the parent (w or w/o reference to parent):
        - if the key is non-nullable(int, Guid), the relationship is required.
        - if the key is nullable, the relationship is optional.
    2. When dependent entity has no foreign key:
        - If the dependent and parent entity has references to each other, this is ambigious. Manual Configuraiton is required.
        - 
## Single Navigation
In this type of navigation, only the dependent end has information about the principal end.

### Foreign Key (Skip Navigation Property)
```csharp
// 
//==================
// Convention
//==================
/*
- Optional by default
- An Address instance can NOT be persisted w/o a Student instance (Child)
- A Student instance can be persisted w/o an Address instance (Parent)
- No cascade delete. Deleting parent(student) will not delete the address and keeps the guid of the removed parent in the 
child table
*/
public class Address
{
    public Guid Id { get; set; }
    public string City { get; set; }
    public Guid StudentId { get; set; } // Foreign key
}

public class Student
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
}

// This populates the StudentId with Empty Guid if no value is given to the StudentId

//==================
// FLUENT API
//==================
/*
- REquired 
- An Address instance can NOT be persisted w/o a Student instance (Child)
- A Student instance can be persisted w/o an Address instance (Parent)
- Cascade delete
*/
// This defines the required one to one relationship by marking Address as child
// and Student as parent 
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Address>()//Child
        .HasOne<Student>()//Parent (Populate the argument if a reference from child to parent exists i.e. a=>a.Student)
        .WithOne() //Child (Populate the argument if a reference from parent to child exists i.e. s=>s.Address)
        .HasForeignKey(p => p.StudentId);// This line is optional.
        //IsRequired is useless here since we are fdriving the logic using foreign keys
        // IsRequired(false) will throw
}
```

### Navigation Property
```csharp
// 
//==================
// Convention
//==================
/*
- Optional by default
- An Address instance can NOT be persisted w/o a Student instance (Child)
- A Student instance can be persisted w/o an Address instance (Parent)
- No cascade delete. Deleting parent(student) will not delete the address and marks the parent in the child table as null
*/
public class Address
{
    public Guid Id { get; set; }
    public string City { get; set; }
    public Student Student { get; set; }
}

public class Student
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
}

// This populates the StudentId with null in the Address table if no value is given to the Student reference

//==================
// FLUENT API
//==================
/*
- Optional
- An Address instance can be persisted w/o a Student instance (Child)
- A Student instance can be persisted w/o an Address instance (Parent)
- No Cascade delete
*/
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
   modelBuilder.Entity<Address>()
        .HasOne<Student>(a=>a.Student)
        .WithOne()
        .HasForeignKey<Address>("StudentId"); // Address is using "Student" + "Id" as foreign key (shadow)
}

/*
- Required
- An Address instance can NOT be persisted w/o a Student instance (Child)
- A Student instance can be persisted w/o an Address instance (Parent)
- Cascade delete
*/
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
   modelBuilder.Entity<Address>()
        .HasOne<Student>(a=>a.Student)
        .WithOne()
        .HasForeignKey<Address>("StudentId") // Address is using "Student" + "Id" as foreign key (shadow)
        .IsRequired();
}
```