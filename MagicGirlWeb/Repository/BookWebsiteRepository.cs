using System.Linq;
using Microsoft.EntityFrameworkCore;
using MagicGirlWeb.Models;
using MagicGirlWeb.Data;


namespace MagicGirlWeb.Repository
{
  public class BookWebsiteRepository : GenericRepository<BookWebsite>, IBookWebsiteRepository
  {
    // internal DbContext _context;

    public BookWebsiteRepository(MagicContext context) : base(context)
    {
    }
    // public BookWebsite GetByUrl(string url)
    // {
    //   return _context.BookWebsite.Where(s => s.Url == url).FirstOrDefault();
    // }
  }
}