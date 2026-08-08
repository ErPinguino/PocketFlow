using PocketFlow.Models;

namespace PocketFlow.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetBySupabaseUserIdAsync(string supabaseUserId);
    Task<User?> GetByIdAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(User user);
    void Update(User user);
    Task SaveChangesAsync();
}
