using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
// using CSNovelCrawler.Core;


namespace CSNovelCrawler.Plugin
{
  internal class stoDownloader : AbstractDownloader
  {
    private string str_regex = @"^https?:\/\/\w*\.sto.cx\/*([a-z]+)-*(?<TID>\d{3,7})";

    public stoDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
      CurrentParameter.Timeout = 10000;
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
      //取TID
      Regex r = new Regex(str_regex);
      Match m = r.Match(TaskInfo.Url);
      if (m.Success)
      {
        TaskInfo.Tid = m.Groups["TID"].Value;
        _logger.LogDebug(LogMessage.Plugin.Tid, TaskInfo.Tid);
      }

      TaskInfo.Url = string.Format("https://www.sto.cx/book-{0}-1.html", TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);

      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);

      ////取作者跟書名
      string htmlTitle = htmlRoot.DocumentNode.SelectSingleNode("//h1").InnerText;
      r = new Regex(@"(?<Title>(.(?!【))+)[^\u4e00-\u9fa5a-zA-Z0-9]*作\s*者[^\u4e00-\u9fa5a-zA-Z0-9]*(?<Author>[\u0800-\u9fa5\x3130-\x318Fa-zA-Z0-9]+)");
      m = r.Match(htmlTitle);
      if (m.Success)
      {
        TaskInfo.Author = m.Groups["Author"].Value.Trim();
        TaskInfo.Author = OpenCC.ConvertToTW(TaskInfo.Author);
        TaskInfo.Title = m.Groups["Title"].Value.Trim();
        TaskInfo.Title = Regex.Replace(TaskInfo.Title, @"[/\|\\\?""\*:><\.]+", "");
        TaskInfo.Title = OpenCC.ConvertToTW(TaskInfo.Title);
        _logger.LogDebug(LogMessage.Plugin.Author, TaskInfo.Author);
        _logger.LogDebug(LogMessage.Plugin.Title, TaskInfo.Title);
      }


      //取總頁數
      string nodeHeaders2 = htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"webPage\"]").InnerHtml;
      
      if (nodeHeaders2 != null)
      {
        r = new Regex(TaskInfo.Tid + @"-(?<TotalPage>\d{1,4})");
        MatchCollection matchs = r.Matches(nodeHeaders2);
        foreach (Match ma in matchs)
        {
          int tmpTotalPage = CommonTools.TryParse(ma.Groups["TotalPage"].Value, 0);
          if (tmpTotalPage > TaskInfo.TotalSection)
          {
            TaskInfo.TotalSection = tmpTotalPage;
          }
        }
      }

      if (TaskInfo.BeginSection <= 1)
      { TaskInfo.BeginSection = 1; }
      if (TaskInfo.EndSection == 0)
      { TaskInfo.EndSection = TaskInfo.TotalSection; }

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
      typeSetting.Add(new HtmlDecode());
      typeSetting.Add(new UniformFormat());
      typeSetting.Add(new stoRegex());
      typeSetting.Add(new u8hexRegex());
      typeSetting.Add(new Traditional());

      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {
        string url = string.Format("https://www.sto.cx/book-{0}-{1}.html",
            TaskInfo.Tid,
            TaskInfo.CurrentSection + 1);//組合網址
                                         //log.Debug("download url=" + url);

        try
        {
          string htmlstring = GetHtmlString(url);

          HtmlDocument htmlRoot = Network.GetHtmlDocument(htmlstring);

          if (htmlRoot != null)
          {
            HtmlNode tempNode = htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"BookContent\"]");
            Network.RemoveSubHtmlNode(tempNode, "script");
            Network.RemoveSubHtmlNode(tempNode, "span");
            Network.RemoveSubHtmlNode(tempNode, "div");
            string tempTextFile = tempNode.InnerHtml + "\r\n";

            foreach (var item in typeSetting)
            {
              item.Set(ref tempTextFile);
            }
            FileWrite.TxtWrire(tempTextFile, TaskInfo.SaveFullPath, TaskInfo.TextEncoding);
          }


        }
        catch (Exception ex)
        {
          //發生錯誤，當前區塊重取
          _logger.LogError(LogMessage.Plugin.ErrorMessage, ex.ToString());
          TaskInfo.BeginSection--;
          TaskInfo.FailTimes++;

          continue;
        }

        TaskInfo.HasStopped = CurrentParameter.IsStop;
      }

      bool finish = TaskInfo.CurrentSection == TaskInfo.EndSection;
      return finish;
    }
  }
}
