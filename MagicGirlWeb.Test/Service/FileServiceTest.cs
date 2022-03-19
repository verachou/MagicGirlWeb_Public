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
  public class FileServiceTest
  {
    private IFileService _fileService;
    private ILoggerFactory _loggerFactory;
    private IConfiguration _config;
    private string _filePath;

    [SetUp]
    public void Setup()
    {
      this._loggerFactory = Substitute.For<ILoggerFactory>();
      var appSettings =
      @"{""Authentication"": 
          {
            ""Google"": {
              ""DriveApiKey"": ""F:/vscode/github/CSharpExercise/testSolution/testConsole/credentials.json"",
              ""ApplicationName"": ""MagicGirlWeb FileService"",
              ""DriveFolderId"": ""14M4JwGgWRe0dWBePc0XXtgVIGh_7PQw9""
            }
          }
        }";
      var builder = new ConfigurationBuilder();
      builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));
      this._config = builder.Build();

      _fileService = new FileService(_loggerFactory, _config);

      // 每次測試前清出檔案產生目錄
      _filePath = @"./books";
      if (Directory.Exists(_filePath))
      {
        Directory.Delete(_filePath, true); // true: 移除 path 中的目錄、子目錄和檔案
        Directory.CreateDirectory(_filePath);
      }
    }

    [Test]
    [TestCase("1DkHvDeGvwLZOckQRAOMNpRsGGHyu4gWY", 5, "　　《（ABO）陽澄湖帝王》第0章　　書名：（ABO）陽澄湖帝王　　作者：superpanda")]
    public void Download_InputFileId_FileIsDownload(
      string inputFileId,
      int testRowCount,
      string expectContent
    )
    {
      // Arrange      
      string fileId = inputFileId;
      string fileName = "FileService下載測試.txt";
      string folderName = _filePath;
      string actualFile = folderName + @"/" + fileName;
      string actualContent = "";
      bool ActualReturn = false;
      // string fileId = "1DkHvDeGvwLZOckQRAOMNpRsGGHyu4gWY";
      // string fileName = "FileService下載測試.txt";
      // string folderName = @"./books";
      // string actualFile = folderName + @"/" + fileName;
      // string actualContent = "";
      // bool ActualReturn = false;

      // Act
      // FileService.Download()正常情況下會將fileStream dispose, 安全起見這邊用using確保檔案資源被正常釋放
      using (FileStream fileStream = new FileStream(actualFile, FileMode.OpenOrCreate, FileAccess.Write))
      {
        ActualReturn = _fileService.Download(fileId, fileStream);
      }

      using (FileStream fileStream = new FileStream(actualFile, FileMode.Open))
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
    [TestCase("上傳檔案A.txt", "text/plain;charset=UTF-8", "PluginName")]
    public void Upload_InputFileName_ReturnFileId(
      string inputFileName, 
      string inputMimeType,
      string inputDescription
    )
    {
      // Arrange      
      string fileName = inputFileName;
      string folderName = _filePath;
      string uploadFile = folderName + @"/" + fileName;
      string mimeType = inputMimeType;

      using (FileStream stream = new FileStream(uploadFile, FileMode.OpenOrCreate))
      {
        StreamWriter sw = new StreamWriter(stream);
        sw.WriteLine("This is test file {0}.", fileName);
        sw.Close();
      }

      // Act
      string expectFileId = _fileService.Upload(fileName, folderName, mimeType, inputDescription);

      // Assert
      Assert.NotNull(expectFileId);
    }



  }


}