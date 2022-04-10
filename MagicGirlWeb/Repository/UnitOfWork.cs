using System;
using Microsoft.EntityFrameworkCore;
using MagicGirlWeb.Models;
using MagicGirlWeb.Data;

// 使用Microsoft官方提供的泛型Repository程式碼範例：
//   https://docs.microsoft.com/zh-tw/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application#implement-a-generic-repository-and-a-unit-of-work-class
namespace MagicGirlWeb.Repository
{
  public class UnitOfWork : IDisposable
  {
    private MagicContext _context;
    // private AccountRepository _accountRepository;
    private GenericRepository<AccountEmail> _accountEmailRepository;
    private AuthorRepository _authorRepository;
    private BookRepository _bookRepository;
    private GenericRepository<BookWebsite> _bookWebsiteRepository;
    private GenericRepository<BookDownload> _bookDownloadRepository;

    public UnitOfWork(MagicContext context)
    {
      _context = context;
    }

    // public AccountRepository AccountRepository
    // {
    //   get
    //   {
    //     if (this._accountRepository == null)
    //     {
    //       this._accountRepository = new AccountRepository(_context);
    //     }
    //     return _accountRepository;
    //   }
    // }

    public GenericRepository<AccountEmail> AccountEmailRepository
    {
      get
      {
        if (this._accountEmailRepository == null)
        {
          this._accountEmailRepository = new GenericRepository<AccountEmail>(_context);
        }
        return _accountEmailRepository;
      }
    }

    public AuthorRepository AuthorRepository
    {
      get
      {
        if (this._authorRepository == null)
        {
          this._authorRepository = new AuthorRepository(_context);
        }
        return _authorRepository;
      }
    }
    public BookRepository BookRepository
    {
      get
      {
        if (this._bookRepository == null)
        {
          this._bookRepository = new BookRepository(_context);
        }
        return _bookRepository;
      }
    }

    public GenericRepository<BookWebsite> BookWebsiteRepository
    {
      get
      {
        if (this._bookWebsiteRepository == null)
        {
          this._bookWebsiteRepository = new GenericRepository<BookWebsite>(_context);
        }
        return _bookWebsiteRepository;
      }
    }

    public GenericRepository<BookDownload> BookDownloadRepository
    {
      get
      {
        if (this._bookDownloadRepository == null)
        {
          this._bookDownloadRepository = new GenericRepository<BookDownload>(_context);
        }
        return _bookDownloadRepository;
      }
    }

    public void Save()
    {
      _context.SaveChanges();
    }

    private bool _disposed = false;
    protected virtual void Dispose(bool _disposing)
    {
      if (!this._disposed)
      {
        if (_disposing)
        {
          _context.Dispose();
        }
      }
      this._disposed = true;
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

  }
}