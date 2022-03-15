using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;

namespace CSNovelCrawler.Core
{
  public class ConfigManager
  {
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public ConfigManager(ILoggerFactory loggerFactory, IConfiguration config)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
      _config = config;
      LoadSettings();
    }

    public CustomSettings Settings { get; set; }

    /// <summary>
    /// 儲存設定
    /// </summary>
    public void SaveSettings(CustomSettings tempSettings)
    {
      //   this.configuration["CSNovelCrawler"].Add(WatchClipboardipboard, tempSettings.WatchClipboardipboard);
      //   this.configuration["CSNovelCrawler"].Add(DefaultSaveFolder, tempSettings.DefaultSaveFolder);
      //   this.configuration["CSNovelCrawler"].Add(HideSysTray, tempSettings.HideSysTray);
      //   this.configuration["CSNovelCrawler"].Add(Logging, tempSettings.Logging);
      //   this.configuration["CSNovelCrawler"].Add(TextEncodingoding, tempSettings.TextEncodingoding);
      //   this.configuration["CSNovelCrawler"].Add(SubscribeTime, tempSettings.SubscribeTime);
      //   this.configuration["CSNovelCrawler"].Add(SaveFolders, tempSettings.SaveFolders);
      //   this.configuration["CSNovelCrawler"].Add(SelectFormatName, tempSettings.SelectFormatName);
      //   this.configuration["CSNovelCrawler"].Add(SelectFormat, tempSettings.SelectFormat);
      //   this.configuration["CSNovelCrawler"].Add(CustomFormatFileName, tempSettings.CustomFormatFileName);
      throw new NotImplementedException();

    }

    /// <summary>
    /// 讀取設定
    /// </summary>
    /// <returns></returns>
    public void LoadSettings()
    {
      try
      {
        CustomSettings settings = new CustomSettings();
        _config.GetSection(key: "CSNovelCrawler").Bind(settings);
        this.Settings = settings;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }
    }

  }
}
