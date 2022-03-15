using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("wfxsDownloader", "wfxs插件", "JeanLin", "1.0.1.0", "wfxs下載插件", "https://www.wfxs.org")]
  public class wfxsPlugin : AbstractPlugin
  {
    public wfxsPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^https?:\/\/\w*\.*wfxs\d?.org\/(html|book)\/(?<TID>\d+)";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new wfxsDownloader(_loggerFactory);
    }
  }
}
