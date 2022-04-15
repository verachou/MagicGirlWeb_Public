using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MagicGirlWeb.Models.AccountViewModels
{
  public class EmailListView
  {
    [Required]
    [EmailAddress]
    [StringLength(100, ErrorMessage = "Cannot be longer than 100 characters.")]
    public string EditEmail { get; set; }

    [Required]
    [StringLength(20, ErrorMessage = "Cannot be longer than 20 characters.")]
    public string EditDescription { get; set; }

    public IEnumerable<EmailView> EmailViews { get; set; }
    public class EmailView
    {
      public int EmailId { get; set; }

      [DataType(DataType.EmailAddress)]
      [StringLength(100, ErrorMessage = "Cannot be longer than 100 characters.")]
      public string Email { get; set; }

      [StringLength(20, ErrorMessage = "Cannot be longer than 20 characters.")]
      public string Description { get; set; }
    }
  }
}