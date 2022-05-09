using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
// using CSNovelCrawler.Core;

namespace CSNovelCrawler.Plugin
{
  public class BiqugeDownloader : AbstractDownloader
  {
    private string str_regex = @"^http(s*):\/\/\w*\.*biqudu.com\/(?<TID>\d+\D+\d+)\/";

    public BiqugeDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
    }



    public HtmlDocument GetHtmlDocument(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlDocument(Network.GetHtmlSource(CurrentParameter, Encoding.GetEncoding("gb2312")));
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

      TaskInfo.Url = string.Format("https://www.biqudu.com/{0}/", TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);


      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);


      ////取作者跟書名
      var htmlNode = htmlRoot.DocumentNode.SelectNodes("//*[@id=\"info\"]");
      if (htmlNode.Count > 0)
      {
        TaskInfo.Title = htmlNode[0].SelectSingleNode("h1").InnerText;
        TaskInfo.Title = OpenCC.ConvertToTW(TaskInfo.Title);
        TaskInfo.Title = new CommonTools().RemoveSpecialChar(TaskInfo.Title);
        r = new Regex(@"者：(?<Author>\S+)");
        m = r.Match(htmlNode[0].SelectSingleNode("p").InnerText);
        if (m.Success)
        {
          TaskInfo.Author = m.Groups["Author"].Value.Trim();
          TaskInfo.Author = OpenCC.ConvertToTW(TaskInfo.Author);
        }

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
      Regex r = new Regex(@"<a href=\S\/\d+_\d+\/(?<SectionName>\d+)\.html\S>");
      MatchCollection matchs = r.Matches(htmlRoot.DocumentNode.SelectSingleNode("//*[@class='listmain']").InnerHtml);
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
      var typeSetting = new Collection<ITypeSetting>
                {  new BrRegex(),
                    new HtmlDecode(),
                    new UniformFormat(),
                    new Traditional()
                };


      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {

        try
        {
          string url = string.Format("https://www.biqudu.com/{0}/{1}.html", TaskInfo.Tid, SectionNames[TaskInfo.CurrentSection].ToString(CultureInfo.InvariantCulture));//組合網址

          HtmlDocument htmlRoot = GetHtmlDocument(url);
          _logger.LogDebug(LogMessage.Plugin.MethodName + LogMessage.Plugin.Url, 
              System.Reflection.MethodBase.GetCurrentMethod().Name, url);


          HtmlNode tempNode = htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"content\"]");
          Network.RemoveSubHtmlNode(tempNode, "script");

          string tempTextFile = htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"bookname\"]/h1").InnerText
              + "\r\n" + tempNode.InnerHtml + "\r\n";



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
