using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("quanBenDownloader", "quanBen插件", "JeanLin", "1.0.2.0", "quanBen下載插件", "http://big5.quanben5.io")]
  public class qbenPlugin : AbstractPlugin
  {
    public qbenPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^http(s*):\/\/\w*\.*big5\.quanben\d?(.io)*(.com)*\/n\/(?<TID>\S+)\/";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new qbenDownloader(_loggerFactory);
    }
  }
}
