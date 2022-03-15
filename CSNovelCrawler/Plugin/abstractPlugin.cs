using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  public abstract class AbstractPlugin : IPlugin
  {
    protected ILogger _logger;
    protected ILoggerFactory _loggerFactory;
    protected string pattern;
    public string PluginName;

    public AbstractPlugin(ILoggerFactory loggerFactory)
    {
      this._loggerFactory = loggerFactory;
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }
    /// <summary>
    /// 建立IDownloader物件
    /// </summary>
    /// <returns></returns>
    public abstract IDownloader CreateDownloader();

    /// <summary>
    /// 檢查url能使用哪個插件
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public virtual bool CheckUrl(string url)
    {
      Regex r = new Regex(pattern);
      Match m = r.Match(url);
      if (m.Success)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// 取得url的唯一碼
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public virtual string GetHash(string url)
    {
      Regex r = new Regex(pattern);
      Match m = r.Match(url);
      if (m.Success)
      {

        return PluginName + m.Groups["TID"].Value;
      }
      return null;
    }


    /// <summary>
    ///插件的擴充功能
    /// </summary>
    /// 預定刪除功能
    // public Dictionary<string, object> Extensions { get; } 
    /// <summary>
    /// 插件儲存設定
    /// </summary>
    /// 預定刪除功能
    // public DictionaryExtension<string, string> Configuration { get; set; }

  }
}