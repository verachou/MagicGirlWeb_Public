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
  public class RanwenDownloader : AbstractDownloader
  {
    private string str_regex = @"(^http(s*):\/\/\w*\.*ranwena.com(\/files)*(\/article)*\/\d+\/(?<TID>\d+)\/)";

    public RanwenDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
    }

    public HtmlDocument GetHtmlDocument(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlDocument(Network.GetHtmlSource(CurrentParameter, Encoding.GetEncoding("GBK")));
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

      TaskInfo.Url = string.Format("https://www.ranwena.com/files/article/{0}/{1}", (CommonTools.TryParse(TaskInfo.Tid, 0) / 1000).ToString(CultureInfo.InvariantCulture), TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);



      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);


      ////取作者跟書名
      HtmlNode titlenode = htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"info\"]/h1");
      TaskInfo.Title = titlenode.InnerText;
      TaskInfo.Title = new CommonTools().RemoveSpecialChar(TaskInfo.Title);
      TaskInfo.Title = OpenCC.ConvertToTW(TaskInfo.Title);
      TaskInfo.Author =
          htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"info\"]/p[1]")
          .InnerText.Split('：')[1];
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
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);
      Regex r = new Regex(@"<a href=\S+\/(?<SectionName>\d+)\.html\S");
      string str_chapterlist = htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"list\"]").InnerHtml;
      MatchCollection matchs = r.Matches(str_chapterlist);
      _logger.LogDebug(LogMessage.Plugin.MatchCount, matchs.Count);
      foreach (Match m in matchs)
      {
        int temp = CommonTools.TryParse(m.Groups["SectionName"].Value, 0);
        if (!_sectionNames.Contains(temp))
        {
          _sectionNames.Add(temp);
        }
      }
      _sectionNames.Sort();
    }

    public override bool Download()
    {
      CurrentParameter.IsStop = false;

      //排版插件
      var typeSetting = new Collection<ITypeSetting>();
      typeSetting.Add(new BrRegex());
      typeSetting.Add(new PRegex());
      typeSetting.Add(new HtmlDecode());
      typeSetting.Add(new UniformFormat());
      typeSetting.Add(new Traditional());

      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {
        string url = string.Format("https://www.ranwena.com/files/article/{0}/{1}/{2}.html",
            (CommonTools.TryParse(TaskInfo.Tid, 0) / 1000).ToString(CultureInfo.InvariantCulture),
            TaskInfo.Tid,
            SectionNames[TaskInfo.CurrentSection].ToString(CultureInfo.InvariantCulture));//組合網址
                                                                                          //log.Debug("download url=" + url);

        HtmlDocument htmlRoot = GetHtmlDocument(url);

        try
        {

          string tempTextFile = htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"bookname\"]/h1").InnerText
          + "\r\n";

          var node = htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"content\"]");
          Network.RemoveSubHtmlNode(node, "div");

          tempTextFile += node.InnerHtml + "\r\n";
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
