using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MagicGirlWeb.Models.BooksViewModels
{
  public class DepositoryView
  {
    public string SelectedDepoId {get;set;}

    public IList<SelectListItem> Depositorys { get; set; }       

    public IList<DepositoryView.File> Files {get; set;}

    public IList<DepositoryView.AccountEmail> AccountEmails { get; set; }

    public class File 
    {
      public string Id { get; set; }
      [StringLength(50, ErrorMessage = "Cannot be longer than 50 characters.")]

      public string Name { get; set; }

      public int Size { get; set; }

      public string Description {get; set;}

    }

    public class AccountEmail
    {
      public int EmailId { get; set; }
      public string Description { get; set; }
      public bool IsChecked { get; set; } = false;
    }

    
    

  }
}