using System.Data.Entity;

namespace WPFTask
{
    class MyDbContext : DbContext
    {
        public MyDbContext() : base("DbConnectionString")
        {
        }
        
        public DbSet<Person> Persons { get; set; }
    }
}
