using System.Collections.Generic;
using System.Linq;

namespace InsuraceCRM_Dapper.ViewModels;

public class EditUserViewModel
{
    public UserFormViewModel Form { get; set; } = new();
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
}
