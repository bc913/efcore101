using Microsoft.EntityFrameworkCore;

namespace Bcan.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Bcan.Domain.Samples.OneToOne.Run(
            //    new DbContextOptionsBuilder<Bcan.Domain.Contexts.OtoContext>()
            //    .UseSqlite("Filename=OtoRecords.db").Options, 
            //    Bcan.Domain.Contexts.PrincipalType.Address, 
            //    true);

            //Bcan.Domain.Samples.OneToMany.Run(
            //    new DbContextOptionsBuilder<Bcan.Domain.Contexts.OtmContext>()
            //    .UseSqlite("Filename=OtmRecords.db").Options,
            //    true);

            Bcan.Domain.Samples.ManyToMany.Run(
                new DbContextOptionsBuilder<Bcan.Domain.Contexts.MtmContext>()
               .UseSqlite("Filename=MtmRecords.db").Options);
        }
    }
}
