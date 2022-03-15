using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("卡orgDownloader", "ck101.org插件", "JeanLin", "1.0.0.0", "卡提諾小說下載插件", "https://www.ck101.org")]
  public class Ck101OrgPlugin : AbstractPlugin
  {
    public Ck101OrgPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^https?:\/\/\w*\.*ck101.org\/(book\/)*(\/)*(\d{1,3}\/)*(info-)*(wapbook-)*(?<TID>\d{3,6})";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new ck101OrgDownloader(_loggerFactory);
    }
  }
}
