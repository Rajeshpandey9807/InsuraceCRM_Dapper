using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class ManageRolesViewModel
{
    public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();
    public IEnumerable<string> Roles { get; set; } = new[] { "Admin", "Manager", "Employee" };
}
