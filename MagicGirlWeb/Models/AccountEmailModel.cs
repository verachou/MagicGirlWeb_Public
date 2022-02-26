using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  [Table("ACCOUNT_EMAIL")]
  public class AccountEmailModel : ObjectDetailModel
  {
    [ForeignKey("AccountModel")]
    [Column("account_id")]
    public int AccountId { get; set; }

    [DataType(DataType.EmailAddress)]
    [StringLength(100, ErrorMessage = "Cannot be longer than 100 characters.")]
    [Column("email")]
    public string Email { get; set; }

    [StringLength(50, ErrorMessage = "Cannot be longer than 50 characters.")]
    [Column("description")]
    public string Description { get; set; }


  }
}