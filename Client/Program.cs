using Microsoft.EntityFrameworkCore;

namespace Bcan.Efpg.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Bcan.Efpg.Domain.Samples.OneToOne.Run(
            //    new DbContextOptionsBuilder<Bcan.Efpg.Domain.Contexts.OtoContext>()
            //    .UseSqlite("Filename=OtoRecords.db").Options, 
            //    Bcan.Efpg.Domain.Contexts.PrincipalType.Address, 
            //    true);

            //Bcan.Efpg.Domain.Samples.OneToMany.Run(
            //    new DbContextOptionsBuilder<Bcan.Efpg.Domain.Contexts.OtmContext>()
            //    .UseSqlite("Filename=OtmRecords.db").Options,
            //    true);

            Bcan.Efpg.Domain.Samples.ManyToMany.Run(
                new DbContextOptionsBuilder<Bcan.Efpg.Domain.Contexts.MtmContext>()
               .UseSqlite("Filename=MtmRecords.db").Options);
        }
    }
}
