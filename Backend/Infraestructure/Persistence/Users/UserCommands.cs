using Application.Interfaces.Persistence.Users;
using Domain.Entities;
using Infraestructure.Data;

namespace Infraestructure.Persistence.Users;
public class UserCommands : IUserCommands
{
    private readonly AppDbContext _context;

    public UserCommands(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateAsync(int id, User user)
    {
        var existing = await _context.Users.FindAsync(id);
        if (existing == null) return false;

        existing.Name = user.Name;
        existing.Email = user.Email;

        await _context.SaveChangesAsync();
        return true;
    }
}