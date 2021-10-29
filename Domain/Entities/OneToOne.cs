using System;

namespace Bcan.Domain.Entities
{
    public class Student
    {
        public Guid Id { get; set; } /*= Guid.NewGuid();*/

        public string Name { get; set; }

        public Address Address { get; set; }

    }

    public class Address
    {
        public Guid Id { get; set; } = Guid.NewGuid();// throws exception while updating a student address with new address. Let it be generated implicitly.
        //https://github.com/npgsql/efcore.pg/issues/971#issuecomment-520315902

        public string City { get; set; }

        public Student Student { get; set; }

    }
}
