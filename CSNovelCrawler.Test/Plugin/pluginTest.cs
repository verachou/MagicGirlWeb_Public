using NUnit.Framework;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Text;
using System.Net;
using System.IO;
using CSNovelCrawler.Core;
using CSNovelCrawler.Interface;
using CSNovelCrawler.Class;
using CSNovelCrawler.Test;

namespace CSNovelCrawler.Test.Plugin
{
  [TestFixture]
  public class pluginTest
  {

    private ILoggerFactory _loggerFactory;

    [SetUp]
    public void Setup()
    {
      _loggerFactory = Substitute.For<ILoggerFactory>();
      //   Directory.Delete(@"../bin/Debug/net5.0/books", true); // true: 移除 path 中的目錄、子目錄和檔案
      string filePath = @"./books";
      CommonTool tool = new CommonTool();
      tool.CleanFileDirectory(filePath);
    }

    [Test]
    [TestCase("https://czbooks.net/n/cf2efm")]
    // [TestCase("https://www.sto.cx/book-179093-1.html")]
    public void PreTest_CheckUrl_isHTTP200(string inputUrl)
    {
      // Arrange    
      string url = inputUrl;
      WebRequest request = WebRequest.Create(url);

      // Act
      HttpWebResponse response = (HttpWebResponse)request.GetResponse();

      // Assert
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Test]
    [TestCase("https://czbooks.net/n/cf2efm",           "《（ABO）陽澄湖帝王》",        "作者: superpanda")]
    [TestCase("https://www.sto.cx/book-179093-1.html",  "《合籠蠱》",                   "作者: 首初")]
    public void AnalysisTask_InputUrl_GetInformation(
        string inputUrl,
        string expectTitle,
        string expectAuthor
    )
    {
      try
      {
        // Arrange    
        string url = inputUrl;

        var appSettings = @"{""CSNovelCrawler"": {
                    ""DefaultSaveFolder"": ""./books"",
                    ""WatchClipboard"": true,
                    ""HideSysTray"": false,
                    ""Logging"": false,
                    ""TextEncoding"": ""utf-8"",
                    ""SubscribeTime"": 10,
                    ""SaveFolders"": """",
                    ""SelectFormatName"": ""書名"",
                    ""SelectFormat"": ""%Title%"",
                    ""CustomFormatFileName"": """"
            }}";

        var builder = new ConfigurationBuilder();
        builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));
        IConfiguration config = builder.Build();

        // Act
        CoreManager coreManager = new CoreManager(_loggerFactory, config);
        IPlugin plugin = coreManager.PluginManager.GetPlugin(url);
        TaskInfo taskInfo = coreManager.TaskManager.AddTask(plugin, url);
        coreManager.TaskManager.AnalysisTask(taskInfo);
        string actualTitle = taskInfo.Title;
        string actualAuthor = taskInfo.Author;

        // Assert
        Assert.AreEqual(expectTitle, actualTitle);
        Assert.AreEqual(expectAuthor, actualAuthor);
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }

    [Test]
    [TestCase("https://czbooks.net/n/cf2efm",           @"./books/《（ABO）陽澄湖帝王》.txt")]
    [TestCase("https://www.sto.cx/book-179093-1.html",  @"./books/《合籠蠱》.txt")]
    public void DownloadTask_InputUrl_FileIsExist(
        string inputUrl,
        string expectFilePath
    )
    {
      // Arrange    
      string url = inputUrl;

      var appSettings = @"{""CSNovelCrawler"": {
                    ""DefaultSaveFolder"": ""./books"",
                    ""WatchClipboard"": true,
                    ""HideSysTray"": false,
                    ""Logging"": false,
                    ""TextEncoding"": ""utf-8"",
                    ""SubscribeTime"": 10,
                    ""SaveFolders"": """",
                    ""SelectFormatName"": ""書名"",
                    ""SelectFormat"": ""%Title%"",
                    ""CustomFormatFileName"": """"
            }}";

      var builder = new ConfigurationBuilder();
      builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));
      IConfiguration config = builder.Build();

      // Act
      CoreManager coreManager = new CoreManager(_loggerFactory, config);
      IPlugin plugin = coreManager.PluginManager.GetPlugin(url);
      TaskInfo taskInfo = coreManager.TaskManager.AddTask(plugin, url);


      // taskInfo.Url = url;
      // taskInfo.Status = DownloadStatus.AnalysisComplete;
      // taskInfo.CustomFileName = "《（ABO）陽澄湖帝王》";
      // taskInfo.Title = "《（ABO）陽澄湖帝王》";
      // taskInfo.Author = "作者: superpanda";
      // taskInfo.BeginSection = 1;
      // taskInfo.EndSection = 4;
      // taskInfo.TotalSection = 4;

      // 理論上DownloadTask的測試應和Analysis鬆綁
      // 但plugin.CreateDownloder()被封裝在taskInfo.Analysis中無法外部呼叫
      coreManager.TaskManager.AnalysisTask(taskInfo);
      taskInfo.EndSection = 3;
      coreManager.TaskManager.DownloadTask(taskInfo);

      // Assert
      Assert.That(expectFilePath, Does.Exist);
    }

  }
}