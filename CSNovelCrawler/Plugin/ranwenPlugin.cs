using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("燃Downloader", "ranwen.net插件", "JeanLin", "1.0.3.0", "燃文下載插件", "https://www.ranwena.com")]
  public class RanwenPlugin : AbstractPlugin
  {
    public RanwenPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"(^http(s*):\/\/\w*\.*ranwena.com(\/files)*(\/article)*\/\d+\/(?<TID>\d+)\/)";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new RanwenDownloader(_loggerFactory);
    }

  }
}
