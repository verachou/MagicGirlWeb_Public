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

    public IList<FetchView.AccountEmail> AccountEmails { get; set; }

    [DataType(DataType.EmailAddress)]
    public string CustomEmail{ get; set; }

    public bool IsDownload { get; set; } = true;

    public ICollection<string> SupportUrls { get; set; }

    //下載百分比使用
    public string HubConnId { get; set; }
    public int ProgressPct { get; set; }

    public class AccountEmail
    {
      public int EmailId { get; set; }
      public string Description { get; set; }
      public bool IsChecked { get; set; } = false;      
    }
  }
}