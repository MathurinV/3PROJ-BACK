namespace API.SumUps.UserSumUps;

public class UserSumUpModel
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public ICollection<GroupInfos> GroupInfos { get; set; } = null!;
    public DateTime CurrentDate { get; } = DateTime.Now;
}

public class GroupInfos
{
    public string GroupName { get; set; } = null!;
    public decimal Balance { get; set; }
    public decimal AmountDue { get; set; }
    public string? ToUserName { get; set; }
}