﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Class
{
  public class Downloader
  {


    private Thread mainThread;



    public event EventHandler StateChanged;

    protected virtual void OnStateChanged()
    {
      if (StateChanged != null)
      {
        StateChanged(this, EventArgs.Empty);
      }
    }

    private void SetState(DownloadStatus value)
    {
      _status = value;
      OnStateChanged();
    }

    /// <summary>
    /// 任務的GUID
    /// </summary>
    public Guid TaskId { get; set; }

    public bool Subscribe { get; set; }

    public string GetSubscribe()
    {
      return Subscribe ? "※" : "";
    }
    /// <summary>
    /// 失敗次數
    /// </summary>
    [XmlIgnore]
    public int FailTimes { get; set; }

    private Encoding _textEncoding;

    [XmlIgnore]
    public Encoding TextEncoding
    {
      get { return _textEncoding ?? (_textEncoding = FileWrite.GetFileEncoding(SaveFullPath)); }
      set { _textEncoding = value; }

    }

    /// <summary>
    /// 標題
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// 總頁數
    /// </summary>
    public int TotalPage { get; set; }



    /// <summary>
    /// 文章ID
    /// </summary>
    public string Tid { get; set; }

    /// <summary>
    /// 每一頁的章數
    /// </summary>
    public int PageSection { get; set; }

    /// <summary>
    /// 是否已停止下载(可以在下载过程中进行设置，用来控制下载过程的停止)
    /// </summary>
    public bool HasStopped { get; set; }

    /// <summary>
    /// 總章數
    /// </summary>
    public int TotalSection { get; set; }

    /// <summary>
    /// 起始章數
    /// </summary>
    public int BeginSection { get { return CurrentSection + 1; } set { CurrentSection = value - 1; } }

    /// <summary>
    /// 結束章數
    /// </summary>
    public int EndSection { get; set; }

    /// <summary>
    /// 目前章數
    /// </summary>
    public int CurrentSection { get; set; }

    /// <summary>
    /// 儲存目錄
    /// </summary>
    public string SaveDirectoryName { get; set; }

    /// <summary>
    /// 完整路徑
    /// </summary>
    public string SaveFullPath
    {
      get
      {
        return string.Format("{0}\\{1}.txt", SaveDirectoryName, Title);
      }
    }

    /// <summary>
    /// 任務URL
    /// </summary>
    public string Url { get; set; }

    public string GetDownloadStatus()
    {
      var type = typeof(DownloadStatus);
      var memInfo = type.GetMember(Status.ToString());
      var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute),
          false);
      var description = ((DescriptionAttribute)attributes[0]).Description;
      return description;
    }

    /// <summary>
    /// 關聯的Ui Object
    /// </summary>
    [XmlIgnore]
    public Object UiItem { get; set; }

    /// <summary>
    /// 下載狀態
    /// </summary>
    private DownloadStatus _status { get; set; }
    public DownloadStatus Status { get { return _status; } }

    private IPlugin _basePlugin;
    /// <summary>
    /// IPlugin插件
    /// </summary>
    public IPlugin BasePlugin
    {
      get
      {
        if (_basePlugin == null)
        {
          throw new Exception("Plugin Not Found");
        }
        return _basePlugin;
      }
    }

    private IDownloader _downloader;
    /// <summary>
    /// IDownloader
    /// </summary>
    public IDownloader Downloader2
    {
      get
      {
        if (_downloader == null)
        {
          _downloader = BasePlugin.CreateDownloader();
          // _downloader.TaskInfo = this;
        }
        return _downloader;
      }
    }

    /// <summary>
    /// 釋放IDownloader
    /// </summary>
    public void DisposeDownloader()
    {
      _downloader = null;
    }

    internal double Progress;
    public void SetPlugin(IPlugin plugin)
    {
      _basePlugin = plugin;
    }


    /// <summary>
    /// 任務下載進度
    /// </summary>
    /// <returns></returns>
    public double GetProgress()
    {
      Progress = 0;
      if (TotalSection != 0)
      {
        Progress = CurrentSection / (double)TotalSection;
        if (Progress < 0) Progress = 0.00;
        else if (Progress > 1.00) Progress = 1.00;
      }

      return Progress;
    }


    /// <summary>
    /// 分析任務
    /// </summary>
    public void Analysis()
    {


      mainThread = new Thread(() =>
      {
        try
        {


          SetState(DownloadStatus.TaskAnalysis);

          SetState(Downloader2.Analysis()
                             ? DownloadStatus.AnalysisComplete
                             : DownloadStatus.AnalysisFailed);
        }
        catch (Exception ex)
        {
                //Debug.WriteLine(ex.ToString());
                SetState(DownloadStatus.Error);
        }

      });
      mainThread.Start();
    }

    ///// <summary>
    ///// 開始任務
    ///// </summary>
    //public bool Start()
    //{
    //    //resourceDownloader = BasePlugin.CreateDownloader();
    //    //resourceDownloader.taskInfo = this;
    //    return _downloader.Download();
    //}
    /// <summary>
    /// 開始任務
    /// </summary>
    public void Start()
    {

      mainThread = new Thread(() =>
      {
        try
        {
          SetState(DownloadStatus.Downloading);
          SetState(Downloader2.Download() ? DownloadStatus.DownloadComplete : DownloadStatus.Downloadblocked);
        }
        catch (Exception ex)
        {
                //Debug.WriteLine(ex.ToString());
                SetState(DownloadStatus.Error);
        }
      });
      mainThread.Start();


      //return _downloader.Download();
    }

    /// <summary>
    /// 停止任務
    /// </summary>
    public void Stop()
    {
      if (_downloader != null)
        _downloader.StopDownload();
      var t = new Thread(() =>
      {
              //等待時間 (10秒)
              int timeout = 10000;
              //等待停止
              while (Status == DownloadStatus.Stopping)
        {
          Thread.Sleep(500);
          timeout -= 500;
          if (timeout < 0 || HasStopped) //如果到时仍未停止
                {
            break;
          }
        }

        mainThread.Abort();
        mainThread = null;
        SetState(DownloadStatus.TaskPause);

      })
      { IsBackground = true };
      t.Start();


      //if (Status == DownloadStatus.Downloading)
      //{
      //    SetState(DownloadStatus.Stopping);

      //    mainThread.Abort();
      //    mainThread = null;
      //    SetState(DownloadStatus.TaskPause);
      //}


    }
  }
}
