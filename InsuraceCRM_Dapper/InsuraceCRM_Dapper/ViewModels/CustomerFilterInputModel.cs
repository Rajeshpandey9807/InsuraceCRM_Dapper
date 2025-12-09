using System;

namespace InsuraceCRM_Dapper.ViewModels;

public class CustomerFilterInputModel
{
    public const string AssignmentAssigned = "assigned";
    public const string AssignmentUnassigned = "unassigned";

    public string? SearchTerm { get; set; }
    public string? Location { get; set; }
    public string? InsuranceType { get; set; }
    public string? Assignment { get; set; }

    public bool HasValues =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        !string.IsNullOrWhiteSpace(Location) ||
        !string.IsNullOrWhiteSpace(InsuranceType) ||
        !string.IsNullOrWhiteSpace(Assignment);

    public bool AssignmentEquals(string value) =>
        !string.IsNullOrWhiteSpace(Assignment) &&
        Assignment.Equals(value, StringComparison.OrdinalIgnoreCase);
}
