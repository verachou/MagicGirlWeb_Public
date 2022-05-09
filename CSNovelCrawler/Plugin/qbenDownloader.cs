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

namespace CSNovelCrawler.Plugin
{
  public class qbenDownloader : AbstractDownloader
  {
    private string str_regex = @"^http(s*):\/\/\w*\.*big5\.quanben\d?(.io)*(.com)*\/n\/(?<TID>\S+)\/";

    public qbenDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
    }



    public HtmlDocument GetHtmlDocument(string url)
    {
      CurrentParameter.Url = url;
      return Network.GetHtmlDocument(Network.GetHtmlSource(CurrentParameter, Encoding.GetEncoding("UTF-8")));
    }


    public HtmlDocument PostHtmlDocument(string formData)
    {
      CurrentParameter.Url = "http://big5.quanben5.com/index.php?c=book&a=ajax_content";
      return Network.GetHtmlDocument(Network.PostHtmlSource(CurrentParameter, Encoding.GetEncoding("UTF-8"), formData));
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
      }

      TaskInfo.Url = string.Format("http://big5.quanben5.com/n/{0}/", TaskInfo.Tid);



      //用HtmlAgilityPack分析
      HtmlDocument htmlRoot = GetHtmlDocument(TaskInfo.Url);


      ////取作者跟書名
      TaskInfo.Title =
         htmlRoot.DocumentNode.SelectSingleNode("/html/body/div[3]/div/div[2]/div[1]/h3/span")
                  .InnerText;
      TaskInfo.Title = new CommonTools().RemoveSpecialChar(TaskInfo.Title);
      TaskInfo.Title = OpenCC.ConvertToTW(TaskInfo.Title);
      TaskInfo.Author =
          htmlRoot.DocumentNode.SelectSingleNode("/html/body/div[3]/div/div[2]/div[1]/p[1]/span").InnerText;



      TaskInfo.Url = string.Format("http://big5.quanben5.com/n/{0}/xiaoshuo.html", TaskInfo.Tid);
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
      Regex r = new Regex(string.Format(@"<a href=\S\/n\/{0}\/(?<SectionName>\d+)\.html\S", TaskInfo.Tid));
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

      typeSetting.Add(new AnnotationRegex());
      typeSetting.Add(new BrRegex());
      typeSetting.Add(new PRegex());
      typeSetting.Add(new HtmlDecode());
      typeSetting.Add(new UniformFormat());

      for (; TaskInfo.BeginSection <= TaskInfo.EndSection && !CurrentParameter.IsStop; TaskInfo.BeginSection++)
      {
        string url = string.Format("http://big5.quanben5.com/n/{0}/{1}.html",
            TaskInfo.Tid,
            SectionNames[TaskInfo.CurrentSection].ToString(CultureInfo.InvariantCulture));//組合網址

        HtmlDocument htmlRoot = GetHtmlDocument(url);

        try
        {
          string tempTextFile = htmlRoot.DocumentNode.SelectSingleNode("//h1").InnerText
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
