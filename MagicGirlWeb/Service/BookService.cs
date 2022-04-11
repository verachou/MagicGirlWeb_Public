using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MagicGirlWeb.Models;
using MagicGirlWeb.Repository;
using MagicGirlWeb.Data;

namespace MagicGirlWeb.Service
{
  public class BookService : IBookService
  {
    private readonly ILogger _logger;
    private readonly UnitOfWork _unitOfWork;
    public BookService(ILoggerFactory loggerFactory, MagicContext context)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
      _unitOfWork = new UnitOfWork(context);      
    }

    public Book GetBookByUrl(string url)
    {
      return _unitOfWork.BookRepository.GetByUrl(url);
    }

    public Book GetBookBySourceId(string sourceId)
    {
      return _unitOfWork.BookRepository.GetBySourceId(sourceId);
    }

    public IEnumerable<BookDownload> GetBookDownloadAll()
    {
      return _unitOfWork.BookDownloadRepository.GetAll();
    }

    public Book InsertBook(
      string bookName,
      string authorName,
      int totalPage,
      string url,
      string sourceId,
      int lastPageFrom,
      int lastPageTo,
      int type = Constant.BOOK_TYPE_NOVEL,
      int status = Constant.BOOK_STATUS_UNKNOWN
    )
    {
      // 檢查該作者是否已建檔，如為新作者則先新增Author
      Author author = _unitOfWork.AuthorRepository.GetByName(authorName);
      if (author == null)
      {
        author = new Author(authorName);
        _unitOfWork.AuthorRepository.Insert(author);
        _unitOfWork.Save();
      }

      // 檢查Book主檔是否已建檔
      Book book = _unitOfWork.BookRepository.GetByNameAndAuthor(bookName, author);
      if (book == null)
      {
        book = new Book(bookName, author.Id, totalPage, type, status);
        _unitOfWork.BookRepository.Insert(book);
        _unitOfWork.Save();
      }
      else
      {
        book.TotalPage = totalPage;
        book.Type = type;
        book.Status = status;
        _unitOfWork.BookRepository.Update(book);
      }

      BookWebsite bookWebsite = new BookWebsite(
            url,
            book.Id,
            sourceId,
            lastPageFrom,
            lastPageTo
            );
      _unitOfWork.BookWebsiteRepository.Insert(bookWebsite);

      _unitOfWork.Save();

      return book;
    }

    public Book UpdateBook(
      BookWebsite bookWebsite,
      int totalPage,
      int lastPageFrom,
      int lastPageTo,
      int status = Constant.BOOK_STATUS_UNKNOWN
    )
    {
      if (bookWebsite.LastPageFrom != lastPageFrom || bookWebsite.LastPageTo != lastPageTo)
      {
        bookWebsite.LastPageFrom = lastPageFrom;
        bookWebsite.LastPageTo = lastPageTo;
        _unitOfWork.BookWebsiteRepository.Update(bookWebsite);
      }

      if (bookWebsite.Book.TotalPage < totalPage)
      {
        bookWebsite.Book.TotalPage = totalPage;
        _unitOfWork.BookRepository.Update(bookWebsite.Book);
      }

      if (bookWebsite.Book.Status != Constant.BOOK_STATUS_ENDING && bookWebsite.Book.Status != status)
      {
        bookWebsite.Book.Status = status;
        _unitOfWork.BookRepository.Update(bookWebsite.Book);
      }

      _unitOfWork.Save();

      return bookWebsite.Book;
    }

    // 情境：寄送mail
    // public void InsertBookDownload(
    //   BookWebsite bookWebsite,
    //   Account account,
    //   ICollection<string> mails,
    //   int downloadStatus
    // )
    // {
    //   foreach (string mail in mails)
    //   {
    //     BookDownload bookDownload = new BookDownload(mail, bookWebsite, account, downloadStatus);
    //     this._unitOfWork.BookDownloadRepository.Insert(bookDownload);
    //   }

    //   this._unitOfWork.Save();
    // }

    public void InsertBookDownload(
      string url,
      string accountId,
      int downloadStatus,
      int pageFrom,
      int pageTo,
      ICollection<string> mails
    )
    {
      Book book = _unitOfWork.BookRepository.GetByUrl(url);


      foreach (var mail in mails)
      {
        foreach (var bw in book.BookWebsites)
        {
          BookDownload bookDownload = new BookDownload(bw.Id, accountId);
          bookDownload.Email = mail;
          bookDownload.DownloadStatus = downloadStatus;
          bookDownload.PageFrom = pageFrom;
          bookDownload.PageTo = pageTo;
          _unitOfWork.BookDownloadRepository.Insert(bookDownload);
        }
      }

      _unitOfWork.Save();
    }

    public void InsertBookDownload(
      string url,
      string accountId,
      int downloadStatus,
      int pageFrom,
      int pageTo
    )
    {
      Book book = _unitOfWork.BookRepository.GetByUrl(url);

      foreach (var bw in book.BookWebsites)
      {
        BookDownload bookDownload = new BookDownload(bw.Id, accountId);
        bookDownload.DownloadStatus = downloadStatus;
        bookDownload.PageFrom = pageFrom;
        bookDownload.PageTo = pageTo;
        _unitOfWork.BookDownloadRepository.Insert(bookDownload);
      }

      _unitOfWork.Save();
    }

    public void UpdateFileId(
      string sourceId,
      string fileId,
      string folderId
    )
    {
      Book book = GetBookBySourceId(sourceId);
      BookWebsite bw = book.BookWebsites.FirstOrDefault();
      bw.FileId = fileId;
      bw.FolderId = folderId;

      _unitOfWork.BookWebsiteRepository.Update(bw);
      _unitOfWork.Save();

    }

  }
}