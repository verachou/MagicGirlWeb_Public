using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;

namespace CSNovelCrawler.Plugin
{
  public class eightcomicDownloader : AbstractDownloader
  {
    public eightcomicDownloader(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
    }

    private List<string> ImgUrl;
    public override bool Analysis()
    {
      string ch = string.Empty;
      Regex r = new Regex("^http:\\/\\/new\\.comicvip\\.com\\/show\\/(?<TID>\\D.*)\\.\\S*\\?ch=(?<ch>\\d*)");
      Match m = r.Match(TaskInfo.Url);
      if (m.Success)
      {
        TaskInfo.Tid = m.Groups["TID"].Value;
        ch = m.Groups["ch"].Value;
      }
      try
      {
        CurrentParameter.Url = TaskInfo.Url;

        string sHTML_CODE = Network.GetHtmlSource(CurrentParameter, Encoding.GetEncoding("BIG5"));
        r = new Regex("<title>(?<title>\\S*).*<\\/title>");
        m = r.Match(sHTML_CODE);
        if (m.Success)
        {
          TaskInfo.Title = m.Groups["title"].Value;
        }

        string itemid = string.Empty;
        string chs = string.Empty;
        string allcodes = string.Empty;
        r = new Regex("var\\schs=(?<chs>\\d*);var\\sitemid=(?<itemid>\\d*);var\\sallcodes=\"(?<allcodes>.*)\";");
        m = r.Match(sHTML_CODE);
        if (m.Success)
        {
          itemid = m.Groups["itemid"].Value;
          chs = m.Groups["chs"].Value;
          allcodes = m.Groups["allcodes"].Value;
          ImgUrl = getImgUrl(itemid, chs, allcodes, ch);
          if (ImgUrl.Count != 0)
          {
            TaskInfo.TotalSection = ImgUrl.Count;
            TaskInfo.BeginSection = 1;
            TaskInfo.CurrentSection = 1;
            TaskInfo.EndSection = TaskInfo.TotalSection;
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }

      return false;
    }

    private List<string> getImgUrl(string itemid, string chs, string allcodes, string ch)
    {
      List<string> ImgUrl = new List<string>();
      string[] Codes = allcodes.Split('|');
      string[] Code = new string[5];
      foreach (string i in Codes)
      {
        if (i.IndexOf(ch + " ") == 0)
        {
          Code = i.Split(' ');
          break;
        }
      }
      for (int i = 1; i <= int.Parse(Code[3]); i++)
      {
        int idx = (((i - 1) / 10) % 10) + (((i - 1) % 10) * 3);
        ImgUrl.Add("http://img" + Code[1] + ".8comic.com/" + Code[2] + "/" + itemid + "/" + Code[0] + "/" + i.ToString("000") + "_" + Code[4].Substring(idx, 3) + ".jpg");
      }
      return ImgUrl;
    }

    public override bool Download()
    {
      try
      {
        for (; TaskInfo.CurrentSection <= TaskInfo.TotalSection; TaskInfo.CurrentSection++)
        {
          string Url = ImgUrl[TaskInfo.CurrentSection - 1];
          CurrentParameter.Url = Url;


          string sHTML_CODE = Network.GetHtmlSource(CurrentParameter, Encoding.Default);
        }
      }
      catch
      {
        return false;
      }
      return true;
    }
    public void StopDownload()
    {
      if (CurrentParameter != null)
      {
        //將停止旗標設為true
        CurrentParameter.IsStop = true;
      }
    }
  }
}
