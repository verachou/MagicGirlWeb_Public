using System.Collections.Generic;
using MagicGirlWeb.Models;

namespace MagicGirlWeb.Repository
{
  public interface IAccountEmailRepository : IGenericRepository<AccountEmail>
  {
    AccountEmail GetByAccountAndEmail(string accountId, string Email);
    ICollection<AccountEmail> GetByAccountId(string accountId);
  }
}