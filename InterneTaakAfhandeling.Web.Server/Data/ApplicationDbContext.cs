using Microsoft.EntityFrameworkCore;

namespace InterneTaakAfhandeling.Web.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<TestItem> TestItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestItem>().HasData(
                new TestItem { Id = 1, Name = "Test Item 1", Description = "This is test item 1" },
                new TestItem { Id = 2, Name = "Test Item 2", Description = "This is test item 2" },
                new TestItem { Id = 3, Name = "Test Item 3", Description = "This is test item 3" }
            );
        }
    }

    public class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}