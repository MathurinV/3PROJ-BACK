using DAL.Models.Justifications;
using DAL.Repositories;
using Files.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class JustificationRepository(MoneyMinderDbContext context) : IJustificationRepository
{
    public async Task<bool> SaveJustificationAsync(Guid expenseId, IFormFile file)
    {
        var response =
            await context.SaveFileAsync<Justification>(file.OpenReadStream(), expenseId.ToString(), file.ContentType);
        return true;
    }

    public async Task<Justification?> GetJustificationAsync(Guid expenseId)
    {
        return await context.Justifications.FirstOrDefaultAsync(j => j.Name == expenseId.ToString());
    }
}