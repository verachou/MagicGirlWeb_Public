using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using CSNovelCrawler.Class;
// using CSNovelCrawler.Core;


namespace CSNovelCrawler.Plugin
{
  internal class pttDownloader : AbstractDownloader
  {
    private string str_regex = @"^http(s):\/\/.*ptt.*\/(?<BOARD>[a-zA-Z\d]+)\/(?<TID>\S+)";

    public pttDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
      CurrentParameter.Timeout = 10000;

      //PTT有些版18歲才能進，必須在cookie中加入over18=1
      var cookie = new CookieContainer();
      cookie.Add(new Cookie("over18", "1") { Domain = "www.ptt.cc" });
      CurrentParameter.Cookies = cookie;
    }

    public HtmlDocument GetHtmlDocument(string url)
    {
      CurrentParameter.Url = url;
      var HtmlSource = Network.GetHtmlSource(CurrentParameter, Encoding.UTF8);
      return Network.GetHtmlDocument(HtmlSource);
    }
    public string GetHtmlString(string url)
    {

      CurrentParameter.Url = url;
      return Network.GetHtmlSource(CurrentParameter, Encoding.UTF8);
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
      //取TID，由於有好幾個PTT內容農場，需實際至網頁取得真正ptt的網頁版網址，才能得到真正的版名跟文章代號
      /* 支援:
       * https://www.ptt.cc
       * https://www.pttweb.cc
       * https://pttweb.tw
       */

      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);
      htmlRoot.DocumentNode.SelectNodes("//a[@href]");
      HtmlNodeCollection tempNodeCol = htmlRoot.DocumentNode.SelectNodes("//a[@href]");
      try
      {
        foreach (HtmlNode tempNode in tempNodeCol)
        {
          var tmpUrl = tempNode.GetAttributeValue("href", string.Empty);
          //log.Debug("URL List = " + tmpUrl);
          if (tmpUrl.StartsWith("https://www.ptt.cc/bbs"))
          {
            TaskInfo.Url = tmpUrl;
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(LogMessage.Plugin.ErrorMessage, ex.ToString());
      }

      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);

      Regex r = new Regex(str_regex);
      Match m = r.Match(TaskInfo.Url);
      if (m.Success)
      {
        var board = m.Groups["BOARD"].Value;
        var tid = m.Groups["TID"].Value.Replace(".html", "");
        _logger.LogDebug(LogMessage.Plugin.Board, board);
        TaskInfo.Tid = board + @"\" + tid;
        _logger.LogDebug(LogMessage.Plugin.Tid, TaskInfo.Tid);
      }

      //用HtmlAgilityPack分析
      htmlRoot = GetHtmlDocument(TaskInfo.Url);

      //取作者跟書名
      var title = htmlRoot.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]").Attributes["content"].Value.Trim();
      title = Regex.Replace(title, @"[/\|\\\?""\*:><\.]+", "");
      _logger.LogDebug(LogMessage.Plugin.HtmlTitle, title);
      var auther = htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"main-content\"]/div[1]/span[2]").InnerText;
      TaskInfo.Author = auther;
      TaskInfo.Title = title;
      _logger.LogDebug(LogMessage.Plugin.Author, TaskInfo.Author);
      _logger.LogDebug(LogMessage.Plugin.Title, TaskInfo.Title);

      //取總頁數
      TaskInfo.TotalSection = 1;
      TaskInfo.BeginSection = 1;
      TaskInfo.EndSection = 1;
      TaskInfo.CurrentSection = 0;

      return true;
    }

    public override bool Download()
    {
      CurrentParameter.IsStop = false;

      HtmlNodeCollection nodeHeaders = null;
      int lastPage = 0;
      //排版插件
      var typeSetting = new Collection<ITypeSetting>();
      typeSetting.Add(new BrRegex());
      typeSetting.Add(new PttFormat());

      string url = TaskInfo.Url;

      // log.Debug("download url=" + url);
      _logger.LogDebug(LogMessage.Plugin.MethodName + LogMessage.Plugin.Url, System.Reflection.MethodBase.GetCurrentMethod().Name, url);

      try
      {
        string htmlstring = GetHtmlString(url);
        //標題都黏在一起，處理一下
        htmlstring = htmlstring.Replace("<span class=\"article-meta-tag\">", "<br><br><span class=\"article-meta-tag\">");
        htmlstring = htmlstring.Replace("<span class=\"article-meta-value\">", "&nbsp;&nbsp;<span class=\"article-meta-value\">");
        HtmlDocument htmlRoot = Network.GetHtmlDocument(htmlstring);

        if (htmlRoot != null)
        {
          HtmlNode content = null;

          //取得正文
          content = htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"main-content\"]");

          Network.RemoveSubHtmlNode(content, "div", true);
          Network.RemoveSubHtmlNode(content, ".//span", true);
          Network.RemoveSubHtmlNode(content, ".//a", true);
          Network.RemoveSubHtmlNode(content, ".//img", true);

          string tempTextFile =
              content.InnerHtml;

          foreach (var item in typeSetting)
          {
            item.Set(ref tempTextFile);
          }

          _logger.LogDebug(LogMessage.Plugin.SaveFullPath, TaskInfo.SaveFullPath);


          FileWrite.TxtWrire(tempTextFile, TaskInfo.SaveFullPath, TaskInfo.TextEncoding);
          TaskInfo.CurrentSection = 1;
        }


      }
      catch (Exception ex)
      {
        //發生錯誤，當前區塊重取
        _logger.LogError(LogMessage.Plugin.ErrorMessage, ex.ToString());
      }

      TaskInfo.HasStopped = CurrentParameter.IsStop;

      bool finish = TaskInfo.CurrentSection == TaskInfo.EndSection;
      return finish;
    }
  }
}
