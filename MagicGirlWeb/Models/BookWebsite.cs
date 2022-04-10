using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  // [Table("BOOK_WEBSITE")]
  public class BookWebsite : ObjectDetailModel
  {
    [ForeignKey("BookModel")]
    [Column("book_id")]
    public int BookId { get; set; }

    [StringLength(500, ErrorMessage = "Cannot be longer than 500 characters.")]
    [DataType(DataType.Url)]
    [Column("url")]
    public string Url { get; set; }

    // [StringLength(50, ErrorMessage = "Cannot be longer than 50 characters.")]
    // [Column("plugin")]
    public string Plugin { get; set; }

    [Column("task_status")]
    public int TaskStatus { get; set; } = Constant.BOOK_STATUS_UNKNOWN;

    [Column("last_page_from")]
    public int LastPageFrom { get; set; } = -1;

    [Column("last_page_to")]
    public int LastPageTo { get; set; } = -1;

    // 下載來源識別碼
    // hash(網站代碼 + 書籍網址識別碼), 用來判斷此網站來源是否已下載過
    [StringLength(32, ErrorMessage = "Cannot be longer than 32 characters.")]
    [Column("source_id")]
    public string SourceId { get; set; }

    // Google Drive的FileId
    [StringLength(200, ErrorMessage = "Cannot be longer than 200 characters.")]
    [Column("file_id")]
    public string FileId { get; set; }

    // Google Drive上File的ParentId
    [StringLength(200, ErrorMessage = "Cannot be longer than 200 characters.")]
    [Column("folder_id")]
    public string FolderId { get; set; }

    // Model Relation
    public virtual Book Book { get; set; }

    public virtual ICollection<BookDownload> BookDownloads { get; set; }

    public BookWebsite(
      string url,
      int bookId, 
      string sourceId, 
      int lastPageFrom, 
      int lastPageTo
      )
    {
      Url = url;
      BookId = bookId;
      SourceId = sourceId;
      LastPageFrom = lastPageFrom;
      LastPageTo = lastPageTo;
    }

  }
}