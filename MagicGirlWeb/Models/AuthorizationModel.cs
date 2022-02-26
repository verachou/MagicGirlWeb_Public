using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  [Table("AUTHORIZATION")]
  public class AuthorizationModel: ObjectRelationModel
  {
    [ForeignKey("RoleModel")]
    [Column("role_id")]
    public int RoleId { get; set; }
    
    [ForeignKey("FunctionModel")]
    [Column("function_id")]
    public int FunctionId { get; set; }

  }
}