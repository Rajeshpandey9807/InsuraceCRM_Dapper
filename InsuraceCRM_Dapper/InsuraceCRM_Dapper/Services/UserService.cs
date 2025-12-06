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
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return verificationResult == PasswordVerificationResult.Success ? user : null;
    }

    public Task<IEnumerable<User>> GetAllUsersAsync(bool includeInactive = false) =>
        _userRepository.GetAllAsync(includeInactive);

    public Task<User?> GetByIdAsync(int id) => _userRepository.GetByIdAsync(id);

    public async Task<int> CreateUserAsync(User user, string password)
    {
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        user.IsActive = true;
        return await _userRepository.InsertAsync(user);
    }

    public async Task UpdateUserAsync(User user, string? newPassword = null)
    {
        var existingUser = await _userRepository.GetByIdAsync(user.Id);
        if (existingUser is null)
        {
            throw new KeyNotFoundException($"User with id {user.Id} was not found.");
        }

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        existingUser.Mobile = user.Mobile;
        existingUser.Role = user.Role;
        existingUser.RoleId = user.RoleId;
        existingUser.IsActive = user.IsActive;

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, newPassword);
        }

        await _userRepository.UpdateAsync(existingUser);
    }

    public async Task UpdateRoleAsync(int userId, string role)
    {
        await _userRepository.UpdateRoleAsync(userId, role);
    }

    public async Task SetActiveStateAsync(int userId, bool isActive)
    {
        await _userRepository.SetActiveStateAsync(userId, isActive);
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
            Role = "Admin",
            Mobile = string.Empty,
            IsActive = true
        };

        adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, "Admin@123");
        await _userRepository.InsertAsync(adminUser);
    }
}
