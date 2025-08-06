using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Web.Server.Data;
using InterneTaakAfhandeling.Web.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterneTaakAfhandeling.Web.Server.Features.Kanalen;

public interface IKanalenService
{
    Task<List<KanalenEntity>> GetKanalen();
    Task<KanalenEntity> GetKanalenById(string id);
    Task<KanalenEntity> CreateKanalen(string name);
    Task<KanalenEntity> EditKanalen(string id, string name);
    Task DeleteKanalen(string id);
}

public class KanalenService(ApplicationDbContext dbContext) : IKanalenService
{
    public async Task<List<KanalenEntity>> GetKanalen()
    {
        return await dbContext.Kanalen.ToListAsync();
    }

    public async Task<KanalenEntity> GetKanalenById(string id)
    {
        return await dbContext.Kanalen.FindAsync(Guid.Parse(id)) ??
               throw new ConflictException("Kanaal niet gevonden.");
    }

    public async Task<KanalenEntity> CreateKanalen(string naam)
    {
        if (await dbContext.Kanalen.AnyAsync(x => x.Naam == naam))
            throw new ConflictException($"Er bestaat al een kanaal met de naam {naam}.");

        var kanalen = new KanalenEntity { Naam = naam };
        dbContext.Add(kanalen);
        await dbContext.SaveChangesAsync();
        return kanalen;
    }

    public async Task<KanalenEntity> EditKanalen(string id, string naam)
    {
        var kanalen = await dbContext.Kanalen.FindAsync(Guid.Parse(id));
        if (kanalen == null) throw new ConflictException("Kanaal niet gevonden.");
        kanalen.Naam = naam;
        dbContext.Update(kanalen);
        await dbContext.SaveChangesAsync();
        return kanalen;
    }

    public async Task DeleteKanalen(string id)
    {
        var kanalen = await dbContext.Kanalen.FindAsync(Guid.Parse(id));
        if (kanalen == null) throw new ConflictException("Kanaal niet gevonden.");

        dbContext.Kanalen.Remove(kanalen);
        await dbContext.SaveChangesAsync();
    }
}