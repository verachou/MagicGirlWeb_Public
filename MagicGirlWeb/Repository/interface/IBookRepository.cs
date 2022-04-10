using System.Collections.Generic;
using MagicGirlWeb.Models;

namespace MagicGirlWeb.Repository
{
  public interface IBookRepository : IGenericRepository<Book>
  {
    ICollection<Book> GetByName(string name);
    Book GetByUrl(string url);
    Book GetByNameAndAuthor(string name, Author author);
    Book GetBySourceId(string sourceId);
  }
}