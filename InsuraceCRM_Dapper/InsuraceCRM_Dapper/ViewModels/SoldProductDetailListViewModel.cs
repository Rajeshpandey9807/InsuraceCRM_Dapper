using System.Collections.Generic;
using System.Linq;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class SoldProductDetailListViewModel
{
    public IEnumerable<SoldProductDetailInfo> Details { get; set; } = Enumerable.Empty<SoldProductDetailInfo>();
    public int? FilteredCustomerId { get; set; }
    public string? FilteredCustomerName { get; set; }
    public bool IsFiltered => FilteredCustomerId.HasValue;
}
