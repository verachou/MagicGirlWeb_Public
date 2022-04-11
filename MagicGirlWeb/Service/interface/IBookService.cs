using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MagicGirlWeb.Models;
using MagicGirlWeb.Repository;

namespace MagicGirlWeb.Service
{
  public interface IBookService
  {
    Book GetBookByUrl(string url);

    Book GetBookBySourceId(string sourceId);

    IEnumerable<BookDownload> GetBookDownloadAll();
    
    Book InsertBook(
      string bookName,
      string authorName,
      int totalPage,
      string url,
      string sourceId,
      int lastPageFrom,
      int lastPageTo,
      int type = Constant.BOOK_TYPE_NOVEL,
      int status = Constant.BOOK_STATUS_UNKNOWN
    );

    Book UpdateBook(
      BookWebsite bookWebsite,
      int totalPage,
      int lastPageFrom,
      int lastPageTo,
      int status = Constant.BOOK_STATUS_UNKNOWN
    );

    // 情境：寄送mail
    void InsertBookDownload(
      string url,
      string accountId,
      int downloadStatus,
      int pageFrom,
      int pageTo,
      ICollection<string> mails
    );

    // 情境：下載檔案
    void InsertBookDownload(
      string url,
      string accountId,
      int downloadStatus,
      int pageFrom,
      int pageTo
    );

    void UpdateFileId(
      string sourceId,
      string fileId,
      string folderId
    );
  }
}

