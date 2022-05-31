using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using CSNovelCrawler.Core;
using CSNovelCrawler.Interface;
using CSNovelCrawler.Class;
using MagicGirlWeb.Service;
using MagicGirlWeb.Models;

namespace MagicGirlWeb.Service
{
  public class CrawlService : ICrawlService
  {
    public ICollection<ICrawlService.PluginInfo> PluginInfos { get; set; }
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly CoreManager _coreManager;
    private List<TaskInfo> _taskList;
    public CrawlService(ILoggerFactory loggerFactory, IConfiguration config)
    {
      _loggerFactory = loggerFactory;
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
      _config = config;
      _coreManager = new CoreManager(_loggerFactory, _config);
      _taskList = new List<TaskInfo>();

      // 紀錄CSNovelCrawler支援的插件內容
      PluginInfos = new List<ICrawlService.PluginInfo>();
      foreach (var plugin in _coreManager.PluginManager.Plugins)
      {
        var attribute = (PluginInformationAttribute)Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginInformationAttribute));
        var pluginInfo = new PluginInfo(attribute.Name, attribute.FriendlyName, attribute.SupportUrl);
        PluginInfos.Add(pluginInfo);
      }
    }

    /// <summary>
    /// 根據Url解析書名、作者等相關資訊
    /// </summary>
    /// <param name="url">小說網址</param>
    /// <returns></returns>
    public Book Analysis(string url)
    {
      IPlugin plugin = _coreManager.PluginManager.GetPlugin(url);
      TaskInfo taskInfo = _coreManager.TaskManager.AddTask(plugin, url);
      _coreManager.TaskManager.AnalysisTask(taskInfo);
      if (taskInfo.Status != DownloadStatus.AnalysisComplete)
      {
        return null;
      }
      Author author = new Author(taskInfo.Author);
      Book book = new Book(taskInfo.Title, author.Id, taskInfo.TotalSection);
      book.Author = author;
      book.BookWebsites = new List<BookWebsite>();
      BookWebsite bookWebsite = new BookWebsite(
            taskInfo.Url,
            book.Id,
            taskInfo.BasePlugin.GetHash(url), // SourceId
            taskInfo.BeginSection,            // LastPageFrom
            taskInfo.EndSection               // LastPageTo
        );
      book.BookWebsites.Add(bookWebsite);
      _taskList.Add(taskInfo);

      return book;
    }

    /// <summary>
    /// 將網站上的內容下載後儲存於指定位置
    /// </summary>
    /// <param name="url">小說網址</param>
    /// <param name="lastPageFrom">下載起始頁</param>
    /// <param name="lastPageTo">下載結束頁</param>
    /// <param name="filePath">下載檔案儲存位置</param>
    /// <param name="progress">IProgress，將當前下載進度同步給呼叫者</param>
    /// <returns></returns>
    public Book Download(
      string url,
      int lastPageFrom,
      int lastPageTo,
      string filePath,
      IProgress<int>? progress)
    {
      Book book = Analysis(url);
      TaskInfo taskInfo = _taskList.Find(x => x.Url == url);

      taskInfo.BeginSection = lastPageFrom;
      taskInfo.EndSection = lastPageTo;

      //Task downloadTask = _coreManager.TaskManager.DownloadTaskAsync(taskInfo);
      Task downloadTask = Task.Run(() => _coreManager.TaskManager.DownloadTaskAsync(taskInfo));

      CancellationTokenSource cts = new CancellationTokenSource();
      CancellationToken token = cts.Token;
      Task.Run(() => CheckDownloadProgress(token, taskInfo, progress), token);

      downloadTask.Wait();
      cts.Cancel();

      book.BookWebsites.FirstOrDefault().LastPageFrom = lastPageFrom;
      book.BookWebsites.FirstOrDefault().LastPageTo = lastPageTo;
      book.BookWebsites.FirstOrDefault().TaskStatus = Common.DownloadStatusAdapter(taskInfo.Status);

      if (taskInfo.Status == DownloadStatus.DownloadComplete)
      {
        try
        {
          File.Copy(taskInfo.SaveFullPath, filePath, true);         // true: 目標路徑存在檔案會直接覆蓋掉 
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }
      }

      return book;
    }

    /// <summary>
    /// 檢查taskInfo當前下載進度
    /// </summary>
    /// <param name="token"></param>
    /// <param name="taskInfo"></param>
    /// <param name="progress"></param>
    private void CheckDownloadProgress(CancellationToken token, TaskInfo taskInfo, IProgress<int> progress)
    {
      while (true)
      {
        var rate = taskInfo.GetProgress();
        var percent = (int)(rate * 100);
        if (progress != null)
          progress.Report(percent);

        Thread.Sleep(200);

        // 正常結束此非同步工作，結束前更新最後狀態
        if (token.IsCancellationRequested)
        {
          rate = taskInfo.GetProgress();
          percent = (int)(rate * 100);
          if (progress != null)
            progress.Report(percent);
          break;
        }
      }
    }

    public void DeleteLocalFile(string url)
    {
      TaskInfo taskInfo = _taskList.Find(x => x.Url == url);

      try
      {
        File.Delete(taskInfo.SaveFullPath); // true: 移除 path 中的目錄、子目錄和檔案
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }

    }

    /// <summary>
    /// 將輸入字串依指定格式處理
    /// </summary>
    /// <param name="text">輸入字串</param>
    /// <param name="formatTypes">處理格式</param>
    /// <returns>已處理完成的字串</returns>
    public string Format(string text, FormatType formatTypes)
    {
      ITypeSetting typeSetting = null;
      switch (formatTypes)
      {
        case FormatType.Traditional:
          typeSetting = new Traditional();
          break;
        case FormatType.DoubleEOL:
          typeSetting = new DoubleEOL();
          break;
        default:
          break;
      }
      if (typeSetting != null)
        typeSetting.Set(ref text);

      return text;
    }

    public class PluginInfo : ICrawlService.PluginInfo
    {
      public string Name { get; set; }
      public string AliasName { get; set; }
      public string SupportUrl { get; set; }

      public PluginInfo(string name, string alias, string url) : base(name, alias, url)
      {
      }
    }

  }
}