using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Web.Server.Data;
using InterneTaakAfhandeling.Web.Server.Data.Entities;
using Microsoft.EntityFrameworkCore; 

namespace InterneTaakAfhandeling.Web.Server.Features.Kanalen;

public interface IKanalenService
{
    Task<List<KanalenEntity>> GetKanalen();
    Task<KanalenEntity> CreateKanalen(string name);
}

public class KanalenService(ApplicationDbContext dbContext) : IKanalenService
{
    public async Task<List<KanalenEntity>> GetKanalen()
    {
        return await dbContext.Kanalen.ToListAsync();
    }

    public async Task<KanalenEntity> CreateKanalen(string name)
    {
        if ((await dbContext.Kanalen.AnyAsync(x=> x.Name == name)))
        {
            throw new ConflictException($"Er bestaat al een kanaal met de naam {name}.");
        }
        
        var kanalen = new KanalenEntity { Name = name };
        dbContext.Add(kanalen);
        await dbContext.SaveChangesAsync();
        return kanalen;
    }
}