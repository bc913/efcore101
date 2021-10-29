using Microsoft.EntityFrameworkCore;

namespace Db.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Db.Sqlite.Samples.OneToOne.Run(
            //    new DbContextOptionsBuilder<Db.Sqlite.Contexts.OtoContext>()
            //    .UseSqlite("Filename=OtoRecords.db").Options, 
            //    Db.Sqlite.Contexts.PrincipalType.Address, 
            //    true);

            //Db.Sqlite.Samples.OneToMany.Run(
            //    new DbContextOptionsBuilder<Db.Sqlite.Contexts.OtmContext>()
            //    .UseSqlite("Filename=OtmRecords.db").Options,
            //    true);

            Db.Sqlite.Samples.ManyToMany.Run(
                new DbContextOptionsBuilder<Db.Sqlite.Contexts.MtmContext>()
               .UseSqlite("Filename=MtmRecords.db").Options);
        }
    }
}
