using Application.Interfaces.Persistence.Users;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services.Users;

public class UserService : IUserService
{
    private readonly IUserQueries _queries;
    private readonly IUserCommands _commands;

    public UserService(IUserQueries queries, IUserCommands commands)
    {
        _queries = queries;
        _commands = commands;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _queries.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _queries.GetByIdAsync(id);
    }

    public async Task<User> CreateAsync(User user)
    {
        return await _commands.CreateAsync(user);
    }

    public async Task<bool> UpdateAsync(int id, User user)
    {
        return await _commands.UpdateAsync(id, user);
    }
}