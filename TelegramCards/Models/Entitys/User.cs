using System.ComponentModel.DataAnnotations.Schema;
using TelegramCards.Models.Enum;

namespace TelegramCards.Models.Entitys;

public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long TelegramId { get; set; }
    public string Username { get; set; }
    public Roles Role { get; set; }
    public ICollection<Card> Cards { get; set; }
}