using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;
using Microsoft.AspNetCore.Identity;

namespace InsuraceCRM_Dapper.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return verificationResult == PasswordVerificationResult.Success ? user : null;
    }

    public Task<IEnumerable<User>> GetAllUsersAsync() => _userRepository.GetAllAsync();

    public Task<User?> GetByIdAsync(int id) => _userRepository.GetByIdAsync(id);

    public async Task UpdateRoleAsync(int userId, string role)
    {
        await _userRepository.UpdateRoleAsync(userId, role);
    }

    public async Task EnsureDefaultAdminAsync()
    {
        const string adminEmail = "admin@crm.local";
        var existingAdmin = await _userRepository.GetByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            return;
        }

        var adminUser = new User
        {
            Name = "System Administrator",
            Email = adminEmail,
            Role = "Admin"
        };

        adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, "Admin@123");
        await _userRepository.InsertAsync(adminUser);
    }
}
