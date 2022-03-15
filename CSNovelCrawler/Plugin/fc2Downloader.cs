using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Globalization;
using System.Net;
using System.Threading;
using System.IO;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
// using CSNovelCrawler.Core;

namespace CSNovelCrawler.Plugin
{
  public class fc2Downloader : AbstractDownloader
  {
    private string str_regex = @"^^http(s*)?:\/\/\w*\.blog.fc2.com\/(?<TID>.*)";

    public fc2Downloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
    }



    public HtmlDocument GetHtmlDocument(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlDocument(Network.GetHtmlSource(CurrentParameter, Encoding.UTF8));
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

      //TaskInfo.Url = string.Format("https://www.oldtimescc.cc/go/{0}/", TaskInfo.Tid);
      _logger.LogDebug(LogMessage.Plugin.Url, TaskInfo.Url);


      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);


      ////取作者跟書名
      HtmlNode node_title = htmlRoot.DocumentNode.SelectSingleNode("//title");

      TaskInfo.Title = node_title.InnerText.Trim();
      TaskInfo.Title = Regex.Replace(TaskInfo.Title, @"[/\|\\\?""\*:><\.]+", "");
      foreach (char c in Path.GetInvalidFileNameChars()) // Path.GetInvalidFileNameChars 取得非法字元
      {
        //# 移除非法字元
        TaskInfo.Title = TaskInfo.Title.Replace(c.ToString(), "");
      }

      TaskInfo.Author = "unknown";
      _logger.LogDebug(LogMessage.Plugin.Author, TaskInfo.Author);
      _logger.LogDebug(LogMessage.Plugin.Title, TaskInfo.Title);

      TaskInfo.TotalSection = 1;
      TaskInfo.BeginSection = 1;
      TaskInfo.EndSection = 1;
      TaskInfo.CurrentSection = 0;

      return true;

    }

    public override bool Download()
    {
      CurrentParameter.IsStop = false;

      //排版插件
      var typeSetting = new Collection<ITypeSetting>
                {
                    new BrRegex(),
                    new HtmlDecode(),
                    new UniformFormat()
                };

      string url = TaskInfo.Url;

      // log.Debug("download url=" + url);
      _logger.LogDebug(LogMessage.Plugin.MethodName + LogMessage.Plugin.Url, System.Reflection.MethodBase.GetCurrentMethod().Name, url);

      try
      {
        string htmlstring = GetHtmlString(url);

        HtmlDocument htmlRoot = Network.GetHtmlDocument(htmlstring);

        if (htmlRoot != null)
        {
          HtmlNode content = null;

          if (htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"inner-contents\"]") != null)
          {
            content = htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"inner-contents\"]");
          }
          else if (htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"inner-contents\"]") != null)
          {
            content = htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"inner-contents\"]");
          }
          else if (htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"entry-content clearfix\"]") != null)
          {
            content = htmlRoot.DocumentNode.SelectSingleNode("//*[@class=\"entry-content clearfix\"]");
          }
          else if (htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"inner-contents\"]") != null)
          {
            content = htmlRoot.DocumentNode.SelectSingleNode("//*[@id=\"inner-contents\"]");
          }

          Network.RemoveSubHtmlNode(content, ".//a");
          Network.RemoveSubHtmlNode(content, "hr");
          Network.RemoveSubHtmlNode(content, "script");
          Network.RemoveSubHtmlNode(content, "span", true);
          Network.RemoveSubHtmlNode(content, "div", true);
          Network.RemoveSubHtmlNode(content, "p", true);
          //Network.RemoveSubHtmlNode(content, "//o:p", true);
          //Network.RemoveSubHtmlNode(content, "");

          string tempTextFile = content.InnerHtml;

          foreach (var item in typeSetting)
          {
            item.Set(ref tempTextFile);
          }
          FileWrite.TxtWrire(tempTextFile, TaskInfo.SaveFullPath, TaskInfo.TextEncoding);
          TaskInfo.CurrentSection = 1;
        }


      }
      catch (Exception ex)
      {
        //發生錯誤，當前區塊重取
        _logger.LogError(LogMessage.Plugin.ErrorMessage, ex.ToString());
      }

      TaskInfo.HasStopped = CurrentParameter.IsStop;

      bool finish = TaskInfo.CurrentSection == TaskInfo.EndSection;
      return finish;
    }
  }
}
