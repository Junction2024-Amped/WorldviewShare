using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Data;
using WorldviewShareServer.Dtos;
using WorldviewShareServer.Models;
namespace WorldviewShareServer.Services;

public class UsersService
{
    private readonly WorldviewShareContext _context;
    public UsersService(WorldviewShareContext context)
    {
        _context = context;
    }
        
    public User ToUser(UserRequestDto userRequestDto)
    {
        var user = new User
        {
            Username = userRequestDto.Username
        };
        _context.Users.Add(user);
        return user;
    }
    
    public async Task<List<User>> GetUsers() => await _context.Users.ToListAsync();
        
    public async Task<User?> GetUserById(Guid id) => await _context.Users.FindAsync(id);
        
    public UserResponseDto ToUserResponseDto(User user) => new(user.Id, user.Username, user.TopicSessions.Select(ts => ts.Id).ToList());
    
    public bool UserExists(Guid id) => _context.Users.Any(e => e.Id == id);
    
    public void RemoveUser(User user) => _context.Users.Remove(user);
    
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    
    public void SetEntityState(User user, EntityState state) => _context.Entry(user).State = state;
}