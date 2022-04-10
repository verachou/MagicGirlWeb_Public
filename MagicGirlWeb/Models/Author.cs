using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  // [Table("AUTHOR")]
  public class Author : ObjectModel
  {    
    // Model Relation
    public virtual ICollection<Book> Books { get; set; }
    
    public Author(string name)
    {
      Name = name;
    }

  }
}