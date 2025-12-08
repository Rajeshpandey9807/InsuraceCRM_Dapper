using System.Collections.Generic;
using System.Linq;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class UserInsightsViewModel
{
    public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();
    public int? SelectedUserId { get; set; }
    public User? SelectedUser { get; set; }
    public IEnumerable<UserFollowUpDetail> FollowUps { get; set; } = Enumerable.Empty<UserFollowUpDetail>();
    public IEnumerable<SoldProductDetailInfo> SoldProducts { get; set; } = Enumerable.Empty<SoldProductDetailInfo>();

    public bool HasSelection => SelectedUser is not null;
    public int FollowUpCount => FollowUps?.Count() ?? 0;
    public int SoldProductCount => SoldProducts?.Count() ?? 0;
}
