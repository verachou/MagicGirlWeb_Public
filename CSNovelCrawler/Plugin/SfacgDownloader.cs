using System;
using System.Collections.Generic;
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

  public class SfacgDownloader : AbstractDownloader
  {
    private string str_regex = @"^http:\/\/\.*book\.sfacg\.com\/Novel\/(?<TID>\d+)";

    public SfacgDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
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

      TaskInfo.Url = string.Format("http://book.sfacg.com/Novel/{0}/MainIndex/", TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);

      string introductionUrl = string.Format("http://book.sfacg.com/Novel/{0}", TaskInfo.Tid);

      //用HtmlAgilityPack分析
      HtmlDocument introductionHtml = GetHtmlDocument(introductionUrl);


      ////取作者跟書名
      TaskInfo.Title =
         introductionHtml.DocumentNode.SelectSingleNode("//*[@class=\"title\"]/span").InnerText;
      TaskInfo.Title = new CommonTools().RemoveSpecialChar(TaskInfo.Title);
      TaskInfo.Title = OpenCC.ConvertToTW(TaskInfo.Title);
      TaskInfo.Author =
          introductionHtml.DocumentNode.SelectSingleNode(
              "//*[@class=\"author-name\"]/span").InnerText;
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
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);
      Regex r = new Regex(string.Format(@"<a href=\S\/Novel\/{0}(?<SectionName>\/\d+\/\d+)\S", TaskInfo.Tid));


      MatchCollection matchs = r.Matches(htmlRoot.DocumentNode.InnerHtml);
      foreach (Match m in matchs)
      {
        if (!_sectionNames.Contains(m.Groups["SectionName"].Value))
        {
          _sectionNames.Add(m.Groups["SectionName"].Value);
        }
      }      
    }


    public override bool Download()
    {
      CurrentParameter.IsStop = false;

      string urlHead = string.Format("http://book.sfacg.com/Novel/{0}", TaskInfo.Tid);
      //string urlTail = ".html?charset=big5";

      //排版插件
      var typeSetting = new Collection<ITypeSetting>();    
      typeSetting.Add(new SfacgToIndent());
      typeSetting.Add(new HtmlDecode());
      typeSetting.Add(new UniformFormat());
      typeSetting.Add(new Traditional());



      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {
        string url = urlHead + SectionNames[TaskInfo.CurrentSection].ToString(CultureInfo.InvariantCulture);//組合網址
                                                                                                            //log.Debug("download url=" + url);

        HtmlDocument htmlRoot = GetHtmlDocument(url);


        try
        {
          var nodeTitle =
              htmlRoot.DocumentNode.SelectSingleNode(@"//*[@class=""article-title""]");
          var nodeBody =
              htmlRoot.DocumentNode.SelectSingleNode(@"//*[@id=""ChapterBody""]");
          Network.RemoveSubHtmlNode(nodeBody, "img");
          string tempTextFile = nodeTitle.InnerHtml + "\r\n" + nodeBody.InnerHtml;

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
