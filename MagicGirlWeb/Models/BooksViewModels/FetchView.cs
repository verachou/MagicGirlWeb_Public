using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MagicGirlWeb.Models.BooksViewModels
{
  public class FetchView
  {
    [StringLength(50, ErrorMessage = "Cannot be longer than 50 characters.")]
    public string Title { get; set; }

    [StringLength(500, ErrorMessage = "Cannot be longer than 500 characters.")]
    [DataType(DataType.Url)]
    [Required]
    public string Url { get; set; }

    [Range(1, Int32.MaxValue)]
    [Required]
    public int PageFrom { get; set; } = 1;
    [Range(1, Int32.MaxValue)]
    [Required]
    public int PageTo { get; set; } = 1;

    public ICollection<AccountEmailView> AccountEmails { get; set; }

    [DataType(DataType.EmailAddress)]
    public string CustomEmail{ get; set; }

    public bool isDownload { get; set; } = true;

    public ICollection<string> SupportUrls { get; set; }

    public class AccountEmailView
    {
      [StringLength(100, ErrorMessage = "Cannot be longer than 100 characters.")]
      public string Email { get; set; }
      public string Description { get; set; }
      public bool Checked { get; set; } = false;

    }
  }
}