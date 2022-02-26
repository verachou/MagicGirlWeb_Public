using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  [Table("BOOK")]
  public class BookModel : ObjectModel
  {
    [Column("total_page")]
    public int TotalPage { get; set; } = 0;

    // 書籍類型
    [Column("type")]
    public int Type { get; set; } = Constant.BOOK_TYPE_NOVEL;

    // 書籍狀態
    // 紀錄是否已完結
    public int Status { get; set; } = Constant.BOOK_STATUS_UNKNOWN;

  }
}