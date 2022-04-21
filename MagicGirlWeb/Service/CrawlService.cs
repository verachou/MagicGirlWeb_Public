using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
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
        var attribute = (PluginInformationAttribute) Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginInformationAttribute));
        var pluginInfo = new PluginInfo(attribute.Name, attribute.FriendlyName, attribute.SupportUrl);
        PluginInfos.Add(pluginInfo);
      }
    }

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

    public Book Download(
      string url,
      int lastPageFrom,
      int lastPageTo,
      string filePath)
    {
      // TaskInfo taskInfo = _taskList.Find(x => x.Url == url);
      Book book = Analysis(url);
      TaskInfo taskInfo = _taskList.Find(x => x.Url == url);

      // 若該url找不到對應的taskInfo，則自動將其分析後取得該taskInfo
      // if (taskInfo == null)
      // {
      //   _logger.LogInformation(CustomMessage.ObjectIsNull, nameof(taskInfo));
      //   Analysis(url);
      //   taskInfo = _taskList.Find(x => x.Url == url);
      // }
      taskInfo.BeginSection = lastPageFrom;
      taskInfo.EndSection = lastPageTo;

      _coreManager.TaskManager.DownloadTask(taskInfo);

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

    public class PluginInfo: ICrawlService.PluginInfo
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