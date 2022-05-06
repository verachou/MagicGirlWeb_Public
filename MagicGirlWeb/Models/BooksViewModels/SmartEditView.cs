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
    public string JsonFile { get; set; }


    [Required]
    public string SelectedEncoding { get; set; }
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

    public class FilepondFile
    {
      public string id { get; set; }
      public string name { get; set; }
      public string type { get; set; }
      public int size { get; set; }
      public string data { get; set; }
    }

  }
}
