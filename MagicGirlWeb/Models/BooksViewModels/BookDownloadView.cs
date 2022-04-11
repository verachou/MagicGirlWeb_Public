using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MagicGirlWeb.Models.BooksViewModels
{
  public class BookDownloadView
  {
    // public IEnumerable<BookDownloadView> BookDownloads { get; set; }

    public int Id { get; set; }
    [StringLength(50, ErrorMessage = "Cannot be longer than 50 characters.")]

    public string Title { get; set; }

    public int TotalPage { get; set; }

    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:G}")]  // 6/15/2009 1:45:30 PM
    public DateTime DownloadDate { get; set; }

    public string SourceId { get; set; }

  }
}