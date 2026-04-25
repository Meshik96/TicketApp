using Domain.Entities;

namespace Application.Interfaces.Persistence.Users;

public interface IUserQueries
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
}