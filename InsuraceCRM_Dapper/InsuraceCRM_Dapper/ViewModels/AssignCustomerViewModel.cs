using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class AssignCustomerViewModel
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? AssignedEmployeeId { get; set; }
    public IEnumerable<User> Employees { get; set; } = Enumerable.Empty<User>();
}
