using System.Linq;
using System.Collections.Generic;
using MagicGirlWeb.Models;
using Microsoft.EntityFrameworkCore;
using MagicGirlWeb.Data;

namespace MagicGirlWeb.Repository
{
  public class AccountEmailRepository : GenericRepository<AccountEmail>, IAccountEmailRepository
  {
    // internal DbContext _context;

    public AccountEmailRepository(MagicContext context) : base(context)
    {
    }

    public AccountEmail GetByAccountAndEmail(string accountId, string email)
    {
      return _context.AccountEmail
        .Where(ae => ae.AccountId == accountId)
        .Where(ae => ae.Email == email)
        .FirstOrDefault();
    }

    public ICollection<AccountEmail> GetByAccountId(string accountId)
    {
      return _context.AccountEmail
        .Where(ae => ae.AccountId == accountId)
        .ToList();
    }
  }
}