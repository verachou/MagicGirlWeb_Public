using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using MagicGirlWeb.Models.DataAnnotaions;

namespace MagicGirlWeb.Models.BooksViewModels
{
  public class SmartEditView
  {
    [Required(ErrorMessage = "Error: 請選取正確的檔案")]
    // [AllowFileExtensions("txt", ErrorMessage = "Error: 僅接受txt檔")]
    [FileSize(20*1024*1024, ErrorMessage = "Error: 超過檔案大小上限20MB")]
    public IFormFile TxtFile { get; set; }

    [Required]
    public SelectListItem SelectedEncoding { get; set; }
    public IEnumerable<SelectListItem> Encodings { get; } = new List<SelectListItem>
    {
      new SelectListItem { Value = "gb2312", Text = "簡體中文GB2312" },
      new SelectListItem { Value = "big5", Text = "繁體中文BIG5" },
      new SelectListItem { Value = "utf-8", Text = "UTF-8"  },
    };

    public bool ToTW { get; set; }

    public bool DoubleEOL { get; set; }

    public bool IsDownload { get; set; } = false;
    public bool IsEmail { get; set; } = false;

    public IList<SmartEditView.AccountEmail> AccountEmails { get; set; }

    public class AccountEmail
    {
      public int EmailId { get; set; }
      public string Description { get; set; }
      public bool IsChecked { get; set; } = false;

    }

  }
}
