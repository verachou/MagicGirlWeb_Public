using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  [Table("BOOK_TAG")]
  public class BookTagModel: ObjectRelationModel
  {
    [ForeignKey("BookModel")]
    [Column("book_id")]
    public int BookId { get; set; }
    
    [ForeignKey("TagModel")]
    [Column("tag_id")]
    public int TagId { get; set; }

  }
}