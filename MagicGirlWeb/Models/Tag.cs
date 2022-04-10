using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  // [Table("TAG")]
  public class Tag : ObjectModel
  {
    // Model Relation
    public virtual ICollection<Book> Books { get; set; }

  }
}