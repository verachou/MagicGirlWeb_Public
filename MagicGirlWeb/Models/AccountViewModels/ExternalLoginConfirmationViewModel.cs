using System;
using System.ComponentModel.DataAnnotations;

namespace MagicGirlWeb.Models.AccountViewModels
{
  public class ExternalLoginConfirmationViewModel
  {
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    public string Provider { get; set; }

  }
}