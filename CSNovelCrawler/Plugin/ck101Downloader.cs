using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
namespace CSNovelCrawler.Plugin
{
  internal class Ck101Downloader : AbstractDownloader
  {
    private string str_regex = @"^https?:\/\/\w*\.*ck101.tw(\/thread-)*(\/forum.php\?mod=viewthread&tid=)*(?<TID>\d+).*";

    public Ck101Downloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
    }

    public HtmlDocument GetHtmlDocument(string url)
    {
      CurrentParameter.Url = url;
      var HtmlSource = Network.GetHtmlSource(CurrentParameter, Encoding.UTF8);
      return Network.GetHtmlDocument(HtmlSource);
    }

    public HtmlDocument GetHtmlDocumentReplaceDivToEmpty(string url)
    {
      CurrentParameter.Url = url;
      var HtmlSource = Network.GetHtmlSource(CurrentParameter, Encoding.UTF8);
      HtmlSource = Regex.Replace(HtmlSource, @"<\/?div[^<]*>", string.Empty, RegexOptions.Singleline);
      HtmlSource = Regex.Replace(HtmlSource, @"<!--[^<]*-->", string.Empty, RegexOptions.Singleline);
      return Network.GetHtmlDocument(HtmlSource);
    }

    /// <summary>
    /// 取得網頁上的基本資料
    /// </summary>
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

      TaskInfo.Url = string.Format("https://ck101.tw/thread-{0}-1-1.html", TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);

      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);

      ////取作者跟書名
      string htmlTitle = htmlRoot.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
      r = new Regex(@"(?<Title>(.(?!【))+)[^\u4e00-\u9fa5a-zA-Z0-9]*作\s*者[^\u4e00-\u9fa5a-zA-Z0-9]*(?<Author>[\u0800-\u9fa5\x3130-\x318Fa-zA-Z0-9]+)");
      m = r.Match(htmlTitle);
      if (m.Success)
      {
        TaskInfo.Author = m.Groups["Author"].Value.Trim();
        TaskInfo.Title = m.Groups["Title"].Value.Trim();
        TaskInfo.Title = Regex.Replace(TaskInfo.Title, @"[/\|\\\?""\*:><\.]+", "");
        _logger.LogDebug(LogMessage.Plugin.Author, TaskInfo.Author);
        _logger.LogDebug(LogMessage.Plugin.Title, TaskInfo.Title);
      }

      //取第一頁共幾樓
      htmlRoot = GetHtmlDocument(TaskInfo.Url);
      HtmlNodeCollection nodeSession = htmlRoot.DocumentNode.SelectNodes("//*[@class=\"pi\"]/strong/a/em");
      TaskInfo.PageSection = int.Parse(Regex.Replace(nodeSession[nodeSession.Count - 1].InnerText.Trim(), "[^0-9]", "", RegexOptions.IgnoreCase));

      //取總樓數
      HtmlNode node = htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"last\"]");
      String strLastPage = "";
      //兩頁以上
      if (node != null)
      {
        strLastPage = node.Attributes["href"].Value.Trim();

        htmlRoot = GetHtmlDocument(strLastPage);
      }
      else //只有一頁
      {
        htmlRoot = GetHtmlDocument(TaskInfo.Url);
      }

      nodeSession = null;
      nodeSession = htmlRoot.DocumentNode.SelectNodes("//*[@class=\"pi\"]/strong/a/em");
      String tempStr = Regex.Replace(nodeSession[nodeSession.Count - 1].InnerText.Trim(), "[^0-9]", "", RegexOptions.IgnoreCase);
      TaskInfo.TotalSection = int.Parse(tempStr);

      if (TaskInfo.BeginSection == 0)
      { TaskInfo.BeginSection = 1; }
      if (TaskInfo.EndSection == 0)
      { TaskInfo.EndSection = TaskInfo.TotalSection; }

      return true;
    }

    /// <summary>
    /// 取頁的樓層數
    /// </summary>
    /// <param name="htmlRoot"></param>
    /// <returns></returns>
    public int GetSection(HtmlDocument htmlRoot)
    {
      return htmlRoot.DocumentNode.SelectNodes("//*[@class=\"t_f\"]").Count;
    }

    public override bool Download()
    {
      CurrentParameter.IsStop = false;
      
      //排版插件
      var typeSetting = new Collection<ITypeSetting>();
      typeSetting.Add(new HtmlDecode());
      typeSetting.Add(new UniformFormat());

      Regex r = new Regex(@"(?<Head>^https?:\/\/\w*\.*ck101.tw\/thread-\d+-)(?<CurrentPage>\d+)(?<Tail>-\w+\.html)");
      Match m = r.Match(TaskInfo.Url);
      string urlHead = string.Empty, urlTail = string.Empty;
      if (m.Success)
      {
        urlHead = m.Groups["Head"].Value;
        urlTail = m.Groups["Tail"].Value;
      }

      HtmlDocument htmlRoot = null;
      HtmlNodeCollection nodeHeaders = null;
      int lastPage = 0;       
      int iMaxErrorCount = 5;
      int iCurrentErrorCount = 0;

      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {
        //要下載的頁數

        /**
         * 判斷目前的section在第幾頁，頁碼 = section/PageSection
         * if (第一頁) {
         *     抓取一樓的內容，以及19個class=t_f的物件 
         *     (因為第一頁一共20樓，但一樓為發文本文，t_f只有19個)
         * } else {
         *     抓取class=t_f的內容，最多會有20個
         * }
         */

        try
        {
          // 頁碼，無條件進位
          int newCurrentPage = Convert.ToInt16(Math.Ceiling(Convert.ToDouble(TaskInfo.BeginSection) / TaskInfo.PageSection));
          
          if (lastPage != newCurrentPage)//之前下載的頁數跟當前要下載的頁數
          {
            lastPage = newCurrentPage;//記錄下載頁數，下次如果一樣就不用重抓
            string url = urlHead + lastPage.ToString(CultureInfo.InvariantCulture) + urlTail;//組合網址
            if (lastPage == 1)//卡提諾第一頁的特別處理
            {
              switch (TaskInfo.FailTimes % 2)//常常取不到完整資料，用多個網址取
              {
                case 0:
                  url = string.Format("https://ck101.tw/thread-{0}-1-1.html", TaskInfo.Tid);
                  break;

                case 1:
                  url = string.Format("https://m.ck101.tw/forum.php?mod=redirect&ptid={0}&authorid=0&postno=1", TaskInfo.Tid);
                  break;

                case 2:
                  url = string.Format("https://m.ck101.tw/forum.php?mod=redirect&ptid={0}&authorid=0&postno=1", TaskInfo.Tid);
                  break;
                case 3:
                  url = string.Format("https://m.ck101.tw/forum.php?mod=viewthread&tid={0}&extra=page%3D1", TaskInfo.Tid);
                  break;
              }
            }
            htmlRoot = GetHtmlDocument(url);
          }

          if (htmlRoot != null)
          {
            // 第一頁，特別抓取一樓的內容           
            nodeHeaders = htmlRoot.DocumentNode.SelectNodes("//div[@class=\"t_fsz\"]");
          }

          //計算要取的區塊在第幾個
          int partSection = TaskInfo.BeginSection - ((lastPage - 1) * TaskInfo.PageSection) - 1;
          if (nodeHeaders == null)
          {
            throw new Exception("無下載資料");
          }
          else if (nodeHeaders.Count > partSection)
          {
            Network.RemoveSubHtmlNode(nodeHeaders[partSection], "div");
            Network.RemoveSubHtmlNode(nodeHeaders[partSection], "ignore_js_op");
            Network.RemoveSubHtmlNode(nodeHeaders[partSection], "i");
            Network.RemoveSubHtmlNode(nodeHeaders[partSection], "script");
            string tempTxt = nodeHeaders[partSection].InnerText;

            foreach (var item in typeSetting)
            {
              item.Set(ref tempTxt);
            }
            FileWrite.TxtWrire(tempTxt, TaskInfo.SaveFullPath, TaskInfo.TextEncoding);
          }
          iCurrentErrorCount = 0;
        }
        catch (Exception ex)
        {
          //CoreManager.LoggingManager.Debug(ex.ToString());
          //發生錯誤，當前區塊重取          
          _logger.LogDebug(LogMessage.Plugin.BeginSection, TaskInfo.BeginSection);
          _logger.LogError(ex.ToString());
          iCurrentErrorCount++;
          if (iCurrentErrorCount <= iMaxErrorCount)
          {
            TaskInfo.BeginSection--;
            TaskInfo.FailTimes++;
            lastPage = 0;
          }
          continue;
        }

        TaskInfo.HasStopped = CurrentParameter.IsStop;
      }

      bool finish = TaskInfo.CurrentSection == TaskInfo.EndSection;
      return finish;
    }
  }
}
