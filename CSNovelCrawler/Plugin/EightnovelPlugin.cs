using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{

  [PluginInformation("無限Downloader", "8novel.com插件", "JeanLin", "1.0.2.0", "無限小說下載插件", "https://8book.com")]
  public class EightnovelPlugin : AbstractPlugin
  {
    // private string str_regex = @"^http(s*):\/\/8book.com(\/readbook\/\d+\/)*(\/books\/novelbook_)*(?<TID>\d+).*";

    public EightnovelPlugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^http(s*):\/\/8book.com(\/readbook\/\d+\/)*(\/books\/novelbook_)*(?<TID>\d+).*";
      PluginName = "8novel";
    }

    public override IDownloader CreateDownloader()
    {
      return new EightnovelDownloader(_loggerFactory);
    }

    
  }
}
