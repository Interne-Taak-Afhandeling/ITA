using Microsoft.EntityFrameworkCore;

namespace InterneTaakAfhandeling.Web.Server.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
}