using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MagicGirlWeb.Models;

// SmartEdit
using System.IO;
using System.Text;
//using OpenCC;

namespace MagicGirlWeb.Controllers
{
  [AllowAnonymous]
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger )
    {
      _logger = logger;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Privacy()
    {
      return View();
    }
    public IActionResult News()
    {
      return View();
    }

    [HttpGet]
    public IActionResult SmartEdit()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> SmartEdit(SmartEditModel model)
    {
      // -- 資料驗證 -- //
      if (!ModelState.IsValid)
      {
        return Error();

      }

      // -- 資料處理 -- //
      string fileName = model.TxtFile.FileName;
      // 微軟的encoding list, 因big5不在.Net Core內建支援的編碼內，需另外下載System.Text.Encoding.CodePages，並在使用前註冊
      // https://docs.microsoft.com/zh-tw/windows/win32/intl/code-page-identifiers?redirectedfrom=MSDN
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      Encoding encode = Encoding.GetEncoding(model.Encoding);

      try
      {
        Stream stream = model.TxtFile.OpenReadStream();
        // Create an instance of StreamReader to read from a file.
        // The using statement also closes the StreamReader.
        using (StreamReader sr = new StreamReader("TestFile.txt", encode))
        {
          string text = sr.ReadToEnd();
        }

        /*
        if (model.ToTW)
        {
          text = OpenCC.ConvertToTW(text);
          fileName = OpenCC.ConvertToTW(fileName);
        }

        if (DoubleEOL.Checked)
        {
          text = text.Replace("    \r\n", @"<br />");
          text = text.Replace("\r\n", @"<br />");
          text = text.Replace("\n", @"<br />");
          text = text.Replace("\r", @"<br />");
          text = text.Replace(@"<br />", Environment.NewLine + Environment.NewLine);
        }
        */

        // 儲存檔案

        // -- 寄信 -- //

      }
      catch (Exception ex)
      {
        //log.Error(ex.ToString());
      }

      return Ok();

    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
