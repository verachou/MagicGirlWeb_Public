using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MagicGirlWeb.Repository
{
  public interface IGenericRepository<TEntity> where TEntity : class
  {
    IEnumerable<TEntity> GetAll();
    TEntity GetById(object id);
    TEntity Insert(TEntity obj);
    TEntity Update(TEntity obj);
    TEntity Delete(object id);
  }
}