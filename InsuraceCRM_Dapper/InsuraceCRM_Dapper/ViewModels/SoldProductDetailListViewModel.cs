using System.Collections.Generic;
using System.Linq;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class SoldProductDetailListViewModel
{
    public IEnumerable<SoldProductDetailInfo> Details { get; set; } = Enumerable.Empty<SoldProductDetailInfo>();
}
