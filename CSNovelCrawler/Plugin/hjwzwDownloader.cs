using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Globalization;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
// using CSNovelCrawler.Core;

namespace CSNovelCrawler.Plugin
{
  public class HjwzwDownloader : AbstractDownloader
  {   
    private string str_regex = @"^http(s*):\/\/tw\.*hjwzw.com\/Book(\/Chapter)*(\/Read)*\/(?<TID>\d+)";

    public HjwzwDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
    }

    public HtmlDocument GetHtmlDocument(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlDocument(Network.GetHtmlSource(CurrentParameter, Encoding.UTF8));
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

      TaskInfo.Url = string.Format("https://tw.hjwzw.com/Book/Chapter/{0}", TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);


      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);

      ////取作者跟書名
      var htmlTitle = htmlRoot.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
      r = new Regex(@"(?<Title>\S+)?\/(?<Author>\S+)\/");
      m = r.Match(htmlTitle);
      if (m.Success)
      {
        TaskInfo.Author = m.Groups["Author"].Value.Trim();
        TaskInfo.Title = m.Groups["Title"].Value.Trim();
        TaskInfo.Title = Regex.Replace(TaskInfo.Title, @"[/\|\\\?""\*:><\.]+", "");
        TaskInfo.Title = "《" + TaskInfo.Title + "》";
        _logger.LogDebug(LogMessage.Plugin.Author, TaskInfo.Author);
        _logger.LogDebug(LogMessage.Plugin.Title, TaskInfo.Title);
      }




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
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);
      Regex r = new Regex(@"<a href=\S(\/Book)*(\/Read)*\/\d+,(?<SectionName>\d+)");
      MatchCollection matchs = r.Matches(htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"tbchapterlist\"]").InnerHtml);
      foreach (Match m in matchs)
      {
        int temp = CommonTools.TryParse(m.Groups["SectionName"].Value, 0);
        if (!_sectionNames.Contains(temp))
        {
          _sectionNames.Add(temp);
        }
      }

      //33244會有問題
      //_sectionNames.Sort();
    }

    public override bool Download()
    {
      CurrentParameter.IsStop = false;

      //排版插件
      var typeSetting = new Collection<ITypeSetting>();

      typeSetting.Add(new HtmlDecode());
      typeSetting.Add(new UniformFormat());
      typeSetting.Add(new HjwzwRegex());

      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {

        try
        {
          string url = string.Format("https://tw.hjwzw.com/Book/Read/{0},{1}",
          TaskInfo.Tid,
          SectionNames[TaskInfo.CurrentSection].ToString(CultureInfo.InvariantCulture));//組合網址
                                                                                        //log.Debug("download url=" + url);
          HtmlDocument htmlRoot = GetHtmlDocument(url);

          string tempTextFile = htmlRoot.DocumentNode.SelectSingleNode("//table[7]/tr/td/div[5]").InnerText;
          tempTextFile = tempTextFile.Replace("請記住本站域名: 黃金屋", "").Replace("，歡迎訪問大家讀書院", "");

          foreach (var item in typeSetting)
          {
            item.Set(ref tempTextFile);
          }
          FileWrite.TxtWrire(tempTextFile, TaskInfo.SaveFullPath, TaskInfo.TextEncoding);
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
