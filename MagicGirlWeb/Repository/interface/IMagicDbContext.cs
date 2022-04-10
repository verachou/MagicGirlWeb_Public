using System;
using Microsoft.EntityFrameworkCore;
using MagicGirlWeb.Models;

namespace MagicGirlWeb.Repository
{
  public interface IMagicDbContext : IDisposable
  {
    public DbSet<Author> Author { get; set; }
    public DbSet<Book> Book { get; set; }
    public DbSet<BookWebsite> BookWebsite { get; set; }
    public DbSet<BookDownload> BookDownload { get; set; }


  }
}