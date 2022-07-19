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
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MagicGirlWeb.Data;
using MagicGirlWeb.Models;
using MagicGirlWeb.Models.BooksViewModels;
using MagicGirlWeb.Service;
using MagicGirlWeb.Hubs;

namespace MagicGirlWeb
{
  [Authorize]
  public class BooksController : Controller
  {
    private readonly ILogger<BooksController> _logger;
    private readonly IConfiguration _config;
    private readonly IHubContext<ProgressHub> _progressHubContext;
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
        IHubContext<ProgressHub> hubContext,
        MagicContext context,
        UserManager<IdentityUser> userManager)
    {
      _logger = logger;
      _config = config;
      _progressHubContext = hubContext;
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

    /// <summary>
    /// 根據使用者的角色取得他可寄出的Email
    /// </summary>
    /// <returns>Email list</returns>
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
      _logger.LogTrace("[HttpGet] Fetch");
      if (viewModel == null)
        viewModel = new FetchView();


      var accountId = _userManager.GetUserId(User);
      viewModel.AccountEmails = new List<FetchView.AccountEmail>();

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

      viewModel.SupportUrls = _crawlService.PluginInfos.Select(o => o.SupportUrl).ToList();

      return View(viewModel);
    }


    [HttpPost]
    public async Task<IActionResult> FetchPost(FetchView viewModel)
    {
      switch(viewModel.FormAction)
      {
        // Analysis
        case "A":
          viewModel = FetchAnalysis(viewModel);
          break;

        // Download
        case "D":
          var file = FetchDownload(viewModel);
          return file;
          break;
        // Send
        case "S":
          FetchDownload(viewModel);
          break;
        default:
          break;
      }
      // return View(viewModel);
      return RedirectToAction(nameof(Fetch), viewModel);
    }

    /// <summary>
    /// 根據使用者輸入的網址取得小說相關資訊
    /// </summary>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    private FetchView FetchAnalysis(FetchView viewModel)
    {
      _logger.LogTrace("[HttpPost] FetchAnalysis");
      viewModel.ProgressPct = 0;
      viewModel.Title = "無法解析";
      viewModel.PageFrom = 1;
      viewModel.PageTo = 1;

      if (!ModelState.IsValid)
        _logger.LogWarning(CustomMessage.ModelIsInvalid);
      else         
      {        
        Book book = _crawlService.Analysis(viewModel.Url);
        if (book != null)          
        {
          viewModel.Title = book.Name;
          viewModel.PageFrom = 1;
          viewModel.PageTo = book.TotalPage;
          _logger.LogInformation("Url: {0} analysis success.", viewModel.Url);
        }
        else
          _logger.LogInformation("Url: {0} analysis fail.", viewModel.Url);
      }      

      return viewModel;
    }

    /// <summary>
    /// 根據Url下載小說，並進行本地下載或發送至指定信箱，本地下載與寄信共用此方法。
    /// </summary>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    private ActionResult FetchDownload(FetchView viewModel)
    {
      _logger.LogTrace("[HttpPost] FetchDownload");

      if (!ModelState.IsValid)
      {
        _logger.LogWarning(CustomMessage.ModelIsInvalid);
        // TempData["message"] = "資料錯誤，請重新輸入。";
        return RedirectToAction(nameof(Fetch), viewModel);
      }

      viewModel.ProgressPct = 0;
      var isDownload = viewModel.FormAction=="D";
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

      _logger.LogInformation("User: {0} download. Url: {1}", User.Identity.Name, viewModel.Url);
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
            downloadBook = CrawlBook(viewModel.Url,
              viewModel.PageFrom,
              viewModel.PageTo,
              filePath,
              viewModel.HubConnId);

            if (downloadBook == null)
            {
              _logger.LogError(CustomMessage.ObjectIsNull, downloadBook);
              TempData["message"] = "下載失敗，請稍後再試。";
              //return viewModel;
            }

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

            // 更新下載進度
            viewModel.ProgressPct = (int)(100 * (bw.LastPageTo - bw.LastPageFrom) / (viewModel.PageTo - viewModel.PageFrom));
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
            downloadBook = CrawlBook(viewModel.Url,
              viewModel.PageFrom,
              viewModel.PageTo,
              filePath,
              viewModel.HubConnId);

            if (downloadBook == null)
            {
              _logger.LogError(CustomMessage.ObjectIsNull, downloadBook);
              TempData["message"] = "下載失敗，請稍後再試。";
              //return viewModel;
            }

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

            // 更新下載進度
            viewModel.ProgressPct = (int)(100 * (bw.LastPageTo - bw.LastPageFrom) / (viewModel.PageTo - viewModel.PageFrom));
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
              downloadBook = CrawlBook(viewModel.Url,
                viewModel.PageFrom,
                viewModel.PageTo,
                filePath,
                viewModel.HubConnId);

              if (downloadBook == null)
              {
                _logger.LogError(CustomMessage.ObjectIsNull, downloadBook);
                TempData["message"] = "下載失敗，請稍後再試。";
                return null;
              }

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

              // 更新下載進度
              viewModel.ProgressPct = (int)(100 * (bw.LastPageTo - bw.LastPageFrom) / (viewModel.PageTo - viewModel.PageFrom));
            }
          }
          catch (Exception ex)
          {
            _logger.LogError(ex.ToString());
          }
          break;
        default:
          _logger.LogError("BookStatus is out of definition. BookStatus: {0}", status);
          TempData["message"] = "下載失敗，請稍後再試。";
          //return viewModel;
          break;
      }

      if (isDownload)
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

      return null;
    }



    /// <summary>
    /// 下載小說至指定的路徑
    /// </summary>
    /// <param name="url">小說網址</param>
    /// <param name="pageFrom">下載起始頁數</param>
    /// <param name="pageTo">下載終止頁數</param>
    /// <param name="filePath">下載檔案的路徑</param>
    /// <param name="hubConnId">SignalR hub connection id，將下載進度(%)透過Hub提供給指定對象</param>
    /// <returns></returns>
    private Book CrawlBook(
      string url,
      int pageFrom,
      int pageTo,
      string filePath,
      string? hubConnId)
    {
      Book book;

      if (hubConnId != null)
      {
        var progress = new Progress<int>(percent =>
        {
          // percent有傳出來，但沒有成功send到前端
          _progressHubContext.Clients.Client(hubConnId).SendAsync("UpdProgress", percent);
        });

        book = _crawlService.Download(
          url,
          pageFrom,
          pageTo,
          filePath,
          progress);
      }
      else
        book = _crawlService.Download(
          url,
          pageFrom,
          pageTo,
          filePath,
          null);

      if (book == null)
        _logger.LogError(CustomMessage.ObjectIsNull, nameof(book));

      return book;
    }

    /// <summary>
    /// 上傳檔案至雲端儲存空間
    /// </summary>
    /// <param name="filePath">上傳檔案的儲存位置</param>
    /// <param name="mimeType">檔案格式</param>
    /// <param name="sourceId">BookWebsite的SourceId，已供識別下載來源</param>
    private void UploadCloud(string filePath, string mimeType, string sourceId)
    {
      // 檔案上傳
      List<string> cloudFolderIds = new List<string>();
      cloudFolderIds.Add(_config.GetValue<string>("Authentication:Google:DriveFolderId"));      
      string fileId = _fileService.Upload(
        filePath,
        mimeType,
        sourceId,
        cloudFolderIds);

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
      _logger.LogTrace("[HttpGet] BookDownload");

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

    /// <summary>
    /// 於雲端儲存空間下載小說，並進行本地下載或發送至指定信箱
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="id">SourceId</param>
    /// <returns></returns>
    // Post: Books/BookDownloadData
    [HttpPost]
    public async Task<IActionResult> BookDownloadPost(BookDownloadView viewModel, string? id)
    {
      _logger.LogTrace("[HttpPost] BookDownloadPost");
      _logger.LogInformation("User: {0} download from BookDownload. SourceId: {1}", User.Identity.Name, id);
      var sourceId = id;
      if (sourceId == null)
      {
        _logger.LogError(CustomMessage.ObjectIsNull, "sourceId");
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

    /// <summary>
    /// 根據SourceId於雲端儲存空間下載小說，並進行本地下載
    /// </summary>
    /// <param name="id">SourceId</param>
    /// <returns></returns>
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
      _logger.LogTrace("[HttpGet] SmartEdit");

      if (viewModel.AccountEmails == null)
      {
        var accountId = _userManager.GetUserId(User);
        viewModel.AccountEmails = new List<SmartEditView.AccountEmail>();

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

    /// <summary>
    /// 將使用者上傳的檔案做簡繁轉換與換行修正，完成後進行本地下載或發送至指定信箱
    /// </summary>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> SmartEditPost(SmartEditView viewModel)
    {
      _logger.LogTrace("[HttpPost] SmartEditPost");
      _logger.LogInformation("User: {0} upload file to SmartEdit.", User.Identity.Name);
      // -- 資料驗證 -- //
      if (!ModelState.IsValid)
      {
        _logger.LogWarning(CustomMessage.ModelIsInvalid);
        TempData["message"] = "資料錯誤，請重新輸入。";
        return RedirectToAction(nameof(SmartEdit), viewModel);
      }

      // -- 資料處理 -- //    
      var TxtFile = JsonSerializer.Deserialize<SmartEditView.FilepondFile>(viewModel.JsonFile);
      if (TxtFile.data.Length == 0)
      {
        _logger.LogWarning(CustomMessage.ModelIsInvalid);
        TempData["message"] = "檔案格式錯誤，請重新上傳。";
        return RedirectToAction(nameof(SmartEdit), viewModel);
      }

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

          TempData["message"] = "檔案已寄出。";
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

    [Authorize(Policy = "RequireAdminOrAdvanceUser")]
    // [HttpGet] 
    public async Task<IActionResult> Depository(DepositoryView viewModel)
    {
      _logger.LogInformation("[HttpGet] Depository");

      if (viewModel.Depositorys == null)
      {
        // 從config取得可使用的Google資料夾設定
        viewModel.Depositorys = new List<SelectListItem>();
        var depoSettings = _config.GetSection("Authentication:Google:Depository").GetChildren();
        foreach(var setting in depoSettings)
        {
          var depo = new SelectListItem(setting["Name"], setting.Key);
          viewModel.Depositorys.Add(depo);
        }        
      }

      if (viewModel.SelectedDepoId == null)
        viewModel.SelectedDepoId = viewModel.Depositorys.FirstOrDefault().Value;

      _logger.LogInformation("User: {0} query for {1}. DepositoryId: {2}", User.Identity.Name, viewModel.SelectedDepoId, viewModel.SelectedDepoId);
            
      if(viewModel.Files == null)
      {
        var setting = _config.GetSection(String.Format("Authentication:Google:Depository:{0}", viewModel.SelectedDepoId));
        string cloudFolderId = setting["DriveFolderId"];
          
        List<IFileService.CloudFile> cloudFiles = _fileService.GetFileList(cloudFolderId);
        if(cloudFiles==null)
        {
          _logger.LogError(CustomMessage.ObjectIsNull, "cloudFiles");
          TempData["message"] = "查無資料，請確認資料夾權限與路徑設定是否正確。";
          return View(viewModel);
        }

        viewModel.Files = new List<DepositoryView.File>();
        foreach (var cf in cloudFiles)
        {
          var DepoFile = new DepositoryView.File();
          DepoFile.Id = cf.Id;
          DepoFile.Name = cf.Name;
          DepoFile.Size = cf.Size;     
          DepoFile.Description = cf.Description;
          viewModel.Files.Add(DepoFile);
        }
      }

      var accountId = _userManager.GetUserId(User);
      viewModel.AccountEmails = new List<DepositoryView.AccountEmail>();
      var emails = GetEmailList();
      if (emails != null)
      {
        emails.OrderBy(e => e.AccountId);
        foreach (var email in emails)
        {
          var checkedEmail = new DepositoryView.AccountEmail();
          checkedEmail.EmailId = email.Id;
          checkedEmail.Description = email.Description;
          if (email.AccountId == accountId)
            checkedEmail.IsChecked = true;

          viewModel.AccountEmails.Add(checkedEmail);
        }
      }            
      return View(viewModel);
    }

    /// <summary>
    /// 於雲端儲存空間下載小說，並進行本地下載或發送至指定信箱
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="id">fileId</param>
    /// <returns></returns>
    // Post: Books/DepositoryPost/xxxx?name=xx
    [Authorize(Policy = "RequireAdminOrAdvanceUser")]
    [HttpPost]
    public async Task<IActionResult> DepositoryPost(DepositoryView viewModel, string? id, string? name)
    {
      _logger.LogInformation("[HttpPost] DepositoryPost");
      _logger.LogInformation("User: {0} download from BookDownload. FileId: {1} FileName: {2}", User.Identity.Name, id, name);

      if (id == null || name == null)
      {
        TempData["message"] = "發生錯誤，請重新整理後再嘗試。";
        return RedirectToAction(nameof(Depository));
      }

      string fileName = name;
      string filePath = Path.Combine(_fileDir, fileName);
      string fileId = id;
      bool isSuccess = _fileService.Download(
              fileId,
              filePath);

      if (!isSuccess)
      {
        _logger.LogError(CustomMessage.ObjectIsNull, fileId);
        TempData["message"] = "發生錯誤，請重新整理後再嘗試。";
        return RedirectToAction(nameof(Depository));
      }

      var accountId = _userManager.GetUserId(User);
      byte[] bytes = System.IO.File.ReadAllBytes(filePath);
      // 寄信, 寫入log，return回view
      string subject = String.Format(_config["MailSetting:BookDownload:Subject"], Path.GetFileNameWithoutExtension(filePath));
      string body = _config["MailSetting:Depository:Body"];
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

      return RedirectToAction(nameof(Depository), viewModel);

    }

    /// <summary>
    /// 根據Google的FileId於雲端儲存空間下載小說，並進行本地下載
    /// </summary>
    /// <param name="id">FileId</param>
    /// <returns></returns>
    // GET: Books/CloudDepository/xxxxxxx?name=xx
    [Authorize(Policy = "RequireAdminOrAdvanceUser")]
    public async Task<IActionResult> CloudDepository(string? id, string? name)
    {
      _logger.LogInformation("[HttpPost] CloudDepository");
      _logger.LogInformation("User: {0} download from BookDownload. FileId: {1} FileName: {2}", User.Identity.Name, id, name);
      
      if (id == null || name == null)
      {
        TempData["message"] = "發生錯誤，請重新整理後再嘗試。";
        return RedirectToAction(nameof(Depository));
      }

      string fileName = name;
      string filePath = Path.Combine(_fileDir, fileName);
      string fileId = id;
      bool isSuccess = _fileService.Download(
              fileId,
              filePath);

      if (!isSuccess)
      {
        _logger.LogError(CustomMessage.ObjectIsNull, fileId);
        TempData["message"] = "發生錯誤，請重新整理後再嘗試。";
        return RedirectToAction(nameof(Depository));
      }

      byte[] bytes = System.IO.File.ReadAllBytes(filePath);
      return File(bytes, "application/octet-stream", fileName);
    }
  }
}
