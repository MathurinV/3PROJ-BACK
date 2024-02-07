using System.ComponentModel.DataAnnotations;
using DAL.Models.Users;

namespace DAL.Models.Messages;

public class Message
{
    public Guid Id { get; set; }
    [StringLength(255)]public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public Guid SenderId { get; set; }
    public AppUser Sender { get; set; } = null!;
}