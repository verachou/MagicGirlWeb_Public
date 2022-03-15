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
//using CSNovelCrawler.Core;

namespace CSNovelCrawler.Plugin
{
  public class ggdDownloader : AbstractDownloader
  {
    private string str_regex = @"^http(s*)?:\/\/\w*\.52ggd.com(\/book(s*)(\/\d{1,3})*)*(\/xs(-1)*)*\/(?<TID>\d{1,7})";

    public ggdDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
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

      TaskInfo.Url = string.Format("http://www.52ggd.com/books/{0}.html", TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);


      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);


      ////取作者跟書名
      TaskInfo.Title =
         htmlRoot.DocumentNode.SelectSingleNode("//h1").InnerText.Trim();
      TaskInfo.Title = Regex.Replace(TaskInfo.Title, @"[/\|\\\?""\*:><\.]+", "");
      TaskInfo.Title = "《" + TaskInfo.Title + "》";
      TaskInfo.Title = OpenCC.ConvertToTW(TaskInfo.Title);
      TaskInfo.Author =
          htmlRoot.DocumentNode.SelectSingleNode("//*[@name=\"copyright\"]").Attributes["content"].Value.Trim();
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
      //先從首頁找到目錄位置
      HtmlDocument htmlRoot = GetHtmlDocument(string.Format("http://www.52ggd.com/books/{0}.html", TaskInfo.Tid));
      //再存目錄位置找到各章節位置
      string index_url = htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"book-link\"]/a[2]").Attributes["href"].Value.Trim();
      //http://www.52ggd.com/book/38/38614/index.html
      //log.Debug("Index URL=" + index_url);
      htmlRoot = GetHtmlDocument(index_url);

      string base_url = index_url.Substring(0,
          index_url.IndexOf(string.Format("{0}/index.html", TaskInfo.Tid)));
      //log.Debug("Base URL=" + base_url);

      Regex r = new Regex(@"<dd><a href=""(?<SectionName>\d+)\.html"">.+?<\/a><\/dd>");
      MatchCollection matchs = r.Matches(htmlRoot.DocumentNode.InnerHtml);
      foreach (Match m in matchs)
      {
        int temp = CommonTools.TryParse(m.Groups["SectionName"].Value, 0);
        //log.Debug("[temp={0}][SectionName={1}]", temp, m.Groups["SectionName"].Value);
        //http://www.52ggd.com/book/38/<TID>/<SectionName>.html
        string temp_url = string.Format(@"http://www.52ggd.com/book/38/{0}/{1}.html", TaskInfo.Tid, temp);
        //log.Debug("Temp URL=" + temp_url);
        _sectionNames.Add(temp_url);

      }
      //_sectionNames.Sort();
    }

    public override bool Download()
    {
      CurrentParameter.IsStop = false;

      //排版插件
      var typeSetting = new Collection<ITypeSetting>
                {
                    new BrRegex(),
                    new HtmlDecode(),
                    new UniformFormat(),
                    new Traditional()
                };

      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {
        string url = SectionNames[TaskInfo.CurrentSection].ToString(CultureInfo.InvariantCulture);
        //log.Debug("download url=" + url);

        try
        {
          string htmlstring = GetHtmlString(url);
          //log.Debug("html string = {0}", htmlstring);

          HtmlDocument htmlRoot = Network.GetHtmlDocument(htmlstring);

          if (htmlRoot != null)
          {
            string chaptername = htmlRoot.DocumentNode.SelectSingleNode("//h1").InnerText;
            string tempTextFile =
                chaptername + "\r\n"
                + htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"BookText\"]").InnerHtml;

            //log.Debug("tempTextFile = {0}", tempTextFile);

            foreach (var item in typeSetting)
            {
              item.Set(ref tempTextFile);
            }
            FileWrite.TxtWrire(tempTextFile, TaskInfo.SaveFullPath, TaskInfo.TextEncoding);
          }


        }
        catch (Exception ex)
        {
          //CoreManager.LoggingManager.Debug(ex.ToString());
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
