using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("筆Downloader", "biquge.com插件", "JeanLin", "1.0.3.0", "筆趣讀下載插件", "https://www.biqudu.com")]
  public class BiqugePiugin : AbstractPlugin
  {
    public BiqugePiugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {     
      pattern = @"^http(s*):\/\/\w*\.*biqudu.com\/(?<TID>\d+\D+\d+)\/";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new BiqugeDownloader(_loggerFactory);
    }

  }
}
