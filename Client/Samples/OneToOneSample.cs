using Microsoft.EntityFrameworkCore;
using System;
using Bcan.Efpg.Persistence.Contexts;
using Bcan.Efpg.Domain.Entities;
using System.Linq;

namespace Bcan.Efpg.Persistence.Samples
{
    public static class OneToOne
    {
        public static void Run(DbContextOptions options, PrincipalType type = PrincipalType.Address, bool isRequired = false)
        {
            Console.WriteLine("---- One-to-One Relationship -----");
            Console.WriteLine(string.Format("Principal: {0}", type == PrincipalType.Student ? "Student" : "Address"));
            Console.WriteLine();
            using (var context = new OtoContext(options, type, isRequired))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                #region CREATE
                // Create 
                Console.WriteLine("----- CREATE -----");
                Console.Write("Creating a student w/o address");
                var student1 = new Student { Name = "Micheal" };
                context.Students.Add(student1);
                try
                {
                    context.SaveChanges();
                    Console.WriteLine(" ==> Done");
                }
                catch (Exception e)
                {
                    context.Students.Remove(student1);
                    Console.WriteLine(" ==> FAILED: Can not add student w/o address: \n", e.Message);
                }    
                
                //
                Console.Write("Creating a student w/ address");
                var student2 = new Student { Name = "John", Address = new Address { City = "LA" } };
                context.Students.Add(student2);
                try
                {
                    context.SaveChanges();
                    Console.WriteLine(" ==> Done");
                }
                catch (Exception e)
                {
                    context.Students.Remove(student2);
                    Console.WriteLine(" ==> FAILED: Can not add student w/ address: \n ", e.Message);
                }
                
                //
                Console.Write("Creating an address w/ Student");
                var address3 = new Address { City = "Chicago", Student = new Student { Name = "Karl" } };
                context.Addresses.Add(address3);
                try
                {
                    context.SaveChanges();
                    Console.WriteLine(" ==> Done");
                }
                catch (Exception e)
                {
                    context.Addresses.Remove(address3);
                    Console.WriteLine(" ==> FAILURE: Can not add student w/ address: \n ", e.Message);
                }
                
                //
                Console.Write("Creating an address w/o Student");
                var address4 = new Address { City = "Istanbul" };
                context.Addresses.Add(address4);
                try
                {
                    context.SaveChanges();
                    Console.WriteLine(" ==> Done");
                }
                catch (Exception e)
                {
                    // Transaction rollback or catching exception through transaction commit
                    // does not clean up the context so you have to manually remove it.

                    //context.ChangeTracker.Entries<OneToZero.Address>()
                    //    .Where(e => e.State == EntityState.Added).ToList()
                    //    .ForEach(e => e.State = EntityState.Detached);

                    //context.Addresses.Remove(address4);

                    context.Entry(address4).State = EntityState.Detached;                    
                    Console.WriteLine(" ==> FAILED: Can not insert address4. Address4 instance should have required Student reference defined.");
                }
                #endregion

                #region QUERY
                Console.WriteLine("----- QUERY -----");
                Console.WriteLine("Students.Count: {0}", context.Students.Count());
                Console.WriteLine("Addresses.Count: {0}", context.Addresses.Count());

                Console.Write("Querying for Student");
                var foundStudent = context.Students.FirstOrDefault(u => u.Name == "Micheal");
                if (foundStudent != null)
                    Console.WriteLine("==> Done: Student found: {0}", foundStudent.Name);
                else
                    Console.WriteLine("==> FAILED: Student not found");
                //
                Console.Write("Querying for address");
                var foundAd4 = context.Addresses.FirstOrDefault(ad => ad.City == "Chicago");
                if (foundAd4 != null)
                    Console.WriteLine("==> Done: Address found: {0}", foundAd4.City);
                else
                    Console.WriteLine("==> FAILED: Address not found");
                //
                // No need to use .Include() as stated in Microsoft documentation. The navigation properies are already loaded since we didn't change contexts
                Console.Write("Querying for student w/ address");
                var foundStwAddress = context.Students.FirstOrDefault(s => s.Name == "John");
                if (foundStwAddress == null)
                    Console.WriteLine(" ==> FAILED: Can not find student with Name John");
                else
                    Console.WriteLine(" ==> Done: City for John is : {0}", foundStwAddress.Address.City);
                //
                Console.Write("Querying for address w/ student");
                var foundAddwStudent = context.Addresses.FirstOrDefault(s => s.City == "Chicago");
                if (foundAddwStudent == null)
                    Console.WriteLine(" ==> FAILED: Can not find student with Name John");
                else
                    Console.WriteLine(" ==> Done: Student for city Chicago is : {0}", foundAddwStudent.Student.Name);

                #endregion
            }


            #region UPDATE
            Console.WriteLine();
            using (var context = new OtoContext(options, type, isRequired))
            {
                Console.WriteLine("----- UPDATE -----");
                //
                Console.Write("Update name for a student");
                var foundStudent1 = context.Students.FirstOrDefault(u => u.Name == "John"); // Always exist
                foundStudent1.Name = "Julia";
                try
                {
                    context.SaveChanges();
                    Console.WriteLine(" ==> Done");
                }
                catch (Exception)
                {
                    context.Entry(foundStudent1).State = EntityState.Unchanged;
                    Console.WriteLine("==> FAILED: Can not update the name for the student");
                }

                //
                Console.Write("Update city for an address");
                var foundAddress1 = context.Addresses.FirstOrDefault(a => a.City == "Chicago"); // Always exist
                foundAddress1.City = "New York";
                try
                {
                    context.SaveChanges();
                    Console.WriteLine(" ==> Done");
                }
                catch (Exception)
                {
                    context.Entry(foundAddress1).State = EntityState.Unchanged;
                    Console.WriteLine("==> FAILED: Can not update the city for the address");
                }

                //
                Console.Write("Query a student w/o address and update Address");
                var foundStudent2 = context.Students.FirstOrDefault(u => u.Name == "Micheal");
                if (foundStudent2 == null)
                    Console.WriteLine(" ==> Student: Micheal can not be found");
                else
                {
                    foundStudent2.Address = new Address { City = "London" };
                    try
                    {
                        context.SaveChanges();
                        Console.WriteLine(" ==> Done");
                    }
                    catch (Exception e)
                    {
                        context.Entry(foundStudent2).State = EntityState.Unchanged;
                        Console.WriteLine("==> FAILED: Can not update the student w new Address.");
                    }
                }

                //
                Console.Write("Query an address w/o student and Update Student");
                var foundAddress2 = context.Addresses.FirstOrDefault(a => a.City == "Istanbul");
                if(foundAddress2 == null)
                {
                    Console.WriteLine("\n ==> FAILED: Can not find Istanbul");
                }
                else
                {
                    foundAddress2.Student = new Student { Name = "Mert" };
                    try
                    {
                        context.SaveChanges();
                        Console.WriteLine(" ==> Done");
                    }
                    catch (Exception)
                    {
                        context.Entry(foundAddress2).State = EntityState.Unchanged;
                        Console.WriteLine("==> FAILED: Can not update the address with adding student.");
                    }
                }
                
            }
            #endregion

            #region QUERY NAVIGATION PROPERTIES
            Console.WriteLine();
            // For querying navigation properties
            // Created this context to make use of .Include() during query
            using (var context = new OtoContext(options, type, isRequired))
            {
                Console.WriteLine("----- QUERY FOR NAVIGATION PROPERTIES AFTER UPDATES -----");
                Console.WriteLine("Students.Count: {0}", context.Students.Count());
                Console.WriteLine("Addresses.Count: {0}", context.Addresses.Count());

                Console.Write("Query for the student record with the new address");
                var student = context.Students.Include(s=>s.Address).FirstOrDefault(u => u.Name == "Micheal");
                if (student == null)
                    Console.WriteLine(" ==> FAILED: Can not find the student with Name Micheal");
                else
                    Console.WriteLine(" ==> Done: City for Micheal is: {0}", student.Address.City);

                Console.Write("Query for the address record with new student info");
                var address = context.Addresses.Include(a=>a.Student).FirstOrDefault(a => a.City == "Istanbul");
                if (address == null)
                    Console.WriteLine(" ==> FAILED: Can not find the address with city name Istanbul");
                else
                    Console.WriteLine(" ==> Done: Student for Istanbul is: {0}", address.Student.Name);                      
            }
            #endregion

            #region DELETE
            Console.WriteLine();
            using (var context = new OtoContext(options, type, isRequired))
            {
                Console.WriteLine("----- DELETE -----");
                Console.Write("Deleting student w/ address");
                Student student = null;
                if (!isRequired && type == PrincipalType.Student)
                    student = context.Students.Include(s => s.Address).FirstOrDefault(s => s.Name == "Karl");
                else
                    student = context.Students.FirstOrDefault(s => s.Name == "Karl");

                context.Remove(student);
                context.SaveChanges();
                Console.WriteLine(" ==> Done");
                

                Console.Write("Deleting address w/ student");
                Address address = null;
                if (!isRequired && type == PrincipalType.Address)
                    address = context.Addresses.Include(a => a.Student).FirstOrDefault(a => a.City == "LA");
                else
                    address = context.Addresses.FirstOrDefault(a => a.City == "LA");

                context.Addresses.Remove(address);
                context.SaveChanges();
                Console.WriteLine(" ==> Done");
            }
            #endregion


        }
    }
}
