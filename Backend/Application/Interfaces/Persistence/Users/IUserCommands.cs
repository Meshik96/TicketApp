using Domain.Entities;

namespace Application.Interfaces.Persistence.Users;

public interface IUserCommands
{
    Task<User> CreateAsync(User user);
    Task<bool> UpdateAsync(int id, User user);
}