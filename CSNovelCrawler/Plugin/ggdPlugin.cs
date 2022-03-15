using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("52ggdDownloader", "52ggd插件", "JeanLin", "1.0.0.0", "52格格黨下載插件", "http://www.52ggd.com")]
  public class ggdPlugin : AbstractPlugin
  {
    public ggdPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^http(s*)?:\/\/\w*\.52ggd.com(\/book(s*)(\/\d{1,3})*)*(\/xs(-1)*)*\/(?<TID>\d{3,7})";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new ggdDownloader(_loggerFactory);
    }

  }
}
