using DAL.Models.Groups;
using DAL.Models.Users;
using HotChocolate;

namespace DAL.Models.Messages;

public class GroupMessage
{
    public Guid Id { get; set; }
    [GraphQLIgnore] public Guid SenderId { get; set; }
    public AppUser Sender { get; set; } = null!;
    [GraphQLIgnore] public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
}