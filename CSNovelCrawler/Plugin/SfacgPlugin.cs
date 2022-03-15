using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{

  [PluginInformation("sfDownloader", "sf插件", "JeanLin", "1.0.2.0", "sf下載插件", "http://book.sfacg.com")]
  public class SfacgPlugin : AbstractPlugin
  {
    public SfacgPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^http:\/\/\.*book\.sfacg\.com\/Novel\/(?<TID>\d+)";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new SfacgDownloader(_loggerFactory);
    }

  }
}
