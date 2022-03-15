using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("伊Downloader", "eyny.com插件", "JeanLin", "1.0.4.0", "伊莉下載插件", "https://www.eyny.com/forum.php?gid=1747")]
  public class EynyPlugin : AbstractPlugin
  {
    public EynyPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^http(s*):\/\/www\w*\.eyny.com\/thread-(?<TID>\d+)+-\d+-\w+\.html";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new EynyDownloader(_loggerFactory);
    }

  }
}
