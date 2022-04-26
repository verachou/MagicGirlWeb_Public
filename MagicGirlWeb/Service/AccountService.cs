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

    public AccountEmail GetEmailById(int id)
    {
      return _unitOfWork.AccountEmailRepository.GetById(id);
    }

    public ICollection<AccountEmail> GetEmailByAccountId(string accountId)
    {
      return _unitOfWork.AccountEmailRepository.GetByAccountId(accountId);
    }

    public ICollection<AccountEmail> GetEmailAll()
    {
      return _unitOfWork.AccountEmailRepository.GetAll().ToList();
    }

    public AccountEmail InsertAccountEmail(
      string accountId,
      string email,
      string description)
    {
      if (email == null || description == null)
        return null;

      // 檢查是否已建檔
      AccountEmail accountEmail = _unitOfWork.AccountEmailRepository.GetByAccountAndEmail(accountId, email);
      if (accountEmail == null)
      {
        accountEmail = new AccountEmail();
        accountEmail.AccountId = accountId;
        accountEmail.Email = email;
        accountEmail.Description = description;

        _unitOfWork.AccountEmailRepository.Insert(accountEmail);
        _unitOfWork.Save();
      }
      else
      {
        accountEmail.Email = email;
        accountEmail.Description = description;
        _unitOfWork.AccountEmailRepository.Update(accountEmail);
        _unitOfWork.Save();
      }

      return accountEmail;
    }

    public void DeleteAccountEmail(int accountEmailId)
    {
      _unitOfWork.AccountEmailRepository.Delete(accountEmailId);
      _unitOfWork.Save();

    }

  }
}