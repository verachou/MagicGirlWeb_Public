using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("StoDownloader", "sto.cx插件", "JeanLin", "1.0.0.0", "思兔下載插件", "https://www.sto.cx")]
  public class stoPlugin : AbstractPlugin
  {
    public stoPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^https?:\/\/\w*\.sto.cx\/*([a-z]+)-*(?<TID>\d{3,7})";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new stoDownloader(_loggerFactory);
    }

  }
}
