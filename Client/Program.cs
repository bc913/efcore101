using Microsoft.EntityFrameworkCore;

namespace Bcan.Efpg.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Bcan.Efpg.Domain.Samples.OneToOne.Run(
            //    new DbContextOptionsBuilder<Bcan.Efpg.Persistence.Contexts.OtoContext>()
            //    .UseSqlite("Filename=OtoRecords.db").Options, 
            //    Bcan.Efpg.Persistence.Contexts.PrincipalType.Address, 
            //    true);

            //Bcan.Efpg.Domain.Samples.OneToMany.Run(
            //    new DbContextOptionsBuilder<Bcan.Efpg.Persistence.Contexts.OtmContext>()
            //    .UseSqlite("Filename=OtmRecords.db").Options,
            //    true);

            Persistence.Samples.ManyToMany.Run(
                new DbContextOptionsBuilder<Bcan.Efpg.Persistence.Contexts.MtmContext>()
               .UseSqlite("Filename=MtmRecords.db").Options);
        }
    }
}
