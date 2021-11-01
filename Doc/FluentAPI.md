# Fluent API

- Custom column name:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Course>(c=>
    {
        c.Property(p=>p.IsActive)
            .HasColumnName("Is_active");
        c.Property(p=>p.NumberOfStudents)
            .HasColumnName("Students_enrolled");
    });
}
```