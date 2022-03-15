using System.Globalization;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;


namespace CSNovelCrawler.Plugin
{

  public class EynyDownloader : AbstractDownloader
  {
    private string str_regex = @"^http(s*):\/\/www\w*\.eyny.com\/thread-(?<TID>\d+)+-\d+-\w+\.html";

    public EynyDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
      // CurrentParameter.Timeout = Timeout = 10000;
    }

    public HtmlDocument GetHtmlDocument(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlDocument(Network.GetHtmlSource(CurrentParameter, Encoding.UTF8));
    }


    public override bool Analysis()
    {
      //取TID
      Regex r = new Regex(str_regex);
      Match m = r.Match(TaskInfo.Url);
      if (m.Success)
      {
        TaskInfo.Tid = m.Groups["TID"].Value;
        _logger.LogDebug(LogMessage.Plugin.Tid, TaskInfo.Tid);
      }


      TaskInfo.Url = //string.Format(@"http://archiver.eyny.com/archiver/tid-{0}-1.html", TaskInfo.Tid);
          Regex.Replace(TaskInfo.Url, @"(?!^https:\/\/\w*\.eyny.com\/thread-\d+-)(?<CurrentPage>\d+)(?=-\w+\.html)", "1");

      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);

      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);

      //取作者跟書名
      string htmlTitle = htmlRoot.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
      r = new Regex(@"(?<Author>\S+)\s*-\s*【(?<Title>\S+)】");
      m = r.Match(htmlTitle);
      if (m.Success)
      {
        TaskInfo.Author = m.Groups["Author"].Value;
        TaskInfo.Title = m.Groups["Title"].Value;
        TaskInfo.Title = Regex.Replace(TaskInfo.Title, @"[/\|\\\?""\*:><\.]+", "");
        TaskInfo.Title = "《" + TaskInfo.Title + "》";
        _logger.LogDebug(LogMessage.Plugin.Author, TaskInfo.Author);
        _logger.LogDebug(LogMessage.Plugin.Title, TaskInfo.Title);
      }

      //取總頁數
      HtmlNodeCollection nodeHeaders2 = htmlRoot.DocumentNode.SelectNodes("//*[@id=\"pgt\"]/div[1]/div/a");
      //只有10頁以下時會取不到最後一頁
      int LastPageIndex = 0;
      if (nodeHeaders2.Count == 13)
      {
        LastPageIndex = nodeHeaders2.Count - 3;
      }
      else
      {
        LastPageIndex = nodeHeaders2.Count - 2;
      }
      string s = nodeHeaders2[LastPageIndex].InnerText;
      r = new Regex(@"(?<TotalPage>\d+)");
      m = r.Match(s);
      if (m.Success)
      {
        TaskInfo.TotalPage = CommonTools.TryParse(m.Groups["TotalPage"].Value, 0);
      }

      TaskInfo.PageSection = GetSection(htmlRoot);
      TaskInfo.TotalSection = TaskInfo.PageSection * (TaskInfo.TotalPage - 1) +
          GetSection(
              GetHtmlDocument(Regex.Replace(TaskInfo.Url, @"(?!^https:\/\/\w*\.eyny.com\/thread-\d+-)(?<CurrentPage>\d+)(?=-\w+\.html)", TaskInfo.TotalPage.ToString(CultureInfo.InvariantCulture)))
         );
      if (TaskInfo.BeginSection == 0)
      { TaskInfo.BeginSection = 1; }
      if (TaskInfo.EndSection == 0)
      { TaskInfo.EndSection = TaskInfo.TotalSection; }
      return true;
    }

    public int GetSection(HtmlDocument htmlRoot)
    {
      return htmlRoot.DocumentNode.SelectNodes("//*[@id=\"postlist\"]/div/table/tr[1]/td[2]/div[2]/div[2]/div[1]/table[1]/tr[1]/td[1]").Count;
    }


    public override bool Download()
    {
     string urlHead = string.Empty, urlTail = string.Empty;
      
      //http://archiver.eyny.com/archiver/tid-9169460-1.html
      urlHead = string.Format(@"http://archiver.eyny.com/archiver/tid-{0}-", TaskInfo.Tid);
      urlTail = @".html";
      HtmlNodeCollection nodeHeaders = null;

      int lastPage = 0;

      //排版插件
      var typeSetting = new Collection<ITypeSetting>();
      typeSetting.Add(new EynyTag()); 
      typeSetting.Add(new HtmlDecode());
      typeSetting.Add(new UniformFormat());

      string RawData = "";
      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {


        try
        {
          //要下載的頁數
          int newCurrentPage = (TaskInfo.BeginSection + TaskInfo.PageSection - 1) / TaskInfo.PageSection;

          if (lastPage != newCurrentPage)//之前下載的頁數跟當前要下載的頁數
          {
            lastPage = newCurrentPage;//記錄下載頁數，下次如果一樣就不用重抓
            string url = urlHead + lastPage + urlTail;//組合網址
                                                      //log.Debug("download url=" + url);

            HtmlDocument htmlRoot = GetHtmlDocument(url);

            if (htmlRoot != null)
            {
              nodeHeaders = htmlRoot.DocumentNode.SelectNodes("//*[@id=\"content\"]");

            }

            Network.RemoveSubHtmlNode(nodeHeaders[0], "div");
            Network.RemoveSubHtmlNode(nodeHeaders[0], "ignore_js_op");
            Network.RemoveSubHtmlNode(nodeHeaders[0], "i");
            Network.RemoveSubHtmlNode(nodeHeaders[0], "p", "strong");
            RawData = nodeHeaders[0].InnerText;
            RawData += "\r\n發表於 2001-1-1 1:1 PM";

            foreach (var item in typeSetting)
            {
              item.Set(ref RawData);
            }
            if (nodeHeaders == null)
            {
              throw new Exception("下載資料為空的");
            }

          }

          //計算要取的區塊在第幾個
          int partSection = TaskInfo.BeginSection - ((lastPage - 1) * TaskInfo.PageSection) - 1;



          Regex r = new Regex(@"((發表於(( [昨前]天 \d+:\d+ [PA]M)|( \d+-\d+-\d+ \d+:\d+ [PA]M)|( .+?前))))(?<Main>.+?)(?=(發表於(( [昨前]天 \d+:\d+ [PA]M)|( \d+-\d+-\d+ \d+:\d+ [PA]M)|( .+?前))))", RegexOptions.Singleline);
          var m = r.Matches(RawData);
          string tempTxt = m[partSection].Groups["Main"].Value;
          FileWrite.TxtWrire(tempTxt, TaskInfo.SaveFullPath, TaskInfo.TextEncoding);


        }
        catch (Exception ex)
        {
          //發生錯誤，當前區塊重取  
          _logger.LogError(ex.ToString());
          TaskInfo.BeginSection--;
          TaskInfo.FailTimes++;
          lastPage = 0;

          continue;
        }


        TaskInfo.HasStopped = CurrentParameter.IsStop;
      }

      bool finish = TaskInfo.CurrentSection == TaskInfo.EndSection;
      return finish;
    }




  }
}
