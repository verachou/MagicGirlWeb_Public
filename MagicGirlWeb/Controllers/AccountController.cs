using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MagicGirlWeb.Models.AccountViewModels;

namespace MagicGirlWeb
{
  public class AccountController : Controller
  {
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(
        ILogger<AccountController> logger,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
      _logger = logger;
      _userManager = userManager;
      _signInManager = signInManager;
    }
    public IActionResult Index()
    {
      return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
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
  }
}