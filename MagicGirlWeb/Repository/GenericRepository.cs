using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using MagicGirlWeb.Data;



// 使用Microsoft官方提供的泛型Repository程式碼範例：
//   https://docs.microsoft.com/zh-tw/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application#implement-a-generic-repository-and-a-unit-of-work-class
// async參考資料：
//   https://codingblast.com/entity-framework-core-generic-repository/
namespace MagicGirlWeb.Repository
{
  public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
  {
    protected internal MagicContext _context;
    protected internal DbSet<TEntity> _dbSet;

    public GenericRepository(MagicContext context)
    {
      this._context = context;
      this._dbSet = context.Set<TEntity>();
    }

    public virtual IEnumerable<TEntity> GetAll()
    {
      return _dbSet.AsNoTracking().ToList();
    }

    public virtual TEntity GetById(object id)
    {
      return _dbSet.Find(id);
    }

    public virtual TEntity Insert(TEntity entity)
    {
      if (entity == null)
      {
        throw new ArgumentNullException("entity");
      }
     return _dbSet.Add(entity).Entity;
    }

    public virtual TEntity Update(TEntity entityToUpdate)
    {
      if (entityToUpdate == null)
      {
        throw new ArgumentNullException("entity");
      }

      TEntity entity = _dbSet.Attach(entityToUpdate).Entity;
      _context.Entry(entityToUpdate).State = EntityState.Modified;
      return entity;
    }

    public virtual TEntity Delete(object id)
    {
      if (id == null)
      {
        throw new ArgumentNullException("id");
      }

      TEntity entityToDelete = _dbSet.Find(id);
      return Delete(entityToDelete);
    }

    public virtual TEntity Delete(TEntity entityToDelete)
    {
      if (entityToDelete == null)
      {
        throw new ArgumentNullException("entity");
      }

      if (_context.Entry(entityToDelete).State == EntityState.Detached)
      {
        _dbSet.Attach(entityToDelete);
      }
      return _dbSet.Remove(entityToDelete).Entity;
    }

  
  }
}