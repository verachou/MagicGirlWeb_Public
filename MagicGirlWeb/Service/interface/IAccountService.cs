using System.Collections.Generic;
using MagicGirlWeb.Models;

namespace MagicGirlWeb.Service
{
  interface IAccountService
  {
    ICollection<AccountEmail> GetEmailByAccountId(string id);
    ICollection<AccountEmail> GetEmailAll();

    AccountEmail InsertAccountEmail(
      string accountId, 
      string email,
      string description
    );

    void DeleteAccountEmail(
      int accountEmailId
    );

  }
}