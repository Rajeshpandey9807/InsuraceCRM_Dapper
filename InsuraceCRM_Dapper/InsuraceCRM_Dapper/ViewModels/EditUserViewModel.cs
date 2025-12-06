using InsuraceCRM_Dapper.Models;
using System.Collections.Generic;

namespace InsuraceCRM_Dapper.ViewModels;

public class EditUserViewModel
{
    public UserFormViewModel Form { get; set; } = new();
    public IEnumerable<Role> Roles { get; set; }
    //public IEnumerable<string> Roles { get; set; } = new[] { "Admin", "Manager", "Employee" };
}
