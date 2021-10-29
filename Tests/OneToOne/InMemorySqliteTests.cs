using Bcan.Efpg.Domain.Contexts;
using Bcan.Efpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;

namespace Bcan.Efpg.Tests.OneToOne
{
    [TestFixture]
    public class InMemorySqliteTestsOptional : InMemorySqliteTests
    {
        protected override bool IsRequired => false;


        [SetUp]
        public void SetUp()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Database.EnsureCreated();
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Database.EnsureDeleted();
            }
        }

        #region CREATE
        [Test]
        public void Can_Create_Principal_Without_Dependent()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = new Student { Name = "Earvin" };
                context.Students.Add(student);
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.FirstOrDefault(s => s.Name == "Earvin");
                Assert.IsNotNull(student);
            }
        }

        [Test]
        public void Can_Create_Dependent_Without_Principal()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var address = new Address { City = "Los Angeles" };
                context.Addresses.Add(address);
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var address = context.Addresses.FirstOrDefault(a => a.City == "Los Angeles");
                Assert.IsNotNull(address);
            }
        }


        #endregion

        #region UPDATE
        [Test]
        public void Updating_Dependent_Through_Principal_FAILS_Without_Include()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.FirstOrDefault(s => s.Name == "Julia");
                Assert.IsNotNull(student);
                Assert.IsNull(student.Address);
            }
        }

        [Test]
        public void Updating_Dependent_Through_Principal_Requires_Include()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.Include(s=>s.Address).FirstOrDefault(s => s.Name == "Julia");
                Assert.IsNotNull(student);
                Assert.IsNotNull(student.Address);

                student.Address.City = "New York City";
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var address = context.Addresses.FirstOrDefault(a => a.City == "New York City");
                Assert.IsNotNull(address);
            }
        }

        [Test]
        public void Can_Update_Dependent_Alone()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var address = context.Addresses.FirstOrDefault(a => a.City == "Utah");
                Assert.IsNotNull(address);
                address.City = "New York City";
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var address = context.Addresses.FirstOrDefault(a => a.City == "New York City");
                Assert.IsNotNull(address);

                var student = context.Students.Include(s => s.Address).FirstOrDefault(s => s.Name == "Julia");
                Assert.IsNotNull(student);
                Assert.IsNotNull(student.Address);
                Assert.AreEqual("New York City", student.Address.City);
            }
        }
        #endregion

        #region DELETE
        [Test]
        public void Deleting_Principal_FAILS_Without_Including_Dependent()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.FirstOrDefault(s => s.Name == "John");
                context.Students.Remove(student);
                Assert.That(() => context.SaveChanges(), Throws.Exception);
            }
        }

        [Test]
        public void Deleting_Principal_With_Include_SUCCEED_Has_No_Effect_On_Dependent()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.Include(s=>s.Address).FirstOrDefault(s => s.Name == "John");
                context.Students.Remove(student);
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.FirstOrDefault(s => s.Name == "John");
                Assert.IsNull(student);

                var city = context.Addresses.FirstOrDefault(a => a.City == "Tacoma");
                Assert.IsNotNull(city);
            }
        }

        [Test]
        public void Deleting_DEPENDENT_Has_No_Effect_On_Principal()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var address = context.Addresses.FirstOrDefault(a => a.City == "Tacoma");
                context.Addresses.Remove(address);
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var city = context.Addresses.FirstOrDefault(a => a.City == "Tacoma");
                Assert.IsNull(city);

                var student = context.Students.FirstOrDefault(s => s.Name == "John");
                Assert.IsNotNull(student);

                
            }
        }
        #endregion
    }


    [TestFixture]
    public class InMemorySqliteTestsRequired : InMemorySqliteTests
    {
        protected override bool IsRequired => true;

        [SetUp]
        public void SetUp()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Database.EnsureCreated();
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Database.EnsureDeleted();
            }
        }

        #region CREATE
        [Test]
        public void Can_Create_Principal_Without_Dependent()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = new Student { Name = "Earvin" };
                context.Students.Add(student);
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.FirstOrDefault(s => s.Name == "Earvin");
                Assert.IsNotNull(student);
            }
        }

        [Test]
        public void Can_NOT_Create_Dependent_Without_Principal()
        {
            //Throws
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var address = new Address { City = "Los Angeles" };
                context.Addresses.Add(address);
                Assert.That(() => context.SaveChanges(), Throws.Exception);
            }
        }


        #endregion

        #region UPDATE
        [Test]
        public void Updating_Dependent_Through_Principal_FAILS_Without_Include()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.FirstOrDefault(s => s.Name == "Julia");
                Assert.IsNotNull(student);
                Assert.IsNull(student.Address);
            }
        }

        [Test]
        public void Updating_Dependent_Through_Principal_Requires_Include()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.Include(s => s.Address).FirstOrDefault(s => s.Name == "Julia");
                Assert.IsNotNull(student);
                Assert.IsNotNull(student.Address);

                student.Address.City = "New York City";
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var address = context.Addresses.FirstOrDefault(a => a.City == "New York City");
                Assert.IsNotNull(address);
            }
        }

        [Test]
        public void Can_Update_Dependent_Alone()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var address = context.Addresses.FirstOrDefault(a => a.City == "Utah");
                Assert.IsNotNull(address);
                address.City = "New York City";
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var address = context.Addresses.FirstOrDefault(a => a.City == "New York City");
                Assert.IsNotNull(address);

                var student = context.Students.Include(s => s.Address).FirstOrDefault(s => s.Name == "Julia");
                Assert.IsNotNull(student);
                Assert.IsNotNull(student.Address);
                Assert.AreEqual("New York City", student.Address.City);
            }
        }
        #endregion

        #region DELETE
        [Test]
        public void Deleting_Principal_FAILS_Without_Including_Dependent()// Will remove dependent too
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.FirstOrDefault(s => s.Name == "John");
                context.Students.Remove(student);
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.FirstOrDefault(s => s.Name == "John");
                Assert.IsNull(student);

                var city = context.Addresses.FirstOrDefault(a => a.City == "Tacoma");
                Assert.IsNull(city);
            }
        }

        [Test]
        public void Deleting_Principal_With_Include_SUCCEED_Will_Remove_Dependent()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.Include(s => s.Address).FirstOrDefault(s => s.Name == "John");
                context.Students.Remove(student);
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var student = context.Students.FirstOrDefault(s => s.Name == "John");
                Assert.IsNull(student);

                var city = context.Addresses.FirstOrDefault(a => a.City == "Tacoma");
                Assert.IsNull(city);
            }
        }

        [Test]
        public void Deleting_DEPENDENT_Has_No_Effect_On_Principal()
        {
            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                context.Students.AddRange(GenerateData());
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var address = context.Addresses.FirstOrDefault(a => a.City == "Tacoma");
                context.Addresses.Remove(address);
                context.SaveChanges();
            }

            using (var context = new OtoContext(ContextOptions, Principal, IsRequired))
            {
                var city = context.Addresses.FirstOrDefault(a => a.City == "Tacoma");
                Assert.IsNull(city);

                var student = context.Students.FirstOrDefault(s => s.Name == "John");
                Assert.IsNotNull(student);
            }
        }
        #endregion

    }
}
