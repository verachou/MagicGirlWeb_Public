using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;
using CSNovelCrawler.Core;

namespace CSNovelCrawler.Plugin
{
  [Serializable]
  public abstract class AbstractDownloader : IDownloader
  {
    protected ILogger _logger;
    public TaskInfo TaskInfo { get; set; }

    public AbstractDownloader(ILoggerFactory loggerFactory)
    {
      CurrentParameter = new DownloadParameter
      {
        UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36",
      };
    }

    public DownloadParameter CurrentParameter { get; set; }

    public abstract bool Analysis();


    public abstract bool Download();


    public void StopDownload()
    {
      if (CurrentParameter != null)
      {
        //將停止旗標設為true
        CurrentParameter.IsStop = true;
      }
    }

  }
}
