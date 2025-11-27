using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;

    public RoleService(IRoleRepository roleRepository, IUserRepository userRepository)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
    }

    public Task<IEnumerable<Role>> GetAllAsync() => _roleRepository.GetAllAsync();

    public Task<Role?> GetByIdAsync(int id) => _roleRepository.GetByIdAsync(id);

    public async Task<bool> ExistsAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var role = await _roleRepository.GetByNameAsync(name.Trim());
        return role is not null;
    }

    public async Task<int> CreateAsync(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        var name = role.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Role name is required.");
        }

        var existingRole = await _roleRepository.GetByNameAsync(name);
        if (existingRole is not null)
        {
            throw new InvalidOperationException($"Role '{name}' already exists.");
        }

        role.Name = name;
        role.Description = string.IsNullOrWhiteSpace(role.Description) ? null : role.Description.Trim();
        role.IsSystem = role.IsSystem;

        return await _roleRepository.InsertAsync(role);
    }

    public async Task UpdateAsync(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        var existingRole = await _roleRepository.GetByIdAsync(role.Id);
        if (existingRole is null)
        {
            throw new KeyNotFoundException($"Role with id {role.Id} was not found.");
        }

        var newName = role.Name?.Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new InvalidOperationException("Role name is required.");
        }

        var nameChanged = !string.Equals(existingRole.Name, newName, StringComparison.OrdinalIgnoreCase);
        if (existingRole.IsSystem && nameChanged)
        {
            throw new InvalidOperationException("System roles cannot be renamed.");
        }

        if (nameChanged)
        {
            var duplicate = await _roleRepository.GetByNameAsync(newName);
            if (duplicate is not null && duplicate.Id != existingRole.Id)
            {
                throw new InvalidOperationException($"Role '{newName}' already exists.");
            }

            existingRole.Name = newName;
        }

        existingRole.Description = string.IsNullOrWhiteSpace(role.Description)
            ? null
            : role.Description.Trim();

        await _roleRepository.UpdateAsync(existingRole);
    }

    public async Task DeleteAsync(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role is null)
        {
            throw new KeyNotFoundException($"Role with id {id} was not found.");
        }

        if (role.IsSystem)
        {
            throw new InvalidOperationException("System roles cannot be deleted.");
        }

        var userCount = await _userRepository.CountByRoleAsync(role.Name);
        if (userCount > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete role '{role.Name}' because it is assigned to {userCount} user(s).");
        }

        await _roleRepository.DeleteAsync(id);
    }

    public async Task EnsureDefaultRolesAsync()
    {
        var defaults = new[]
        {
            new Role { Name = "Admin", Description = "Full system access", IsSystem = true },
            new Role { Name = "Manager", Description = "Manage teams and assignments" },
            new Role { Name = "Employee", Description = "Standard access for day-to-day work" }
        };

        foreach (var defaultRole in defaults)
        {
            var exists = await _roleRepository.GetByNameAsync(defaultRole.Name);
            if (exists is not null)
            {
                continue;
            }

            await _roleRepository.InsertAsync(defaultRole);
        }
    }
}
