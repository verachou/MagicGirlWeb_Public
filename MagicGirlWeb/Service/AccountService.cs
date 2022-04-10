using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MagicGirlWeb.Models;
using MagicGirlWeb.Repository;
using MagicGirlWeb.Data;

namespace MagicGirlWeb.Service
{
  public class AccountService : IAccountService
  {
    private readonly ILogger _logger;
    private readonly UnitOfWork _unitOfWork;
    public AccountService(ILoggerFactory loggerFactory, MagicContext context)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
      _unitOfWork = new UnitOfWork(context);
    }

    // public ICollection<AccountEmail> GetEmailByAccountId(int accountId)
    // {
    //   Account account = _unitOfWork.AccountRepository.GetByAccountId(accountId);
    //   return account.AccountEmails;   
    // }

    public ICollection<AccountEmail> GetEmailAll()
    {
      
      return _unitOfWork.AccountEmailRepository.GetAll().ToList();
    }

  }
}