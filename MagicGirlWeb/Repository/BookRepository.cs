using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MagicGirlWeb.Models;
using MagicGirlWeb.Data;

namespace MagicGirlWeb.Repository
{
  public class BookRepository : GenericRepository<Book>, IBookRepository
  {
    // internal DbContext _context;

    public BookRepository(MagicContext context) : base(context)
    {
    }
    public ICollection<Book> GetByName(string name)
    {
      return _context.Book
        .Include(b => b.Author)
        .Include(b => b.BookWebsites)
        .Where(s => s.Name == name).ToList();
    }

    public Book GetByNameAndAuthor(string name, Author author)
    {
      return _context.Book
        .Include(b => b.Author)
        .Include(b => b.BookWebsites)
        .Where(b => b.Name == name)
        .Where(b => b.Author == author)
        .FirstOrDefault();
    }

    public Book GetByUrl(string url)
    {
      var bookWebsite = _context.BookWebsite
        .Include(bw => bw.Book)
        .Where(b => b.Url == url)
        .FirstOrDefault();

      return _context.Book
        .Include(b => b.Author)
        .Include(b => b.BookWebsites)
        .Where(b => b.Id == bookWebsite.Book.Id)
        .FirstOrDefault();
    }

    public Book GetBySourceId(string url)
    {
      var bookWebsite = _context.BookWebsite
        .Include(bw => bw.Book)
        .Where(b => b.SourceId == url)
        .FirstOrDefault();

      if(bookWebsite == null)
      {
        return null;
      }

      return _context.Book
        .Include(b => b.Author)
        .Include(b => b.BookWebsites)
        .Where(b => b.Id == bookWebsite.Book.Id)
        .FirstOrDefault();
    }

  }
}