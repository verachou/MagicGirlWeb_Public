using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  [Table("ACCOUNT")]
  public class Account : ObjectModel
  {
    [StringLength(10, ErrorMessage = "Cannot be longer than 10 characters.")]
    [Column("provider")]
    public string Provider { get; set; }

    // Model Relation
    public virtual ICollection<AccountEmail> AccountEmails { get; set; }
    public virtual ICollection<Role> Roles { get; set; }

  }
}