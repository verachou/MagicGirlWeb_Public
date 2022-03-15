using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Globalization;
using System.Net;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
// using CSNovelCrawler.Core;

namespace CSNovelCrawler.Plugin
{
  public class wfxsDownloader : AbstractDownloader
  {
    private string str_regex = @"^https?:\/\/\w*\.*wfxs\d?.org\/(html|book)\/(?<TID>\d+)";

    public wfxsDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
    }

    public HtmlDocument GetHtmlDocument(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlDocument(Network.GetHtmlSource(CurrentParameter, Encoding.GetEncoding("big5")));
    }


    public string GetHtmlString(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlSource(CurrentParameter, Encoding.GetEncoding("big5"));
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

      TaskInfo.Url = string.Format("https://www.wfxs.org/html/{0}/", TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);


      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(string.Format("https://www.wfxs.org/book/{0}.html", TaskInfo.Tid));


      ////取作者跟書名
      TaskInfo.Title =
         htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"content\"]/div[2]/div[1]/h1/a[2]")
                  .InnerText.Trim();
      TaskInfo.Title = Regex.Replace(TaskInfo.Title, @"[/\|\\\?""\*:><\.]+", "");
      TaskInfo.Title = "《" + TaskInfo.Title + "》";

      TaskInfo.Author =
          htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"content\"]/div[2]/div[2]/table/tr/td/table/tbody/tr[1]/td/table/tr[2]/td[2]/table/tr[2]/td[6]")
          .InnerText.Trim();
      _logger.LogDebug(LogMessage.Plugin.Author, TaskInfo.Author);
      _logger.LogDebug(LogMessage.Plugin.Title, TaskInfo.Title);

      TaskInfo.TotalSection = SectionNames.Count;

      if (TaskInfo.BeginSection == 0)
      {
        TaskInfo.BeginSection = 1;
      }
      if (TaskInfo.EndSection == 0)
      {
        TaskInfo.EndSection = TaskInfo.TotalSection;
      }

      return true;
    }

    private List<int> _sectionNames;

    private List<int> SectionNames
    {
      get
      {
        if (_sectionNames == null || _sectionNames.Count == 0)
        {
          _sectionNames = new List<int>();
          GetTotalSection();

        }
        return _sectionNames;

      }
    }

    /// <summary>
    /// 取目錄
    /// </summary>
    public void GetTotalSection()
    {

      HtmlDocument htmlRoot = GetHtmlDocument(string.Format("https://www.wfxs.org/html/{0}/", TaskInfo.Tid));
      Regex r = new Regex(string.Format(@"<dd><a href=""\/html\/{0}\/(?<SectionName>\d+)\.html\"">.+?<\/a><\/dd>", TaskInfo.Tid));
      MatchCollection matchs = r.Matches(htmlRoot.DocumentNode.InnerHtml);
      foreach (Match m in matchs)
      {
        int temp = CommonTools.TryParse(m.Groups["SectionName"].Value, 0);
        if (!_sectionNames.Contains(temp))
        {
          _sectionNames.Add(temp);
        }
      }
    }

    public override bool Download()
    {
      CurrentParameter.IsStop = false;

      //排版插件
      var typeSetting = new Collection<ITypeSetting>();
      typeSetting.Add(new Remove0007());
      typeSetting.Add(new BrRegex());
      typeSetting.Add(new HtmlDecode());
      typeSetting.Add(new UniformFormat());

      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {
        string url = string.Format("https://www.wfxs.org/html/{0}/{1}.html",
            TaskInfo.Tid,
            SectionNames[TaskInfo.CurrentSection].ToString(CultureInfo.InvariantCulture));//組合網址
                                                                                          //log.Debug("download url=" + url);

        try
        {
          string htmlstring = GetHtmlString(url);
          //log.Debug("html string = {0}", htmlstring);

          string content = "";
          string chaptername = "";
          Regex r = new Regex(@"<H1><a href=.\/html\/\d+\/.*<\/a>(?<chaptername>.+?)<\/H1>.+?<\/script>.+?<\/script>(?<content>.+?)<script", RegexOptions.Singleline);
          Match m = r.Match(htmlstring);
          if (m.Success)
          {
            content = m.Groups["content"].Value;
            chaptername = m.Groups["chaptername"].Value;

            //log.Debug("content string = {0}", content);
            //log.Debug("chaptername string = {0}", chaptername);
          }
          else
          {
            // log.Debug("content and chaptername is empty");
            _logger.LogDebug(LogMessage.Plugin.ContentIsEmptyMessage);


          }

          HtmlDocument htmlRoot = Network.GetHtmlDocument(content);

          var node = htmlRoot.DocumentNode;
          Network.RemoveSubHtmlNode(node, "div");

          //log.Debug("node=" + node.InnerHtml);

          string tempTextFile = chaptername + "\r\n" + node.InnerHtml + "\r\n";
          foreach (var item in typeSetting)
          {
            item.Set(ref tempTextFile);
          }
          FileWrite.TxtWrire(tempTextFile, TaskInfo.SaveFullPath, TaskInfo.TextEncoding);
        }
        catch (Exception ex)
        {
          //CoreManager.LoggingManager.Debug(ex.ToString());
          //發生錯誤，當前區塊重取
          // log.Error(ex.ToString());
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
