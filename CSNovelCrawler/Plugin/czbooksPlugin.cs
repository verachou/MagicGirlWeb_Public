using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("狂人Downloader", "czbooks插件", "JeanLin", "1.0.1.0", "小說狂人下載插件", "https://czbooks.net")]
  public class czbooksPlugin : AbstractPlugin
  {
    public czbooksPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^https?:\/\/czbooks.net\/n\/(?<TID>[a-z0-9]*)";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new czbooksDownloader(_loggerFactory);
    }
  }
}
