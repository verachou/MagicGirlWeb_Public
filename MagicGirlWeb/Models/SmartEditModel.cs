using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MagicGirlWeb.Models
{
  public class SmartEditModel
  {
    [Required]
    [FileExtensions(Extensions = "txt", ErrorMessage = "Error: 僅接受txt檔")]
    public IFormFile TxtFile { get; set; }

    [Required]
    public string Encoding { get; set; } = "gb2312";

    public string[] Encodings = new[] { "gb2312", "big5", "utf8" };

    public string[] EncodingNames = new[] { "簡體中文GB2312", "繁體中文BIG5", "UTF-8" };

    public bool ToTW { get; set; }

    public bool DoubleEOL { get; set; }

  }
}
