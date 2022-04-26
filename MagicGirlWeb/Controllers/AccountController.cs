using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MagicGirlWeb.Data;
using MagicGirlWeb.Models;
using MagicGirlWeb.Models.AccountViewModels;
using MagicGirlWeb.Service;

namespace MagicGirlWeb
{
  public class AccountController : Controller
  {
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAccountService _accountService;
    public AccountController(
        ILogger<AccountController> logger,
        ILoggerFactory loggerFactory,
        MagicContext context,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
      _logger = logger;
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _accountService = new AccountService(loggerFactory, context);
    }
    public IActionResult Index()
    {
      return View();
    }

    [AllowAnonymous]
    public async Task<IActionResult> Logout(string returnUrl = null)
    {
      await _signInManager.SignOutAsync();
      _logger.LogInformation("User logged out.");

      return RedirectToLocal(returnUrl);
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
      if (provider == null)
        return RedirectToLocal(returnUrl);

      // Request a redirect to the external login provider.
      var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
      var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
      return Challenge(properties, provider);
    }

    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
      if (remoteError != null)
      {
        _logger.LogError("Error from external provider: {0}", remoteError);
        // return RedirectToAction(nameof(Login));
        return RedirectToLocal(returnUrl);
      }

      var info = await _signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        return RedirectToLocal(returnUrl);
      }

      // Sign in the user with this external login provider if the user already has a login.
      var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
      string[] userInfo = { info.Principal.FindFirst(ClaimTypes.Name).Value, info.Principal.FindFirst(ClaimTypes.Email).Value };
      if (result.Succeeded)
      {
        // 使用者帳號已存在，可以直接前往目的地
        return RedirectToLocal(returnUrl);
      }
      if (result.IsLockedOut)
      {
        // 使用者帳號被鎖定
        // return RedirectToAction(nameof(Lockout));
        return RedirectToLocal(returnUrl);
      }
      else
      {
        // 使用者帳號不存在, 自動建立新帳號        
        var user = new IdentityUser()
        {
          Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
          UserName = info.Principal.FindFirst(ClaimTypes.Email).Value
        };

        IdentityResult identResult = await _userManager.CreateAsync(user);
        if (identResult.Succeeded)
        {
          identResult = await _userManager.AddLoginAsync(user, info);
          if (identResult.Succeeded)
          {
            await _signInManager.SignInAsync(user, false);
            return RedirectToLocal(returnUrl);
          }
        }
      }
      return RedirectToLocal(returnUrl);
    }

    private ActionResult RedirectToLocal(string returnUrl)
    {
      if (Url.IsLocalUrl(returnUrl))
      {
        return Redirect(returnUrl);
      }
      else
      {
        return RedirectToAction("Index", "Home");
      }
    }

    // GET: Account/EmailSetting
    [HttpGet]
    public async Task<IActionResult> EmailSetting()
    {
      var accountId = _userManager.GetUserId(User);
      ICollection<AccountEmail> acoountEmails = _accountService.GetEmailByAccountId(accountId);
      EmailListView viewModel = new EmailListView();
      ICollection<EmailListView.EmailView> emailViews = new List<EmailListView.EmailView>();
      foreach (var item in acoountEmails)
      {
        var emailView = new EmailListView.EmailView();
        emailView.EmailId = item.Id;
        emailView.Email = item.Email;
        emailView.Description = item.Description;
        emailViews.Add(emailView);
      }

      viewModel.EmailViews = emailViews;
      return View(viewModel);
    }


    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmailSetting(EmailListView viewModel)
    {
      if (ModelState.IsValid)
      {
        var accountId = _userManager.GetUserId(User);
        var accountEmail = _accountService.InsertAccountEmail(accountId, viewModel.EditEmail, viewModel.EditDescription);
        if (accountEmail == null)
        {
          _logger.LogWarning(CustomMessage.InsertFail, accountId, "AccountEmail");
          return NotFound();
        }
        else
        {
          viewModel.EditEmail = null;
          viewModel.EditDescription = null;
        }
      }
      return RedirectToAction(nameof(EmailSetting));
    }

    public async Task<IActionResult> EmailSettingDelete(int emailId)
    {
      _accountService.DeleteAccountEmail(emailId);
      return RedirectToAction(nameof(EmailSetting));
    }

    // GET: Account/EmailSetting/ADMIN
    // [HttpGet]
    public async Task<IActionResult> RoleSetting(RoleView viewModel)
    {
      if (viewModel.Roles == null)
        viewModel.Roles = _roleManager.Roles.Select(o => new SelectListItem(o.Name, o.Id)).ToList();

      if (viewModel.SelectedRoleId == null)
        viewModel.SelectedRoleId = viewModel.Roles.FirstOrDefault().Value;

      var users = _userManager.Users;
      var identityRole = _roleManager.FindByIdAsync(viewModel.SelectedRoleId).Result;
      var usersInRole = _userManager.GetUsersInRoleAsync(identityRole.Name).Result;
      var accounts = new List<RoleView.Account>();

      foreach (var user in users)
      {
        var account = new RoleView.Account();
        account.Id = user.Id;
        account.Name = user.UserName;
        if (usersInRole.Contains(user))
          account.IsChecked = true;
        else
          account.IsChecked = false;
        accounts.Add(account);
      }
      viewModel.Accounts = accounts;
      ModelState.Clear();

      return View(viewModel);
    }

    // Post: Account/EmailSetting
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult RoleSettingSave(RoleView viewModel)
    {
      if (ModelState.IsValid && viewModel.Accounts != null)
      {
        foreach (var user in viewModel.Accounts)
        {
          var identityUser = _userManager.FindByIdAsync(user.Id).Result;
          var identityRole = _roleManager.FindByIdAsync(viewModel.SelectedRoleId).Result;

          if (user.IsChecked == true)
            _userManager.AddToRoleAsync(identityUser, identityRole.Name).Wait();
          else
            _userManager.RemoveFromRoleAsync(identityUser, identityRole.Name).Wait();
        }
      }

      return RedirectToAction(nameof(RoleSetting), viewModel);
    }
  }
}