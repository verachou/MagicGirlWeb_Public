
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;
using CSNovelCrawler.Plugin;

namespace CSNovelCrawler.Core
{
  public class PluginManager
  {
    private readonly ILogger _logger;
    private List<IPlugin> _plugins;
    public List<IPlugin> Plugins
    {
      get
      {
        return _plugins;
      }
    }

    public PluginManager(ILoggerFactory loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
      _plugins = new List<IPlugin>();
      
      _plugins.Add(new BiqugePiugin(loggerFactory));
      _plugins.Add(new Ck101Plugin(loggerFactory));
      _plugins.Add(new Ck101OrgPlugin(loggerFactory));
      _plugins.Add(new czbooksPlugin(loggerFactory));
      _plugins.Add(new EightnovelPlugin(loggerFactory));
      _plugins.Add(new EynyPlugin(loggerFactory));
      _plugins.Add(new fc2Plugin(loggerFactory));
      _plugins.Add(new ggdPlugin(loggerFactory));
      _plugins.Add(new HjwzwPlugin(loggerFactory));
      _plugins.Add(new oldtimesPlugin(loggerFactory));
      _plugins.Add(new pttPlugin(loggerFactory));
      _plugins.Add(new qbenPlugin(loggerFactory));
      _plugins.Add(new RanwenPlugin(loggerFactory));
      _plugins.Add(new SfacgPlugin(loggerFactory));
      _plugins.Add(new stoPlugin(loggerFactory));

      // _plugins.Add(new eightcomicPlugin(loggerFactory));
    }

    /// <summary>
    /// 尋找對應網址的插件
    /// </summary>
    /// <returns></returns>
    public IPlugin GetPlugin(string url)
    {

      if (!string.IsNullOrEmpty(url))
      {

        // return CoreManager.PluginManager.Plugins.Find(plugin => plugin.CheckUrl(url));
        return Plugins.Find(plugin => plugin.CheckUrl(url));
      }

      return null;
    }

    /// <summary>
    /// 儲存插件設定
    /// </summary>
    /// <param name="plugin">需要儲存的插件</param>
    /// <returns>儲存成功為True，反之False</returns>
    /// 預定刪除功能
    // public bool SaveConfiguration(IPlugin plugin)
    // {
    //   try
    //   {
    //     string path = GetSettingFilePath(plugin);
    //     //建立文件夹
    //     if (!Directory.Exists(path)) Directory.CreateDirectory(Path.GetDirectoryName(path));

    //     //反序列化插件设置
    //     XmlSerializer oXmlSerializer = new XmlSerializer(typeof(DictionaryExtension<string, string>));
    //     using (FileStream oFileStream = new FileStream(path, FileMode.Create))
    //     {
    //       oXmlSerializer.Serialize(oFileStream, plugin.Configuration);
    //     }
    //     return true;
    //   }
    //   catch (Exception ex)
    //   {
    //     // log.Error(ex.ToString());
    //     _logger.LogError(ex.ToString());
    //     return false;
    //   }

    // }

    /// <summary>
    /// 讀取插件設定
    /// </summary>
    /// <param name="plugin"></param>
    /// 預定刪除功能
    // private void LoadConfiguration(IPlugin plugin)
    // {
    //   string path = GetSettingFilePath(plugin);
    //   //如果檔案存在
    //   if (File.Exists(path))
    //   {
    //     //反序列化插件設定
    //     XmlSerializer oFileStream = new XmlSerializer(typeof(DictionaryExtension<string, string>));
    //     using (FileStream fs = new FileStream(path, FileMode.Open))
    //     {
    //       try
    //       {
    //         plugin.Configuration = (DictionaryExtension<string, string>)oFileStream.Deserialize(fs);
    //       }
    //       catch (Exception ex)
    //       {
    //         // log.Error(ex.ToString());
    //         _logger.LogError(ex.ToString());
    //       }
    //     }
    //   }
    //   plugin.Configuration = plugin.Configuration ?? new DictionaryExtension<string, string>();

    // }

    /// <summary>
    /// 取得插件設定的檔案路徑
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    /// 預定刪除功能
    // private string GetSettingFilePath(IPlugin plugin)
    // {
    //   string path = Path.Combine(CoreManager.StartupPath, plugin + ".xml");
    //   return path;
    // }
  }
}
