using System.Collections.Generic;
using System.Linq;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class ManageUsersViewModel
{
    public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();
    public UserFormViewModel NewUser { get; set; } = new();
    public IEnumerable<Role> Role { get; set; } = Enumerable.Empty<Role>();
    public IEnumerable<string> Roles { get; set; } = new[] { "Admin", "Manager", "Employee" };
    public IEnumerable<int> RoleId { get; set; } = new[] { 1, 2, 3 };
    public bool IncludeInactive { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }

}
