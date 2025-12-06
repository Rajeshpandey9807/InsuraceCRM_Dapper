using System.Collections.Generic;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class BulkAssignCustomersViewModel
{
    public IEnumerable<Customer> Customers { get; set; } = Enumerable.Empty<Customer>();
    public IEnumerable<User> Employees { get; set; } = Enumerable.Empty<User>();
    public List<int> SelectedCustomerIds { get; set; } = new();
    public int? SelectedEmployeeId { get; set; }
}
