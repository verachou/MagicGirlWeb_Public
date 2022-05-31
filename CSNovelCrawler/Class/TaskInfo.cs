﻿using System;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using CSNovelCrawler.Interface;
using System.ComponentModel;


namespace CSNovelCrawler.Class
{
  public class TaskInfo
  {
    public TaskInfo(string url, Encoding textEncoding, IPlugin plugin, string saveDirectoryName, DownloadStatus status)
    {
      Url = url;
      _textEncoding = textEncoding;
      _basePlugin = plugin;
      SaveDirectoryName = saveDirectoryName;
      Status = status;

      Title = "Unknown";
      CustomFileName = "Unknown";
      Author = "Unknown";
      Progress = 0;
      TotalSection = 0;
      TaskId = Guid.NewGuid();
      Subscribe = false;
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
    /// 總下載頁數
    /// </summary>
    public int TotalDownloadPage { get; set; }

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
        return string.Format("{0}\\{1}.txt", SaveDirectoryName, CustomFileName);
      }
    }

    public string CustomFileName { get; set; }
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
    public DownloadStatus Status { get; set; }

    private IPlugin _basePlugin;
    /// <summary>
    /// IPlugin插件
    /// </summary>
    public IPlugin BasePlugin { get { return _basePlugin; } }

    private IDownloader _downloader;
    /// <summary>
    /// IDownloader
    /// </summary>
    public IDownloader Downloader { get { return _downloader; } }

    /// <summary>
    /// 釋放IDownloader
    /// </summary>
    public void DisposeDownloader()
    {
      _downloader = null;
    }

    internal double Progress;

    /// <summary>
    /// 任務下載進度
    /// </summary>
    /// <returns></returns>
    public double GetProgress()
    {
      Progress = 0;
      if (TotalSection != 0 && TotalDownloadPage != 0)
      {
        // Progress = CurrentSection / (double)TotalSection;
        Progress = Math.Round(1.00 - (double)(EndSection - CurrentSection) / TotalDownloadPage, 2);

        if (Progress < 0) Progress = 0.00;
        else if (Progress > 1.00) Progress = 1.00;
      }

      return Progress;
    }

    /// <summary>
    /// 分析任務
    /// </summary>
    public bool Analysis()
    {
      if (BasePlugin == null)
      {
        Status = DownloadStatus.Error;
        throw new Exception("Plugin Not Found");
      }
      if (_downloader == null)
      {
        _downloader = BasePlugin.CreateDownloader();
        _downloader.TaskInfo = this;
      }

      return _downloader.Analysis();
    }



    /// <summary>
    /// 開始任務
    /// </summary>
    //public bool Start()
    public bool Download()
    {
      if (BasePlugin == null)
      {
        Status = DownloadStatus.Error;
        throw new Exception("Plugin Not Found");
      }
      if (_downloader == null)
      {
        _downloader = BasePlugin.CreateDownloader();
        _downloader.TaskInfo = this;
      }

      return _downloader.Download();
    }

    /// <summary>
    /// 停止任務
    /// </summary>
    public void Stop()
    {
      if (_downloader != null)
        _downloader.StopDownload();
    }


  }
}
