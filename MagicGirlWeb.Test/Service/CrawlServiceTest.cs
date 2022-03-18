using NUnit.Framework;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using MagicGirlWeb.Models;
using MagicGirlWeb.Service;

namespace MagicGirlWeb.Test.Service
{
  [TestFixture]
  public class CrawlServiceTest
  {
    private ICrawlService _crawlService;
    private ILoggerFactory _loggerFactory;
    private IConfiguration _config;
    private string _filePath;

    [SetUp]
    public void Setup()
    {
      this._loggerFactory = Substitute.For<ILoggerFactory>();
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
      this._config = builder.Build();

      _crawlService = new CrawlService(_loggerFactory, _config);

      // 每次測試前清出檔案產生目錄
      _filePath = @"./books";
      if (Directory.Exists(_filePath))
      {
        Directory.Delete(_filePath, true); // true: 移除 path 中的目錄、子目錄和檔案
        Directory.CreateDirectory(_filePath);
      }
    }

    [Test]
    [TestCase("https://czbooks.net/n/cf2efm", "《（ABO）陽澄湖帝王》", "作者: superpanda", 1, 17)]
    public void Analysis_InputUrl_ReturnBook(
        string inputUrl,
        string expectTitle,
        string expectAuthor,
        int expectPageFrom,
        int expectPageTo
    )
    {
      // Arrange      
      string url = inputUrl;

      // Act
      Book actualBook = _crawlService.Analysis(url);
      IEnumerator<BookWebsite> enumBookWebsite = actualBook.BookWebsites.GetEnumerator();
      enumBookWebsite.MoveNext();                                                           // 取得第一筆資料
      BookWebsite actualBookWebsite = enumBookWebsite.Current;

      // Assert
      Assert.AreEqual(expectTitle, actualBook.Name);
      Assert.AreEqual(expectAuthor, actualBook.Author.Name);
      Assert.AreEqual(expectPageFrom, actualBookWebsite.LastPageFrom);
      Assert.AreEqual(expectPageTo, actualBookWebsite.LastPageTo);
    }

    [Test]
    [TestCase("https://czbooks.net/n/cf2efm", 5, "　　《（ABO）陽澄湖帝王》第0章　　書名：（ABO）陽澄湖帝王　　作者：superpanda")]
    public void Download_InputUrl_FileIsDownload(
        string inputUrl,
        int testRowCount,
        string expectContent
    )
    {
      // Arrange      
      string url = inputUrl;
      bool ActualReturn = false;
      string actualContent = "";
      string downloadFileName = _filePath + @"\" + "CrawlService下載測試.txt";
      
      // Act
      // CrawlService.Download()正常情況下會將fileStream dispose, 安全起見這邊用using確保檔案資源被正常釋放
      using (FileStream fileStream = new FileStream(downloadFileName, FileMode.OpenOrCreate))
      {
        ActualReturn = _crawlService.Download(url, 1, 1, fileStream);
      }

      using (FileStream fileStream = new FileStream(downloadFileName, FileMode.Open))
      {
        StreamReader reader = new StreamReader(fileStream);

        for (int i = 0; i < testRowCount; i++)
        {
          actualContent += reader.ReadLine();
        }
      }

      // Assert
      Assert.True(ActualReturn);
      Assert.AreEqual(expectContent, actualContent);
    }

    [Test]
    [TestCase("https://czbooks.net/n/cf2efm", @"./books/《（ABO）陽澄湖帝王》.txt")]
    public void DeleteLocalFile_InputUrl_FileIsDeleted(
        string inputUrl,
        string expectDeleteFile
    )
    {
      // Arrange      
      string url = inputUrl;
      Book actualBook = _crawlService.Analysis(url);
      if (!File.Exists(expectDeleteFile))
      {
        File.Create(expectDeleteFile).Dispose();
      }

      // Act
      _crawlService.DeleteLocalFile(url);

      // Assert
      Assert.That(expectDeleteFile, Does.Not.Exist);
    }


  }


}