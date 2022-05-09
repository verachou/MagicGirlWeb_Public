using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{

  [PluginInformation("屋Downloader", "hjwzw.com插件", "JeanLin", "1.0.4.0", "黃金屋下載插件", "https://tw.hjwzw.com/")]
  public class HjwzwPlugin : AbstractPlugin
  {
    public HjwzwPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^http(s*):\/\/tw\.*hjwzw.com\/Book(\/Chapter)*(\/Read)*\/(?<TID>\d+)";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new HjwzwDownloader(_loggerFactory);
    }

  }
}
