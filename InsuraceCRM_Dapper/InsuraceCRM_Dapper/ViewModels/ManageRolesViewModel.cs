using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class ManageRolesViewModel
{
    public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();
    public IEnumerable<Role> Roles { get; set; } = Enumerable.Empty<Role>();
}
