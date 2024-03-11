using DAL.Models.Justifications;
using Microsoft.AspNetCore.Http;

namespace DAL.Repositories;

public interface IJustificationRepository
{
    Task<bool> SaveJustificationAsync(Guid expenseId, IFormFile file);
    Task<Justification?> GetJustificationAsync(Guid expenseId);
}