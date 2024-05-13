using DAL.Models.Groups;

namespace API.GroupSumUps;

public class GroupSumUpModel
{
    public IQueryable<Group> Group { get; set; }
}