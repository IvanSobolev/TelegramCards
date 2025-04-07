using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TelegramCards.Models.Enum;

namespace TelegramCards.Models.Entitys;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long TelegramId { get; set; }
    public string Username { get; set; }
    public Roles Role { get; set; }
    public DateTime LastTakeCard { get; set; }
    public ICollection<Card> Cards { get; set; }
}