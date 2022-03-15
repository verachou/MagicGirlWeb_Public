using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("卡comDownloader", "ck101.com插件", "JeanLin", "1.2.1.0", "卡提諾論壇下載插件", "https://ck101.com/")]
  public class Ck101Plugin : AbstractPlugin
  {
    public Ck101Plugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^https?:\/\/\w*\.*ck101.com(\/thread-)*(\/forum.php\?mod=viewthread&tid=)*(?<TID>\d+).*";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new Ck101Downloader(_loggerFactory);
    }
  }
}
