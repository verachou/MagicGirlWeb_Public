using MagicGirlWeb.Models;

namespace MagicGirlWeb.Repository
{
  public interface IAuthorRepository : IGenericRepository<Author>
  {
    Author GetByName(string name);
  }
}