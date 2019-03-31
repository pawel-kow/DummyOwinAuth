using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Id4meOwinAuth.Models;

namespace Id4meOwinAuth.DAL
{
    public class ID4meRegistratiornsContext : DbContext
    {

        public ID4meRegistratiornsContext(string dbConnection) : base(dbConnection)
        {
        }

        public DbSet<Id4meClientRegistrations> Registrations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
