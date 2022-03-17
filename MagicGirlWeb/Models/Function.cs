using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  [Table("FUNCTION")]
  public class Function : ObjectModel
  {
    [StringLength(50, ErrorMessage = "Cannot be longer than 50 characters.")]
    [Column("description")]
    public string Description { get; set; }

    // Model Relation
    public virtual ICollection<Role> Roles { get; set; }

  }
}