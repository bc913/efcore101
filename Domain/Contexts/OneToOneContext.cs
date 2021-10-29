using Microsoft.EntityFrameworkCore;
using Bcan.Domain.Entities;

namespace Bcan.Domain.Contexts
{
    public enum PrincipalType
    {
        Student,// Student is parent
        Address // Address is parent
    }

    // One-to-one relationship
    public class OtoContext : DbContext
    {

        public DbSet<Student> Students { get; set; }
        public DbSet<Address> Addresses { get; set; }

        private readonly PrincipalType _type;
        private readonly bool _isRequired;
        
        public OtoContext(DbContextOptions options, 
            PrincipalType type = PrincipalType.Student,
            bool isRequired = false) : base(options) 
        { 
            _type = type;
            _isRequired = isRequired;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Explicitly defined key
            modelBuilder.Entity<Address>().HasKey(a => a.Id);
            modelBuilder.Entity<Student>().HasKey(s => s.Id);

            // if value is generated in the entity class, use this
            modelBuilder.Entity<Address>()
                .Property(a => a.Id).ValueGeneratedNever();

            if(_type == PrincipalType.Student)
            {
                // Student - Parent
                // Address - Child
                // Address can exist w/o Student
                var refBuilder = modelBuilder.Entity<Student>()
                    .HasOne(s => s.Address)
                    .WithOne(a => a.Student)
                    .HasForeignKey<Address>("StudentId"); // Address has foreign key named StudentId(this is shadow)

                if (_isRequired)
                    refBuilder.IsRequired();// Address require this FK. Address model can not exist w/o defined Student
            }
            else
            {
                // Student - child
                // Address - parent
                // Student can exist w/o Address
                var refBuilder = modelBuilder.Entity<Student>()
                    .HasOne(s => s.Address)
                    .WithOne(a => a.Student)
                    .HasForeignKey<Student>("AddressId"); // Student has foreign key named AddressId(this is shadow)
                
                if(_isRequired)
                    refBuilder.IsRequired();// Student requires this FK. Student model can not exist w/o defined Address
            }
        }
    }
}
