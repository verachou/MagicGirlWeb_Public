using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{

  [PluginInformation("8comicDownloader", "8comic.cn插件", "JeanLin", "1.0.1.0", "無限漫畫下載插件", "http://www.8comic.com/")]
  public class eightcomicPlugin : AbstractPlugin
  {
    public eightcomicPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^http:\/\/new\.comicvip\.com\/show";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new eightcomicDownloader(_loggerFactory);
    }
  }
}
