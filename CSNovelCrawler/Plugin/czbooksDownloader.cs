using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Globalization;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Microsoft.Extensions.Logging;

using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;


namespace CSNovelCrawler.Plugin
{
  public class czbooksDownloader : AbstractDownloader
  {
    private string str_regex = @"^https?:\/\/czbooks.net\/n\/(?<TID>[a-z0-9]*)";

    public czbooksDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
    }



    public HtmlDocument GetHtmlDocument(string url)
    {
      return Network.GetHtmlDocument(GetHtmlString(url));
    }


    public string GetHtmlString(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlSource(CurrentParameter, Encoding.UTF8);
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

      TaskInfo.Url = string.Format("https://czbooks.net/n/{0}", TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);


      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);

      ////取作者跟書名
      TaskInfo.Title = htmlRoot.DocumentNode.SelectSingleNode("//span[@class='title']").InnerText.Trim();
      TaskInfo.Title = new CommonTools().RemoveSpecialChar(TaskInfo.Title);

      TaskInfo.Author = htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"author\"]/a").InnerText.Trim();
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
      //log.Debug("GetTotalSection");

      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);
      Regex r = new Regex(string.Format(@"<li><a href=""\/\/czbooks.net\/n\/{0}(\/)*(?<SectionName>[a-zA-Z0-9?=]*)"">.+?<\/a><\/li>", TaskInfo.Tid));
      MatchCollection matchs = r.Matches(htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"nav chapter-list\"]").InnerHtml);
      foreach (Match m in matchs)
      {
        string temp = m.Groups["SectionName"].Value;
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

      // typeSetting.Add(new AnnotationRegex());
      typeSetting.Add(new Remove0007());
      typeSetting.Add(new BrRegex());
      // typeSetting.Add(new PRegex());
      typeSetting.Add(new HtmlDecode());
      typeSetting.Add(new UniformFormat());


      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {
        string url = string.Format("https://czbooks.net/n/{0}/{1}",
            TaskInfo.Tid,
            SectionNames[TaskInfo.CurrentSection].ToString(CultureInfo.InvariantCulture));//組合網址
                                                                                          //log.Debug("download url=" + url);

        try
        {
          string htmlstring = GetHtmlString(url);

          HtmlDocument htmlRoot = Network.GetHtmlDocument(htmlstring);

          if (htmlRoot != null)
          {
            string chaptername = htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"name\"]").InnerText.Trim();
            string tempTextFile =
                chaptername + "\r\n"
                + htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"content\"]").InnerText;

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
