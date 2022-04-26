using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MagicGirlWeb.Models.AccountViewModels
{
  public class RoleView
  {
    public string SelectedRoleId { get; set; }
    // public IEnumerable<SelectListItem> Roles { get; } = new List<SelectListItem>
    // {
    //   new SelectListItem { Value = "ADMIN", Text = "Administrator" },
    //   new SelectListItem { Value = "ADVANCE_USER", Text = "Advanced User" },
    //   new SelectListItem { Value = "ADVANCE_GUEST", Text = "Advanced Guest"  },
    //   new SelectListItem { Value = "GUEST", Text = "Guest"  }
    // };
    public IList<SelectListItem> Roles { get; set; }

    public IList<Account> Accounts { get; set; }

    public class Account
    {
      public string Id { get; set; }
      public string Name { get; set; }
      public bool IsChecked { get; set; } = false;
    }

  }
}