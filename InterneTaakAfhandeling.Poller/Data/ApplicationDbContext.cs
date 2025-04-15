using InterneTaakAfhandeling.Poller.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterneTaakAfhandeling.Poller.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

       public DbSet<InternetakenNotifierState> InternetakenNotifierStates { get; set; }
    }
}