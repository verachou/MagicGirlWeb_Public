using System;
using System.IO;
using System.Collections.Generic;
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
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly CoreManager _coreManager;
    private List<TaskInfo> _taskList;
    public CrawlService(ILoggerFactory loggerFactory, IConfiguration config)
    {
      this._loggerFactory = loggerFactory;
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      this._logger = loggerFactory.CreateLogger(className);
      this._config = config;
      this._coreManager = new CoreManager(_loggerFactory, _config);
      this._taskList = new List<TaskInfo>();
    }
    public Book Analysis(string url)
    {
      Book book = null;

      IPlugin plugin = _coreManager.PluginManager.GetPlugin(url);
      TaskInfo taskInfo = _coreManager.TaskManager.AddTask(plugin, url);
      _coreManager.TaskManager.AnalysisTask(taskInfo);
      if (taskInfo.Status == DownloadStatus.AnalysisComplete)
      {
        _taskList.Add(taskInfo);
        BookWebsite bookWebsite = new BookWebsite(
              taskInfo.Url,
              taskInfo.BasePlugin.GetHash(url), // SourceId
              taskInfo.BeginSection,            // LastPageFrom
              taskInfo.EndSection               // LastPageTo
          );

        List<BookWebsite> bookWebsites = new List<BookWebsite>();
        bookWebsites.Add(bookWebsite);

        Author author = new Author(taskInfo.Author);

        book = new Book(
            taskInfo.Title,
            taskInfo.TotalSection,
            author,
            bookWebsites
          );
      }
      return book;
    }

    public bool Download(string url, int lastPageFrom, int lastPageTo, FileStream fileStream)
    {
      TaskInfo taskInfo = _taskList.Find(x => x.Url == url);

      // 若該url找不到對應的taskInfo，則自動將其分析後取得該taskInfo
      if (taskInfo == null)
      {
        _logger.LogInformation(CustomMessage.ObjectIsNull, nameof(taskInfo));
        Analysis(url);
        taskInfo = _taskList.Find(x => x.Url == url);
      }
      taskInfo.BeginSection = lastPageFrom;
      taskInfo.EndSection = lastPageTo;
      _coreManager.TaskManager.DownloadTask(taskInfo);

      if (taskInfo.Status != DownloadStatus.DownloadComplete)
      {
        return false;
      }

      try
      {
        using (FileStream downloadfile = new FileStream(taskInfo.SaveFullPath, FileMode.Open))
        {
          downloadfile.CopyTo(fileStream);
          fileStream.Dispose();
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        return false;
      }

      return true;
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


  }
}