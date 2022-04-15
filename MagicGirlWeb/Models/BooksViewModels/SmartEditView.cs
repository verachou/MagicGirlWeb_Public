using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MagicGirlWeb.Models.DataAnnotaions;

namespace MagicGirlWeb.Models.BooksViewModels
{
  public class SmartEditView
  {
    [Required(ErrorMessage = "Error: 請選取正確的檔案")]
    [AllowFileExtensions("txt", ErrorMessage = "Error: 僅接受txt檔")]
    [FileSize(20*1024*1024, ErrorMessage = "Error: 超過檔案大小上限20MB")]
    public IFormFile TxtFile { get; set; }

    [Required]
    public string Encoding { get; set; } = "gb2312";

    // public string[] Encodings = new[] { "gb2312", "big5", "utf-8" };

    // public string[] EncodingNames = new[] { "簡體中文GB2312", "繁體中文BIG5", "UTF-8" };

    public bool ToTW { get; set; }

    public bool DoubleEOL { get; set; }

  }
}
