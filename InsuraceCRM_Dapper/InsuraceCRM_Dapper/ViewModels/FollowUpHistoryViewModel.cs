using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class FollowUpHistoryViewModel
{
    public Customer? Customer { get; set; }
    public IEnumerable<FollowUp> FollowUps { get; set; } = Enumerable.Empty<FollowUp>();
}
