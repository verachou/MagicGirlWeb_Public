using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  [Table("ACCOUNT")]
  public class AccountModel : ObjectModel
  {
    [StringLength(10, ErrorMessage = "Cannot be longer than 10 characters.")]
    [Column("provider")]
    public string Provider { get; set; }

  }
}