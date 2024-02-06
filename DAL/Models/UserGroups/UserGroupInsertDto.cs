namespace DAL.Models.UserGroups;

public class UserGroupInsertDto
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    
    public UserGroup ToUserGroup()
    {
        return new UserGroup
        {
            UserId = UserId,
            GroupId = GroupId
        };
    }
}