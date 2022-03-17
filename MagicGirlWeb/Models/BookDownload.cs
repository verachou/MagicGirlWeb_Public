using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  [Table("BOOK_DOWNLOAD")]
  public class BookDownload : ObjectDetailModel
  {
    // [ForeignKey("BookWebsiteModel")]
    // [Column("book_website_id")]
    // public int BookWebsiteId { get; set; }

    // // 下載者
    // [ForeignKey("AccountModel")]
    // [Column("account_id")]
    // public int AccountId { get; set; }

    // 寄送信箱
    // 因提供自行設定寄送信箱的功能，故此欄位資料不一定存在於AccountEmail
    [StringLength(100, ErrorMessage = "Cannot be longer than 100 characters.")]
    [DataType(DataType.EmailAddress)]
    [Column("email")]
    public string Email { get; set; }


    // 紀錄下載時的頁碼
    // Book_Website的last_page_from / last_page_to會被使用者更新，因此下載時的頁碼和Website紀錄的頁碼不一定相同
    [Column("page_from")]
    public int PageFrom { get; set; } = -1;

    [Column("page_to")]
    public int PageTo { get; set; } = -1;

    [Column("download_status")]
    public int DownloadStatus { get; set; } = Constant.DOWNLOAD_STATUS_FAIL;

    // Model Relation
    public virtual BookWebsite BookWebsite { get; set; }
    // 下載者
    public virtual Account Account { get; set; }

    public BookDownload (string email, BookWebsite bookWebsite, Account account, int DownloadStatus)
    {
      Email = email;
      PageFrom = bookWebsite.LastPageFrom;
      PageTo = bookWebsite.LastPageTo;
      DownloadStatus = DownloadStatus;
      Account = account;
      BookWebsite = bookWebsite;
    }
  }
}