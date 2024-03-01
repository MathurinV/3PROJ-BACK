namespace DAL.Models.UserGroups;

public class UserGroupInsertInput
{
    public Guid GroupId { get; set; }

    public UserGroupInsertDto ToUserGroupInsertDto(Guid userId)
    {
        return new UserGroupInsertDto
        {
            UserId = userId,
            GroupId = GroupId
        };
    }
}