using System.Collections.Generic;
using MagicGirlWeb.Models;

namespace MagicGirlWeb.Service
{
  interface IAccountService
  {
    AccountEmail GetEmailById(int id);
    ICollection<AccountEmail> GetEmailByAccountId(string accountId);
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