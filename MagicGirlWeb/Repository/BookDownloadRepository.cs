using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MagicGirlWeb.Models;
using MagicGirlWeb.Data;


namespace MagicGirlWeb.Repository
{
  public class BookDownloadRepository : GenericRepository<BookDownload>, IBookDownloadRepository
  {
    // internal DbContext _context;

    public BookDownloadRepository(MagicContext context) : base(context)
    {
    }
    public override IEnumerable<BookDownload> GetAll()
    {
      return _context.BookDownload
        .Include(bd => bd.BookWebsite)
          .ThenInclude(bw => bw.Book)
            .ThenInclude(b => b.Author)
        .ToList();
    }
  }
}