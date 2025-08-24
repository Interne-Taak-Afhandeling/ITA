using InterneTaakAfhandeling.Web.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterneTaakAfhandeling.Web.Server.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    
   
   public DbSet<KanalenEntity> Kanalen { get; set; }


   #region Override Methods

   public override int SaveChanges()
   {
       SetAuditFields();
       return base.SaveChanges();
   }  

   public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
   {
       SetAuditFields();
       return await base.SaveChangesAsync(cancellationToken);
   }

   #endregion
   #region Helpers

   private void SetAuditFields()
   {
       var entries = ChangeTracker.Entries<BaseEntity>();

       foreach (var entry in entries)
       {
           switch (entry.State)
           {
               case EntityState.Added:
               {
                   entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                   entry.Entity.UpdatedAt = DateTimeOffset.UtcNow; 
                    entry.Entity.Id = Guid.NewGuid();  
                   break;
               }
               case EntityState.Modified:
                   entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                   break;
               case EntityState.Detached:
               case EntityState.Unchanged:
               case EntityState.Deleted:
                   break;
               default:
                   throw new ArgumentOutOfRangeException();
           }
       }
   }


   #endregion
} 