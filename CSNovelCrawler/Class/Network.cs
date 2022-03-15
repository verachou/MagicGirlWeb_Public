﻿using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using CSNovelCrawler.Core;
using Microsoft.Extensions.Configuration;
using HtmlAgilityPack;
using System.Text;

namespace CSNovelCrawler.Class
{
  public class Network
  {
    // private static LogManager log = new LogManager(typeof(Network));
    // private static LogManager log;
    // public Network(IConfiguration config)
    // {
    //   log = new LogManager(config, typeof(Network));
    // }
    public static HtmlDocument GetHtmlDocument(string htmlSource)
    {

      var htmlRoot = new HtmlDocument();
      htmlRoot.LoadHtml(htmlSource);

      return htmlRoot;
    }

    public static void RemoveSubHtmlNode(HtmlNode curHtmlNode, string subNodeToRemove)
    {

      RemoveSubHtmlNode(curHtmlNode, subNodeToRemove, false);
    }

    public static void RemoveSubHtmlNode(HtmlNode curHtmlNode, string subNodeToRemove, bool keepContent)
    {

      try
      {
        var foundAllSub = curHtmlNode.SelectNodes(subNodeToRemove);
        if (foundAllSub != null)
        {
          //log.Debug("RemoveSubHtmlNode, tag={0}, size={1}", subNodeToRemove, foundAllSub.Count);
          foreach (HtmlNode subNode in foundAllSub)
          {
            try
            {
              //log.Debug("remove node text = {0}", subNode.InnerText);
              //log.Debug("remove node html = {0}", subNode.InnerHtml);
              subNode.ParentNode.RemoveChild(subNode, keepContent);

              /*
              if (!subNode.HasChildNodes)
              {
                  log.Debug("no child node");
                  subNode.ParentNode.RemoveChild(subNode, keepContent);
                  continue;
              }

              for (var i = subNode.ChildNodes.Count - 1; i >= 0; i--)
              {
                  log.Debug("childNodes count = {0}", i);
                  var child = subNode.ChildNodes[i];
                  subNode.ParentNode.InsertAfter(child, subNode);
              }
              subNode.ParentNode.RemoveChild(subNode);
              */
            }
            catch (Exception ex1)
            {
              //log.Error(ex1.ToString());
            }

          }
        }
      }
      catch (Exception ex)
      {
        //log.Error(ex.ToString());
      }

      //return curHtmlNode;
    }

    public static void RemoveSubHtmlNode(HtmlNode curHtmlNode, string subNodeToRemove, string subNodeToRemove2)
    {

      try
      {
        var foundAllSub = curHtmlNode.SelectNodes(subNodeToRemove);
        if (foundAllSub != null)
        {
          foreach (HtmlNode subNode in foundAllSub)
          {
            RemoveSubHtmlNode(subNode, subNodeToRemove2);
          }
        }
      }
      catch (Exception ex)
      {

        throw;
      }

      //return curHtmlNode;
    }
    /// <summary>
    /// 取得網頁網始碼
    /// </summary>
    /// <param name="para"></param>
    /// <param name="encode"></param>
    /// <returns></returns>
    public static string GetHtmlSource(DownloadParameter para, System.Text.Encoding encode)
    {
      return GetHtmlSource(para, encode, new WebProxy());
    }

    /// <summary>
    /// 取得網頁網始碼
    /// </summary>
    /// <param name="request"></param>
    /// <param name="encode"></param>
    /// <returns></returns>
    public static string GetHtmlSource(HttpWebRequest request, System.Text.Encoding encode)
    {
      string sline = "";
      bool needRedownload = false;
      int remainTimes = 3;
      //log.Debug("GetHtmlSource, line 91");
      do
      {
        try
        {
          //接收 HTTP 回應
          var res = (HttpWebResponse)request.GetResponse();
          //log.Debug("http status = {0}", res.StatusCode);
          var responseStream = res.GetResponseStream();
          if (responseStream == null) throw new Exception();
          StreamReader reader;
          switch (res.ContentEncoding)
          {
            case "gzip":
              //Gzip解壓縮
              using (var gzip = new GZipStream(responseStream, CompressionMode.Decompress))
              {
                reader = new StreamReader(gzip, encode);

                sline = reader.ReadToEnd();

              }
              break;
            case "deflate":
              //deflate解壓縮
              using (var deflate = new DeflateStream(responseStream, CompressionMode.Decompress))
              {
                reader = new StreamReader(deflate, encode);

                sline = reader.ReadToEnd();

              }
              break;
            default:
              reader = new StreamReader(responseStream, encode);

              sline = reader.ReadToEnd();
              break;
          }
          needRedownload = false;
        }
        catch (Exception ex)
        {
        //   log.Error(ex.ToString());
          //重試等待時間
          Thread.Sleep(3000);
          needRedownload = true;

          //重試次數-1
          remainTimes--;
          //如果重試次數小於0，拋出錯誤
          if (remainTimes <= 0)
          {
            needRedownload = false;
            //log.Error(ex.ToString());

          }


        }
      } while (needRedownload);
      return sline;
    }

    /// <summary>
    /// 取得網頁網始碼
    /// </summary>
    /// <param name="para"></param>
    /// <param name="encode"></param>
    /// <param name="proxy"></param>
    /// <returns></returns>
    public static string GetHtmlSource(DownloadParameter para, System.Text.Encoding encode, WebProxy proxy)
    {
      //log.Debug("GetHtmlSource");
      //再來建立你要取得的Request
      var webReq = (HttpWebRequest)WebRequest.Create(para.Url);
      webReq.ContentType = "application/x-www-form-urlencoded";
      webReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
      webReq.Headers.Set("Accept-Language", "zh-TW");
      webReq.UserAgent = para.UserAgent;
      webReq.Headers.Set("Accept-Encoding", "gzip, deflate");
      //webReq.Host = "www09.eyny.com";
      webReq.KeepAlive = true;
      //將剛剛取得的cookie加上去
      webReq.CookieContainer = para.Cookies;
      webReq.Timeout = 30000;
      if (para.Timeout != 0)
      {
        webReq.Timeout = para.Timeout;
      }

      //webReq.Proxy = proxy;
      return GetHtmlSource(webReq, encode);
    }


    /// <summary>
    /// 取得網頁網始碼
    /// </summary>
    /// <param name="para"></param>
    /// <param name="encode"></param>
    /// <returns></returns>
    public static string PostHtmlSource(DownloadParameter para, System.Text.Encoding encode, string formData)
    {

      //再來建立你要取得的Request
      var webReq = (HttpWebRequest)WebRequest.Create(para.Url);
      webReq.ContentType = "application/x-www-form-urlencoded";
      webReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
      webReq.Headers.Set("Accept-Language", "zh-TW,zh;q=0.8,en-US;q=0.6,en;q=0.4");
      webReq.UserAgent = para.UserAgent;
      webReq.Headers.Set("Accept-Encoding", "gzip, deflate");
      webReq.Method = "POST";
      //將剛剛取得的cookie加上去
      webReq.CookieContainer = para.Cookies;
      webReq.Timeout = 30000;
      if (para.Timeout != 0)
      {
        webReq.Timeout = para.Timeout;
      }
      byte[] bs = Encoding.ASCII.GetBytes(formData);
      using (Stream reqStream = webReq.GetRequestStream())
      {
        reqStream.Write(bs, 0, bs.Length);
      }

      return GetHtmlSource(webReq, encode);
    }



  }


  /// <summary>
  /// 下载参数
  /// </summary>
  public class DownloadParameter
  {
    /// <summary>
    /// 资源的网络位置
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// 要创建的本地文件位置
    /// </summary>
    public string FilePath { get; set; }


    /// <summary>
    /// 是否停止下载(可以在下载过程中进行设置，用来控制下载过程的停止)
    /// </summary>
    public bool IsStop { get; set; }

    /// <summary>
    /// 读取或设置发出请求时使用的Cookie
    /// </summary>
    public CookieContainer Cookies { get; set; }

    /// <summary>
    /// 读取或设置下载请求所使用的Referer值
    /// </summary>
    public string Referer { get; set; }
    /// <summary>
    /// 读取或设置下载请求所使用的User-Agent值
    /// </summary>
    public string UserAgent { get; set; }

    public int Timeout { get; set; }
  }
}
