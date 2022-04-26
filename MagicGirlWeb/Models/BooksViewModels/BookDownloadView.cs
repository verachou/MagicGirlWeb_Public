using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MagicGirlWeb.Models.BooksViewModels
{
  public class BookDownloadView
  {
    public IList<BookDownloadView.BookDownload> BookDownloads { get; set; }    

    public IList<BookDownloadView.AccountEmail> AccountEmails { get; set; }

    public class BookDownload 
    {
      public int Id { get; set; }
      [StringLength(50, ErrorMessage = "Cannot be longer than 50 characters.")]

      public string Title { get; set; }

      public int TotalPage { get; set; }

      [DataType(DataType.DateTime)]
      [DisplayFormat(DataFormatString = "{0:G}")]  // 6/15/2009 1:45:30 PM
      public DateTime DownloadDate { get; set; }

      public string SourceId { get; set; }

    }

    public class AccountEmail
    {
      public int EmailId { get; set; }
      public string Description { get; set; }
      public bool IsChecked { get; set; } = false;
    }

    

  }
}