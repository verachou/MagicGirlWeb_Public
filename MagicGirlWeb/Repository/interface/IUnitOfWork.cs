using System;
using MagicGirlWeb.Models;

namespace MagicGirlWeb.Repository
{
  public interface IUnitOfWork : IDisposable
  {
    IAuthorRepository AuthorRepository { get; }
    IBookRepository BookRepository { get; }
    IGenericRepository<BookWebsiteRepository> BookWebsiteRepository { get; }
    IGenericRepository<BookDownload> BookDownloadRepository { get; }

    void Save();




  }
}