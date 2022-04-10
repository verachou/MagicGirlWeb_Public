using System.Collections.Generic;
using MagicGirlWeb.Models;

namespace MagicGirlWeb.Service
{
  interface IAccountService
  {
    // ICollection<AccountEmail> GetEmailByAccountId(int id);
    ICollection<AccountEmail> GetEmailAll();

  }
}