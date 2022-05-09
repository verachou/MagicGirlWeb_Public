using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MagicGirlWeb.Data;
using MagicGirlWeb.Models;
using MagicGirlWeb.Models.BooksViewModels;
using MagicGirlWeb.Service;

namespace MagicGirlWeb
{
  [Authorize]
  public class BooksController : Controller
  {
    private readonly ILogger<BooksController> _logger;
    private readonly IConfiguration _config;
    private readonly MagicContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IAccountService _accountService;
    private readonly IBookService _bookService;
    private readonly ICrawlService _crawlService;
    private readonly IFileService _fileService;
    private readonly NotificationService _notificationService;
    private readonly string _fileDir;
    private readonly string _fileNameFormat;
    private readonly string _folderId;

    public BooksController(
        ILogger<BooksController> logger,
        ILoggerFactory loggerFactory,
        IConfiguration config,
        MagicContext context,
        UserManager<IdentityUser> userManager)
    {
      _logger = logger;
      _config = config;
      _context = context;
      _userManager = userManager;
      _accountService = new AccountService(loggerFactory, context);
      _bookService = new BookService(loggerFactory, context);
      _crawlService = new CrawlService(loggerFactory, config);
      _fileService = new FileService(loggerFactory, config);
      _notificationService = new NotificationService(loggerFactory, config);
      _fileDir = config["FileSetting:WebSaveFolder"];
      if (!Directory.Exists(_fileDir))
      {
        Directory.CreateDirectory(_fileDir);
      }
      _fileNameFormat = config["FileSetting:FileNameFormat"];
      _folderId = config["Authentication:DriveFolderId"];
    }

    private ICollection<AccountEmail> GetEmailList()
    {
      var isAuthorized = User.IsInRole("ADMIN") ||
                         User.IsInRole("ADVANCE_USER");
      var accountEmails = new List<FetchView.AccountEmail>();

      if (!isAuthorized)
      {
        var accountId = _userManager.GetUserId(User);
        return _accountService.GetEmailByAccountId(accountId);
      }

      return _accountService.GetEmailAll();
    }

    [HttpGet]
    public async Task<IActionResult> Fetch(FetchView viewModel)
    {
      if (viewModel.AccountEmails == null)
      {
        var accountId = _userManager.GetUserId(User);
        viewModel.AccountEmails = new List<FetchView.AccountEmail>();

        // var emails = _accountService.GetEmailAll();
        var emails = GetEmailList();
        if (emails != null)
        {
          emails.OrderBy(e => e.AccountId);
          foreach (var email in emails)
          {
            var checkedEmail = new FetchView.AccountEmail();
            checkedEmail.EmailId = email.Id;
            checkedEmail.Description = email.Description;
            if (email.AccountId == accountId)
              checkedEmail.IsChecked = true;

            viewModel.AccountEmails.Add(checkedEmail);
          }
        }
      }

      if (viewModel.SupportUrls == null)
        viewModel.SupportUrls = _crawlService.PluginInfos.Select(o => o.SupportUrl).ToList();

      return View(viewModel);
    }

    [HttpPost]
    public ActionResult FetchAnalysis(FetchView viewModel)
    {
      if (ModelState.IsValid)
      {
        Book book = _crawlService.Analysis(viewModel.Url);
        if (book == null)
        {
          viewModel.Title = "無法解析";
          viewModel.PageFrom = 1;
          viewModel.PageTo = 1;
        }
        else
        {
          viewModel.Title = book.Name;
          viewModel.PageFrom = 1;
          viewModel.PageTo = book.TotalPage;
        }
      }

      return RedirectToAction(nameof(Fetch), viewModel);
    }

    [HttpPost]
    public ActionResult FetchPost(FetchView viewModel)
    {
      if (!ModelState.IsValid)
        return null;

      var title = viewModel.Title;
      var pageFrom = viewModel.PageFrom;
      var pageTo = viewModel.PageTo;
      string fileName = String.Format(_fileNameFormat, title, pageFrom, pageTo);
      string filePath = Path.Combine(_fileDir, fileName);
      Book currBook = _crawlService.Analysis(viewModel.Url);
      Book saveBook = _bookService.GetBookBySourceId(currBook.BookWebsites.FirstOrDefault().SourceId);
      Book downloadBook;
      BookStatus status = BookStatus.BookNotExist;

      var accountId = _userManager.GetUserId(User);

      // 預設無status: 判斷當前狀態
      // 若已設定status則直接以該status執行，不用另外判斷

      if (saveBook != null)
      {
        status = BookStatus.BookNotChanged;
        BookWebsite bw = saveBook.BookWebsites.FirstOrDefault();

        if (bw.LastPageFrom != viewModel.PageFrom || bw.LastPageTo != viewModel.PageTo)
          status = BookStatus.BookChanged;
      }
      else
        status = BookStatus.BookNotExist;


      // BookNotChanged(book存在，起訖頁和fetchview相同)：直接從雲端下載，提供file
      // BookChanged(book存在，起訖頁和fetchview不同)：下載檔案，使用UpdateBook
      // BookNotExist：下載檔案，使用InsertBook     

      // _fileService.ClearLocalFolder(_fileDir);
      switch (status)
      {
        case BookStatus.BookNotExist:
          try
          {
            // 下載檔案
            downloadBook = CrawlBook(viewModel, filePath);

            if (downloadBook == null)
              return null;

            // 更新資料庫
            BookWebsite bw = downloadBook.BookWebsites.FirstOrDefault();
            _bookService.InsertBook(
              downloadBook.Name,
              downloadBook.Author.Name,
              downloadBook.TotalPage,
              bw.Url,
              bw.SourceId,
              bw.LastPageFrom,
              bw.LastPageTo,
              Constant.BOOK_TYPE_NOVEL,
              bw.TaskStatus);

            // 檔案上傳 & 更新FileId
            UploadCloud(filePath, Constant.MIME_TEXT_UTF8, bw.SourceId);

          }
          catch (Exception ex)
          {
            _logger.LogError(ex.ToString());
          }
          break;
        case BookStatus.BookChanged:
          try
          {
            // 下載檔案
            downloadBook = CrawlBook(viewModel, filePath);

            if (downloadBook == null)
              return null;

            // 更新資料庫
            BookWebsite bw = downloadBook.BookWebsites.FirstOrDefault();
            _bookService.UpdateBook(
              saveBook.BookWebsites.FirstOrDefault(),
              saveBook.TotalPage,
              bw.LastPageFrom,
              bw.LastPageTo,
              bw.TaskStatus);

            // 檔案上傳 & 更新FileId
            UploadCloud(filePath, Constant.MIME_TEXT_UTF8, bw.SourceId);
          }
          catch (Exception ex)
          {
            _logger.LogError(ex.ToString());
          }
          break;
        case BookStatus.BookNotChanged:
          try
          {
            BookWebsite bw = saveBook.BookWebsites.FirstOrDefault();
            bool isSuccess = _fileService.Download(
              bw.FileId,
              filePath);

            // 雲端找不到檔案，則重新下載
            if (!isSuccess)
            {
              // 下載檔案
              downloadBook = CrawlBook(viewModel, filePath);

              if (downloadBook == null)
                return null;

              // 更新資料庫
              bw = downloadBook.BookWebsites.FirstOrDefault();
              _bookService.UpdateBook(
                saveBook.BookWebsites.FirstOrDefault(),
                saveBook.TotalPage,
                bw.LastPageFrom,
                bw.LastPageTo,
                bw.TaskStatus);

              // 檔案上傳 & 更新FileId
              UploadCloud(filePath, Constant.MIME_TEXT_UTF8, bw.SourceId);
            }
          }
          catch (Exception ex)
          {
            _logger.LogError(ex.ToString());
          }
          break;
        default:
          _logger.LogError("BookStatus is out of definition. BookStatus: {0}", status);
          return null;
      }

      if (viewModel.IsDownload)
      {
        // 情境：下載檔案    
        _bookService.InsertBookDownload(
          viewModel.Url,
          accountId,
          Constant.DOWNLOAD_STATUS_SUCCESS,
          viewModel.PageFrom,
          viewModel.PageTo);

        byte[] bytes = System.IO.File.ReadAllBytes(filePath);
        return File(bytes, "application/octet-stream", fileName);
      }

      // 寄信, 寫入log，return回view
      string subject = String.Format(_config["MailSetting:Fetch:Subject"], Path.GetFileNameWithoutExtension(filePath));
      string body = _config["MailSetting:Fetch:Body"];
      List<string> mails = new List<string>();
      if (viewModel.AccountEmails != null)
        foreach (var accountMail in viewModel.AccountEmails)
          if (accountMail.IsChecked == true)
          {
            var mail = _accountService.GetEmailById(accountMail.EmailId);
            mails.Add(mail.Email);
          }

      if (viewModel.CustomEmail != null)
        mails.Add(viewModel.CustomEmail);

      _notificationService.SendMail(
        mails,
        subject,
        body,
        filePath);

      _bookService.InsertBookDownload(
        viewModel.Url,
        accountId,
        Constant.DOWNLOAD_STATUS_SUCCESS,
        viewModel.PageFrom,
        viewModel.PageTo,
        mails);

      return RedirectToAction(nameof(Fetch), viewModel);
    }

    private Book CrawlBook(FetchView viewModel, string filePath)
    {
      Book book = _crawlService.Download(
        viewModel.Url,
        viewModel.PageFrom,
        viewModel.PageTo,
        filePath);

      if (book == null)
        _logger.LogError(CustomMessage.ObjectIsNull, nameof(book));

      return book;
    }

    private void UploadCloud(string filePath, string mimeType, string sourceId)
    {
      // 檔案上傳
      string fileId = _fileService.Upload(
        filePath,
        mimeType,
        sourceId);

      if (!String.IsNullOrEmpty(fileId))
        _bookService.UpdateFileId(sourceId, fileId, _folderId);
    }

    public enum BookStatus
    {
      BookNotChanged,
      BookChanged,
      BookNotExist
    }

    // GET: Books/BookDownload
    [HttpGet]
    public async Task<IActionResult> BookDownload()
    {
      var bookDownloads = _bookService.GetBookDownloadAll();
      var viewModel = new BookDownloadView();
      viewModel.BookDownloads = (from bd in bookDownloads
                                 select new BookDownloadView.BookDownload()
                                 {
                                   Id = bd.Id,
                                   Title = bd.BookWebsite.Book.Name,
                                   TotalPage = bd.BookWebsite.Book.TotalPage,
                                   DownloadDate = bd.CreateDate,
                                   SourceId = bd.BookWebsite.SourceId
                                 }).ToList();

      if (viewModel.AccountEmails == null)
      {
        var accountId = _userManager.GetUserId(User);
        viewModel.AccountEmails = new List<BookDownloadView.AccountEmail>();

        // var emails = _accountService.GetEmailAll();
        var emails = GetEmailList();
        if (emails != null)
        {
          emails.OrderBy(e => e.AccountId);
          foreach (var email in emails)
          {
            var checkedEmail = new BookDownloadView.AccountEmail();
            checkedEmail.EmailId = email.Id;
            checkedEmail.Description = email.Description;
            if (email.AccountId == accountId)
              checkedEmail.IsChecked = true;

            viewModel.AccountEmails.Add(checkedEmail);
          }
        }
      }
      return View(viewModel);
    }

    // Post: Books/BookDownloadData
    [HttpPost]
    public async Task<IActionResult> BookDownloadPost(BookDownloadView viewModel, string? id)
    {
      var sourceId = id;
      if (sourceId == null)
      {
        TempData["message"] = "此項目已遺失，請使用線上小說轉檔功能重新下載。";
        return RedirectToAction(nameof(BookDownload));
      }

      Book book = _bookService.GetBookBySourceId(sourceId);
      if (book == null)
      {
        _logger.LogError(CustomMessage.ObjectIsNull, sourceId);
        TempData["message"] = "此項目已遺失，請使用線上小說轉檔功能重新下載。";
        return RedirectToAction(nameof(BookDownload));
      }

      var title = book.Name;
      var pageFrom = book.BookWebsites.FirstOrDefault().LastPageFrom;
      var pageTo = book.BookWebsites.FirstOrDefault().LastPageTo;
      string fileName = String.Format(_fileNameFormat, title, pageFrom, pageTo);
      string filePath = Path.Combine(_fileDir, fileName);
      string fileId = book.BookWebsites.FirstOrDefault().FileId;
      bool isSuccess = _fileService.Download(
              fileId,
              filePath);

      if (!isSuccess)
      {
        _logger.LogError(CustomMessage.ObjectIsNull, sourceId);
        TempData["message"] = "此項目已遺失，請使用線上小說轉檔功能重新下載。";
        return RedirectToAction(nameof(BookDownload));
      }

      var accountId = _userManager.GetUserId(User);
      byte[] bytes = System.IO.File.ReadAllBytes(filePath);
      // 寄信, 寫入log，return回view
      string subject = String.Format(_config["MailSetting:BookDownload:Subject"], Path.GetFileNameWithoutExtension(filePath));
      string body = _config["MailSetting:BookDownload:Body"];
      List<string> mails = new List<string>();
      if (viewModel.AccountEmails != null)
        foreach (var accountMail in viewModel.AccountEmails)
          if (accountMail.IsChecked == true)
          {
            var mail = _accountService.GetEmailById(accountMail.EmailId);
            mails.Add(mail.Email);
          }

      _notificationService.SendMail(
        mails,
        subject,
        body,
        filePath);

      _bookService.InsertBookDownload(
        book.BookWebsites.FirstOrDefault().Url,
        accountId,
        Constant.DOWNLOAD_STATUS_SUCCESS,
        book.BookWebsites.FirstOrDefault().LastPageFrom,
        book.BookWebsites.FirstOrDefault().LastPageTo,
        mails);

      return RedirectToAction(nameof(BookDownload), viewModel);

    }

    // GET: Books/CloudFile/id
    public async Task<IActionResult> CloudFile(string? id)
    {
      var sourceId = id;
      if (sourceId == null)
      {
        TempData["message"] = "此項目已遺失，請使用線上小說轉檔功能重新下載。";
        return RedirectToAction(nameof(BookDownload));
      }

      Book book = _bookService.GetBookBySourceId(sourceId);
      if (book == null)
      {
        _logger.LogError(CustomMessage.ObjectIsNull, sourceId);
        TempData["message"] = "此項目已遺失，請使用線上小說轉檔功能重新下載。";
        return RedirectToAction(nameof(BookDownload));
      }

      var title = book.Name;
      var pageFrom = book.BookWebsites.FirstOrDefault().LastPageFrom;
      var pageTo = book.BookWebsites.FirstOrDefault().LastPageTo;
      string fileName = String.Format(_fileNameFormat, title, pageFrom, pageTo);
      string filePath = Path.Combine(_fileDir, fileName);
      string fileId = book.BookWebsites.FirstOrDefault().FileId;
      bool isSuccess = _fileService.Download(
              fileId,
              filePath);

      if (!isSuccess)
      {
        _logger.LogError(CustomMessage.ObjectIsNull, sourceId);
        TempData["message"] = "此項目已遺失，請使用線上小說轉檔功能重新下載。";
        return RedirectToAction(nameof(BookDownload));
      }

      byte[] bytes = System.IO.File.ReadAllBytes(filePath);
      return File(bytes, "application/octet-stream", fileName);
    }


    [HttpGet]
    public IActionResult SmartEdit(SmartEditView viewModel)
    {
      if (viewModel.AccountEmails == null)
      {
        var accountId = _userManager.GetUserId(User);
        viewModel.AccountEmails = new List<SmartEditView.AccountEmail>();

        // var emails = _accountService.GetEmailAll();
        var emails = GetEmailList();
        if (emails != null)
        {
          emails.OrderBy(e => e.AccountId);
          foreach (var email in emails)
          {
            var checkedEmail = new SmartEditView.AccountEmail();
            checkedEmail.EmailId = email.Id;
            checkedEmail.Description = email.Description;
            if (email.AccountId == accountId)
              checkedEmail.IsChecked = true;

            viewModel.AccountEmails.Add(checkedEmail);
          }
        }
      }
      return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> SmartEditPost(SmartEditView viewModel)
    {
      // -- 資料驗證 -- //
      if (!ModelState.IsValid)
        // return Error();
        return RedirectToAction(nameof(SmartEdit), viewModel);


      // -- 資料處理 -- //    
      var TxtFile = JsonSerializer.Deserialize<SmartEditView.FilepondFile>(viewModel.JsonFile);
      if (TxtFile.data.Length == 0)
        // return Error();
        return RedirectToAction(nameof(SmartEdit), viewModel);

      // 微軟的encoding list, 因big5不在.Net Core內建支援的編碼內，需另外下載System.Text.Encoding.CodePages，並在使用前註冊
      // https://docs.microsoft.com/zh-tw/windows/win32/intl/code-page-identifiers?redirectedfrom=MSDN
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      Encoding encode = Encoding.GetEncoding(viewModel.SelectedEncoding);
      string text = encode.GetString(Convert.FromBase64String(TxtFile.data));
      string fileName = TxtFile.name;

      try
      {
        if (viewModel.ToTW)
        {
          text = _crawlService.Format(text, FormatType.Traditional);
          fileName = _crawlService.Format(fileName, FormatType.Traditional);
        }

        if (viewModel.DoubleEOL)
          text = _crawlService.Format(text, FormatType.DoubleEOL);

        // 儲存檔案
        string filePath = Path.Combine(_fileDir, fileName);
        await System.IO.File.WriteAllTextAsync(filePath, text);


        if (viewModel.IsEmail)
        {
          // 寄信, 寫入log，return回view
          string subject = String.Format(_config["MailSetting:SmartEdit:Subject"], Path.GetFileNameWithoutExtension(filePath));
          string body = _config["MailSetting:SmartEdit:Body"];
          List<string> mails = new List<string>();
          if (viewModel.AccountEmails != null)
            foreach (var accountMail in viewModel.AccountEmails)
              if (accountMail.IsChecked == true)
              {
                var mail = _accountService.GetEmailById(accountMail.EmailId);
                mails.Add(mail.Email);
              }

          _notificationService.SendMail(
            mails,
            subject,
            body,
            filePath);
        }

        if (viewModel.IsDownload)
        {
          // 情境：下載檔案    
          byte[] bytes = System.IO.File.ReadAllBytes(filePath);
          return File(bytes, "application/octet-stream", fileName);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }

      return RedirectToAction(nameof(SmartEdit), viewModel);

    }

  }
}
