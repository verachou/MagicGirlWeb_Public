using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models.Interface
{
  [Table("ROLE")]
  public class RoleModel : ObjectModel
  {
    [StringLength(50, ErrorMessage = "Cannot be longer than 50 characters.")]
    [Column("description")]
    public string Description { get; set; }

  }
}