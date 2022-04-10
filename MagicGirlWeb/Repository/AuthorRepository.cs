using System.Linq;
using MagicGirlWeb.Models;
using Microsoft.EntityFrameworkCore;
using MagicGirlWeb.Data;

namespace MagicGirlWeb.Repository
{
  public class AuthorRepository : GenericRepository<Author>, IAuthorRepository
  {
    // internal DbContext _context;

    public AuthorRepository(MagicContext context) : base(context)
    {
    }
    public Author GetByName(string name)
    {
      return _context.Author.Where(s => s.Name == name).FirstOrDefault();
    }
  }
}