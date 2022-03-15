using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Globalization;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;

namespace CSNovelCrawler.Plugin
{
  public class oldtimesDownloader : AbstractDownloader
  {
    private string str_regex = @"^http(s*)?:\/\/\w*\.oldtimescc.cc\/go\/(?<TID>\d{1,7})";

    public oldtimesDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
    }



    public HtmlDocument GetHtmlDocument(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlDocument(Network.GetHtmlSource(CurrentParameter, Encoding.GetEncoding("GBK")));
    }


    public string GetHtmlString(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlSource(CurrentParameter, Encoding.GetEncoding("GBK"));
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

      TaskInfo.Url = string.Format("https://www.oldtimescc.cc/go/{0}/", TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);


      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);


      ////取作者跟書名
      TaskInfo.Title =
         htmlRoot.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]").Attributes["content"].Value.Trim();
      TaskInfo.Title = OpenCC.ConvertToTW(TaskInfo.Title);
      TaskInfo.Title = Regex.Replace(TaskInfo.Title, @"[/\|\\\?""\*:><\.]+", "");
      TaskInfo.Title = "《" + TaskInfo.Title + "》";

      TaskInfo.Author =
          htmlRoot.DocumentNode.SelectSingleNode("//*[@property=\"og:novel:author\"]").Attributes["content"].Value.Trim();
      TaskInfo.Author = OpenCC.ConvertToTW(TaskInfo.Author);
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

    private List<string> _sectionNames;

    private List<string> SectionNames
    {
      get
      {
        if (_sectionNames == null || _sectionNames.Count == 0)
        {
          _sectionNames = new List<string>();
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
      HtmlDocument htmlRoot = GetHtmlDocument(string.Format("https://www.oldtimescc.cc/go/{0}/", TaskInfo.Tid));

      HtmlNode node = htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"list-chapterAll\"]");

      Regex r = new Regex(@"<dd><a href=""(?<SectionName>\d+)\.html"".+?>.+?<\/a><\/dd>");
      MatchCollection matchs = r.Matches(node.InnerHtml);
      foreach (Match m in matchs)
      {
        int temp = CommonTools.TryParse(m.Groups["SectionName"].Value, 0);
        string temp_url = string.Format(@"https://www.oldtimescc.cc/go/{0}/{1}.html", TaskInfo.Tid, temp);
        _sectionNames.Add(temp_url);

      }
    }

    public override bool Download()
    {
      CurrentParameter.IsStop = false;

      //排版插件
      var typeSetting = new Collection<ITypeSetting>();
      typeSetting.Add(new BrRegex());
      typeSetting.Add(new HtmlDecode());
      typeSetting.Add(new UniformFormat());
      typeSetting.Add(new Traditional());

      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {
        string url = SectionNames[TaskInfo.CurrentSection].ToString(CultureInfo.InvariantCulture);

        try
        {
          string htmlstring = GetHtmlString(url);

          HtmlDocument htmlRoot = Network.GetHtmlDocument(htmlstring);

          if (htmlRoot != null)
          {
            string chaptername = htmlRoot.DocumentNode.SelectSingleNode("//h1").InnerText;
            HtmlNode content = htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"readcontent\"]");
            Network.RemoveSubHtmlNode(content, "div");
            Network.RemoveSubHtmlNode(content, "p");

            string tempTextFile =
                chaptername + "\r\n"
                + content.InnerHtml;

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
        Thread.Sleep(1500);
      }

      bool finish = TaskInfo.CurrentSection == TaskInfo.EndSection;
      return finish;
    }
  }
}
