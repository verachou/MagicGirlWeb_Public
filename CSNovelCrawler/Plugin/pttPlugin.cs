using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("PTTDownloader", "PTT插件", "JeanLin", "1.0.0.0", "PTT下載插件", "https://www.ptt.cc/")]
  public class pttPlugin : AbstractPlugin
  {    
    public pttPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^http(s):\/\/.*ptt.*\/(?<BOARD>[a-zA-Z\d]+)\/(?<TID>\S+)";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new pttDownloader(_loggerFactory);
    }

  }
}
