using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
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
  // [Authorize(Policy = "PowerUser")]
  [AllowAnonymous]
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
      _fileDir = config["WebSaveFolder"];
      if (!Directory.Exists(_fileDir))
      {
        Directory.CreateDirectory(_fileDir);
      }
      _folderId = config["Authentication:DriveFolderId"];
    }

    // GET: Books
    public async Task<IActionResult> Index()
    {
      var magicContext = _context.Book.Include(b => b.Author);
      return View(await magicContext.ToListAsync());
    }

    // GET: Books/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var book = await _context.Book
          .Include(b => b.Author)
          .FirstOrDefaultAsync(m => m.Id == id);
      if (book == null)
      {
        return NotFound();
      }

      return View(book);
    }

    // GET: Books/Create
    public IActionResult Create()
    {
      ViewData["AuthorId"] = new SelectList(_context.Author, "Id", "Id");
      return View();
    }

    // POST: Books/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("AuthorId,TotalPage,Type,Status,Id,Name,CreateDate,ModifyDate,IsDelete")] Book book)
    {
      if (ModelState.IsValid)
      {
        _context.Add(book);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      ViewData["AuthorId"] = new SelectList(_context.Author, "Id", "Id", book.AuthorId);
      return View(book);
    }

    // GET: Books/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var book = await _context.Book.FindAsync(id);
      if (book == null)
      {
        return NotFound();
      }
      ViewData["AuthorId"] = new SelectList(_context.Author, "Id", "Id", book.AuthorId);
      return View(book);
    }

    // POST: Books/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("AuthorId,TotalPage,Type,Status,Id,Name,CreateDate,ModifyDate,IsDelete")] Book book)
    {
      if (id != book.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          _context.Update(book);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!BookExists(book.Id))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        return RedirectToAction(nameof(Index));
      }
      ViewData["AuthorId"] = new SelectList(_context.Author, "Id", "Id", book.AuthorId);
      return View(book);
    }

    // GET: Books/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var book = await _context.Book
          .Include(b => b.Author)
          .FirstOrDefaultAsync(m => m.Id == id);

      if (book == null)
        return NotFound();

      return View(book);
    }

    // POST: Books/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var book = await _context.Book.FindAsync(id);
      _context.Book.Remove(book);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    private bool BookExists(int id)
    {
      return _context.Book.Any(e => e.Id == id);
    }

    [HttpGet]
    public async Task<IActionResult> Fetch(FetchView fetchView)
    {
      var accountId = _userManager.GetUserId(User);

      if (fetchView.AccountEmails == null)
      {
        fetchView.AccountEmails = new List<FetchView.AccountEmailView>();

        var emails = _accountService.GetEmailAll();
        emails.OrderBy(e => e.AccountId);
        foreach (var email in emails)
        {
          var checkedEmail = new FetchView.AccountEmailView();
          checkedEmail.Email = email.Email;
          checkedEmail.Description = email.Description;
          if (email.AccountId == accountId)
            checkedEmail.Checked = true;

          fetchView.AccountEmails.Add(checkedEmail);
        }
      }

      if (fetchView.SupportUrls ==null)
        fetchView.SupportUrls = _crawlService.PluginInfos.Select(o => o.SupportUrl).ToList();

      return View(fetchView);
    }

    [HttpPost]
    public ActionResult FetchAnalysis(FetchView fetchView)
    {
      if (ModelState.IsValid)
      {
        Book book = _crawlService.Analysis(fetchView.Url);
        fetchView.Title = book.Name;
        fetchView.PageFrom = 1;
        fetchView.PageTo = book.TotalPage;
      }

      return RedirectToAction(nameof(Fetch), fetchView);
    }

    [HttpPost]
    public ActionResult FetchPost(FetchView fetchView)
    {
      if (!ModelState.IsValid)
        return null;

      string fileName = String.Format("{0}.txt", fetchView.Title);
      string filePath = Path.Combine(_fileDir, fileName);
      Book currBook = _crawlService.Analysis(fetchView.Url);
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

        if (bw.LastPageFrom != fetchView.PageFrom || bw.LastPageTo != fetchView.PageTo)
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
            downloadBook = CrawlBook(fetchView, filePath);

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
            downloadBook = CrawlBook(fetchView, filePath);

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
              downloadBook = CrawlBook(fetchView, filePath);

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

      if (fetchView.isDownload)
      {
        // 情境：下載檔案    
        _bookService.InsertBookDownload(
          fetchView.Url,
          accountId,
          Constant.DOWNLOAD_STATUS_SUCCESS,
          fetchView.PageFrom,
          fetchView.PageTo);

        byte[] bytes = System.IO.File.ReadAllBytes(filePath);
        return File(bytes, "application/octet-stream", fileName);
      }

      // 寄信, 寫入log，return回view
      string subject = String.Format(_config["MailSetting:Fetch:Subject"], Path.GetFileNameWithoutExtension(filePath));
      string body = _config["MailSetting:Fetch:Body"];
      List<string> mails = new List<string>();
      if (fetchView.AccountEmails != null)
        foreach (var accountMail in fetchView.AccountEmails)
          mails.Add(accountMail.Email);

      if (fetchView.CustomEmail != null)
        mails.Add(fetchView.CustomEmail);

      _notificationService.SendMail(
        mails,
        subject,
        body,
        filePath);

      _bookService.InsertBookDownload(
        fetchView.Url,
        accountId,
        Constant.DOWNLOAD_STATUS_SUCCESS,
        fetchView.PageFrom,
        fetchView.PageTo,
        mails);

      return RedirectToAction(nameof(Fetch), fetchView);
    }

    private Book CrawlBook(FetchView fetchView, string filePath)
    {
      Book book = _crawlService.Download(
        fetchView.Url,
        fetchView.PageFrom,
        fetchView.PageTo,
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
    public async Task<IActionResult> BookDownload(string sortOrder)
    {
      ViewData["IdSortParm"] = String.IsNullOrEmpty(sortOrder) ? "id_asc" : "";
      ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
      ViewData["PageSortParm"] = sortOrder == "page" ? "page_desc" : "page";

      var bookDownloads = _bookService.GetBookDownloadAll();
      IEnumerable<BookDownloadView> viewModels = null;
      viewModels = (from bd in bookDownloads
                    select new BookDownloadView()
                    {
                      Id = bd.Id,
                      Title = bd.BookWebsite.Book.Name,
                      TotalPage = bd.BookWebsite.Book.TotalPage,
                      DownloadDate = bd.CreateDate,
                      SourceId = bd.BookWebsite.SourceId
                    }).AsEnumerable();

      switch (sortOrder)
      {
        case "id_asc":
          viewModels = viewModels.OrderBy(bd => bd.Id);
          break;
        case "date":
          viewModels = viewModels.OrderBy(bd => bd.DownloadDate);
          break;
        case "page_desc":
          viewModels = viewModels.OrderByDescending(bd => bd.TotalPage);
          break;
        case "page":
          viewModels = viewModels.OrderBy(bd => bd.TotalPage);
          break;
        case "date_desc":
          viewModels = viewModels.OrderByDescending(bd => bd.DownloadDate);
          break;
        default:
          viewModels = viewModels.OrderByDescending(bd => bd.Id);
          break;
      }

      return View(viewModels);
    }

    // GET: Books/BookDownloadData
    // public async Task<IActionResult> BookDownloadData(int pageSize, int pageIndex, string userName, string userId)
    // {

    // }

    // GET: Books/CloudFile/xxxxxxx
    public async Task<IActionResult> CloudFile(string? sourceId)
    {
      if (sourceId == null)
        return NotFound();

      Book book = _bookService.GetBookBySourceId(sourceId);
      if (book == null)
        return NotFound();

      string fileName = String.Format("{0}.txt", book.Name);
      string filePath = Path.Combine(_fileDir, fileName);
      string fileId = book.BookWebsites.FirstOrDefault().FileId;
      bool isSuccess = _fileService.Download(
              fileId,
              filePath);

      if (!isSuccess)
        return NotFound();

      byte[] bytes = System.IO.File.ReadAllBytes(filePath);
      return File(bytes, "application/octet-stream", fileName);
    }


    [HttpGet]
    public IActionResult SmartEdit()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> SmartEdit(SmartEditView model)
    {
      // -- 資料驗證 -- //
      if (!ModelState.IsValid)
        // return Error();
        return View();

      // -- 資料處理 -- //
      string fileName = model.TxtFile.FileName;
      string filePath = Path.Combine(_fileDir, fileName);
      // 微軟的encoding list, 因big5不在.Net Core內建支援的編碼內，需另外下載System.Text.Encoding.CodePages，並在使用前註冊
      // https://docs.microsoft.com/zh-tw/windows/win32/intl/code-page-identifiers?redirectedfrom=MSDN
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      Encoding encode = Encoding.GetEncoding(model.Encoding);

      try
      {
        if (model.TxtFile.Length == 0)
          // return Error();
          return View();
        
        // 使用OpenReadStream的方式直接得到上傳檔案的Stream
        StreamReader streamReader = new StreamReader(model.TxtFile.OpenReadStream(), encode);
          string text = streamReader.ReadToEnd();        

        if (model.ToTW)
        {
          text = _crawlService.Format(text, FormatType.Traditional);
          fileName = _crawlService.Format(fileName, FormatType.Traditional);
        }

        if (model.DoubleEOL)
          text = _crawlService.Format(text, FormatType.DoubleEOL);

        // 儲存檔案
        await System.IO.File.WriteAllTextAsync(filePath, text);

        // -- 寄信 -- //
        string subject = String.Format(_config["MailSetting:SmartEdit:Subject"], Path.GetFileNameWithoutExtension(filePath));
        string body = _config["MailSetting:SmartEdit:Body"];
        List<string> mails = new List<string>();
        var user = await _userManager.GetUserAsync(User); 
        var mail = await _userManager.GetEmailAsync(user);
        mails.Add(mail);

        _notificationService.SendMail(
          mails,
          subject,
          body,
          filePath);

      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }

      return View();

    }

  

  }
}
