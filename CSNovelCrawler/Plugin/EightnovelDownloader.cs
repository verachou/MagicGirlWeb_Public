using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
// using CSNovelCrawler.Core;

namespace CSNovelCrawler.Plugin
{
  public class EightnovelDownloader : AbstractDownloader
  {
    private string str_regex = @"^http(s*):\/\/8book.com(\/readbook\/\d+\/)*(\/books\/novelbook_)*(?<TID>\d+).*";

    public EightnovelDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
    }

    public HtmlDocument GetHtmlDocument(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlDocument(Network.GetHtmlSource(CurrentParameter, Encoding.GetEncoding("BIG5")));
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

      TaskInfo.Url = string.Format("https://8book.com/books/novelbook_{0}.html", TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);


      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);

      ////取作者跟書名
      TaskInfo.Title = htmlRoot.DocumentNode.SelectSingleNode("/body/table[2]/tr[2]/td[3]/table[2]/tr/td[2]/table/tr[1]/td[2]/font[1]").InnerText;
      TaskInfo.Title = "《" + TaskInfo.Title + "》";
      TaskInfo.Title = Regex.Replace(TaskInfo.Title, @"[/\|\\\?""\*:><\.]+", "");
      TaskInfo.Author = htmlRoot.DocumentNode.SelectSingleNode("/body/table[2]/tr[2]/td[3]/table[2]/tr/td[2]/table/tr[2]/td/font[1]").InnerText;

      _logger.LogDebug("before get section names");
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
          _logger.LogDebug("before GetTotalSection()");
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
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);
      _logger.LogDebug(LogMessage.Plugin.MethodName + LogMessage.Plugin.Url,
              System.Reflection.MethodBase.GetCurrentMethod().Name, TaskInfo.Url);

      Regex r = new Regex(@"<a href=\S(\/readbook)*\/(?<SectionName>\d+\/\d+\/\d+)");
      HtmlNodeCollection tempNode2 = htmlRoot.DocumentNode.SelectNodes("//*[@class=\"episodelist\"]");
      string tempList = "";
      foreach (HtmlNode i in tempNode2)
      {
        tempList += i.InnerHtml;
      }

      MatchCollection matchs = r.Matches(tempList);
      foreach (Match m in matchs)
      {
        string temp = m.Groups["SectionName"].Value.Trim();
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

      typeSetting.Add(new BrRegex());
      typeSetting.Add(new HtmlDecode());
      typeSetting.Add(new UniformFormat());

      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {

        try
        {
          string url = string.Format("https://8book.com/readbook/{0}.html",
          SectionNames[TaskInfo.CurrentSection].ToString(CultureInfo.InvariantCulture));//組合網址

          HtmlDocument htmlRoot = GetHtmlDocument(url);
          HtmlNode temptitle = htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"contenttitle\"]");
          HtmlNode tempbody = htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"content\"]");
          Network.RemoveSubHtmlNode(tempbody, "table");
          Network.RemoveSubHtmlNode(tempbody, "p");

          string tempTextFile = temptitle.InnerHtml
              + "\r\n" + tempbody.InnerHtml + "\r\n";

          foreach (var item in typeSetting)
          {
            item.Set(ref tempTextFile);
          }
          FileWrite.TxtWrire(tempTextFile, TaskInfo.SaveFullPath, TaskInfo.TextEncoding);


        }
        catch (Exception ex)
        {
          //發生錯誤，當前區塊重取
          _logger.LogError(ex.ToString());
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

