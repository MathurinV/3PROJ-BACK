namespace DAL.Models.UserGroups;

public class UserGroupInsertInput
{
    public Guid GroupId { get; set; }
    
    public UserGroupInsertDto ToUserGroupInsertDto(Guid userId) => new()
    {
        UserId = userId,
        GroupId = GroupId
    };
}