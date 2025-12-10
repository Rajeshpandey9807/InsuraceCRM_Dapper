using System;
using System.Collections.Generic;

namespace InsuraceCRM_Dapper.ViewModels;

public class CustomerCreateViewModel
{
    public CustomerInputModel NewCustomer { get; set; } = new();
    public IReadOnlyList<string> BulkUploadErrors { get; set; } = Array.Empty<string>();
}
