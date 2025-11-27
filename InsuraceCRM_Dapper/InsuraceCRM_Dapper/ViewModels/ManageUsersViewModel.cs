using System.Collections.Generic;
using System.Linq;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class ManageUsersViewModel
{
    public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();
    public UserFormViewModel NewUser { get; set; } = new();
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
}
