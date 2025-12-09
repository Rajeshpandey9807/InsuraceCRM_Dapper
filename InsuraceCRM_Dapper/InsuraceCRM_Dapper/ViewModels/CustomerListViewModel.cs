using System.Collections.Generic;
using System.Linq;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class CustomerListViewModel
{
    public IEnumerable<Customer> Customers { get; set; } = Enumerable.Empty<Customer>();
    public bool CanEdit { get; set; }
    public CustomerInputModel NewCustomer { get; set; } = new();
    public CustomerFilterInputModel Filters { get; set; } = new();
    public bool HasActiveFilters { get; set; }
}
