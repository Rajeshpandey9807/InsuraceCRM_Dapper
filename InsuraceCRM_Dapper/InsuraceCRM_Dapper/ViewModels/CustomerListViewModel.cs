using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class CustomerListViewModel
{
    public IEnumerable<Customer> Customers { get; set; } = Enumerable.Empty<Customer>();
    public bool CanEdit { get; set; }
}
