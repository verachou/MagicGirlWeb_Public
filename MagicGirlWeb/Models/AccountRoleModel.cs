using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  [Table("ACCOUNT_ROLE")]
  public class AccountRoleModel: ObjectRelationModel
  {
    [ForeignKey("AccountModel")]
    [Column("account_id")]
    public int AccountId { get; set; }

    [ForeignKey("RoleModel")]
    [Column("role_id")]
    public int RoleId { get; set; }

  }
}