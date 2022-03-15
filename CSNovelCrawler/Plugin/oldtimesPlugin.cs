using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("oldtimesDownloader", "oldtimes插件", "JeanLin", "1.0.0.0", "舊時光下載插件", "https://www.oldtimescc.cc")]
  public class oldtimesPlugin : AbstractPlugin
  {
    public oldtimesPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^http(s*)?:\/\/\w*\.oldtimescc.cc\/go\/(?<TID>\d{1,7})";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new oldtimesDownloader(_loggerFactory);
    }
  }
}
