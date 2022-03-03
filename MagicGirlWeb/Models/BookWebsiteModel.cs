using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  [Table("BOOK_WEBSITE")]
  public class BookWebsiteModel : ObjectDetailModel
  {
    [ForeignKey("BookModel")]
    [Column("book_id")]
    public int BookId { get; set; }

    [StringLength(500, ErrorMessage = "Cannot be longer than 500 characters.")]
    [DataType(DataType.Url)]
    [Column("url")]
    public string Url { get; set; }

    [StringLength(30, ErrorMessage = "Cannot be longer than 30 characters.")]
    [Column("web_domain")]
    public string WebDomain { get; set; }

    [Column("task_id")]
    public int TaskId { get; set; }

    [Column("task_status")]
    public int TaskStatus { get; set; } = Constant.BOOK_STATUS_UNKNOWN;

    [Column("last_page_from")]
    public int LastPageFrom { get; set; } = -1;

    [Column("last_page_to")]
    public int LastPageTo { get; set; } = -1;

    [StringLength(500, ErrorMessage = "Cannot be longer than 500 characters.")]
    [Column("file_path")] 
    public string FilePath { get; set; }

    // 下載來源識別碼
    //hash(網站代碼 + 書籍網址識別碼), 用來判斷此網站來源是否已下載過
    [StringLength(32, ErrorMessage = "Cannot be longer than 32 characters.")]
    [Column("source_id")]
    public string SourceId { get; set; }







  }
}