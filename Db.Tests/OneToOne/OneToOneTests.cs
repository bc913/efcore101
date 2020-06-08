using Db.Sqlite.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Db.Sqlite.Contexts;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Db.Tests.OneToOne
{
    public abstract class OtoTestsBase
    {
        protected readonly DbContextOptions ContextOptions;
        protected readonly PrincipalType Principal = PrincipalType.Student;
        protected abstract bool IsRequired { get; }
        public OtoTestsBase(DbContextOptions options)
        {
            ContextOptions = options;
        }

        // Asuumption: Works only where Principal type is Student.
        protected IList<Student> GenerateData()
        {
            var students = new List<Student>();

            students.Add(new Student { Name = "Karl" });
            students.Add(new Student { Name = "Julia", Address = new Address { City = "Utah" } });
            students.Add(new Student { Name = "John", Address = new Address { City = "Tacoma" } });

            return students;
        }
    }

    public abstract class InMemorySqliteTests : OtoTestsBase, IDisposable
    {
        private readonly DbConnection _connection;

        public InMemorySqliteTests() : base(new DbContextOptionsBuilder<Db.Sqlite.Contexts.OtoContext>()
            .UseSqlite(CreateInMemoryDatabase())
            .Options)
        {
            _connection = RelationalOptionsExtension.Extract(ContextOptions).Connection;
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }
        public void Dispose() => _connection.Dispose();
    }    
}
