using System.Collections.Generic;
using System.Linq;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class ProductListViewModel
{
    public IEnumerable<Product> Products { get; set; } = Enumerable.Empty<Product>();
    public bool CanManage { get; set; }
}
