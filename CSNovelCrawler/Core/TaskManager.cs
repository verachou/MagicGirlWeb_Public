using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;


namespace CSNovelCrawler.Core
{
  public class TaskManager : IDisposable
  {
    private readonly ILogger _logger;
    private readonly CustomSettings _setting;
    //存放任務
    public List<TaskInfo> TaskInfos = new List<TaskInfo>();
    //TaskInfos新增刪除鎖定
    public object TaskInfosLock = new object();
    //SaveLock鎖定
    public object SaveTaskLock = new object();

    public TaskManager(ILoggerFactory loggerFactory, CustomSettings setting)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
      _setting = setting;
    }

    /// <summary>
    /// 增加任務
    /// </summary>
    /// <param name="plugin">任務的插件</param>
    /// <param name="url">任務網址</param>
    /// <returns></returns>
    public TaskInfo AddTask(IPlugin plugin, string url)
    {
      //建立TaskInfo物件
      var taskInfo = new TaskInfo
      (
        url,
        System.Text.Encoding.GetEncoding(_setting.TextEncoding),
        plugin,
        _setting.DefaultSaveFolder,
        DownloadStatus.TaskAnalysis
      );

      //向任務集合增加新任務
      Monitor.Enter(TaskInfosLock);
      TaskInfos.Add(taskInfo);
      Monitor.Exit(TaskInfosLock);

      return taskInfo;
    }

    public void AnalysisTask(TaskInfo taskInfo)
    {
      try
      {
        taskInfo.Status = DownloadStatus.TaskAnalysis;

        if (taskInfo.Analysis())
        {
          taskInfo.Status = DownloadStatus.AnalysisComplete;
          if (taskInfo.CustomFileName == "Unknown")
          {
            FormatFileName FFN = new FormatFileName();
            taskInfo.CustomFileName = FFN.OutputFormat(taskInfo, _setting.SelectFormat);
          }
        }
        else
        {
          taskInfo.Status = DownloadStatus.AnalysisFailed;
        }

      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        taskInfo.Status = DownloadStatus.Error;
      }

    }

    // 目前未使用功能
    // public void AnalysisTaskAsync(TaskInfo taskInfo)
    // {
    //   var t = new Thread(() =>
    //   {
    //     try
    //     {
    //       taskInfo.Status = DownloadStatus.TaskAnalysis;

    //       if (taskInfo.Analysis())
    //       {
    //         taskInfo.Status = DownloadStatus.AnalysisComplete;
    //         if (taskInfo.CustomFileName == "Unknown")
    //         {
    //           FormatFileName FFN = new FormatFileName();
    //           taskInfo.CustomFileName = FFN.OutputFormat(taskInfo, _setting.SelectFormat);
    //         }
    //       }
    //       else
    //       {
    //         taskInfo.Status = DownloadStatus.AnalysisFailed;
    //       }

    //     }
    //     catch (Exception ex)
    //     {
    //       _logger.LogError(ex.ToString());
    //       taskInfo.Status = DownloadStatus.Error;
    //     }


    //   })
    //   { IsBackground = true };
    //   t.Start();

    // }
    
    /// <summary>
    /// 切換訂閱任務
    /// </summary>
    /// <param name="taskInfo"></param>
    /// 預定移除功能
    // public void SwitchSubscribe(TaskInfo taskInfo)
    // {
    //   var t = new Thread(() =>
    //   {
    //     try
    //     {
    //       taskInfo.Subscribe = !taskInfo.Subscribe;
    //     }
    //     catch (Exception ex)
    //     {
    //       _logger.LogError(ex.ToString());
    //       taskInfo.Status = DownloadStatus.Error;
    //     }

    //   })
    //   { IsBackground = true };

    //   t.Start();

    // }

    /// <summary>
    /// 訂閱任務排程
    /// </summary>
    /// 預定移除功能
    // public void SubscribeTask()
    // {
    //   var t = new Thread(() =>
    //   {
    //     foreach (var taskInfo in TaskInfos.FindAll(taskInfo =>
    //               taskInfo.Subscribe &&
    //               taskInfo.Status != DownloadStatus.Downloading &&
    //               taskInfo.Status != DownloadStatus.SubscribeCheck &&
    //               taskInfo.Status != DownloadStatus.SubscribeUpdate &&
    //               taskInfo.Status != DownloadStatus.Error

    //               ))
    //     {
    //       taskInfo.Status = DownloadStatus.SubscribeCheck;

    //       if (taskInfo.Analysis())
    //       {
    //         if (taskInfo.TotalSection > taskInfo.CurrentSection)
    //         {
    //           taskInfo.EndSection = taskInfo.TotalSection;
    //           taskInfo.Status = DownloadStatus.SubscribeUpdate;
    //           DownloadTask(taskInfo);
    //         }
    //         else
    //         {
    //           taskInfo.Status = DownloadStatus.SubscribeNoneUpdate;
    //         }
    //       }
    //       else
    //       {
    //         taskInfo.Status = DownloadStatus.AnalysisFailed;
    //       }

    //     }

    //   })
    //   { IsBackground = true };

    //   t.Start();

    // }


    /// <summary>
    /// 開始任務
    /// </summary>
    /// <param name="taskInfo"></param>
    public void DownloadTask(TaskInfo taskInfo)
    {
      try
      {
        taskInfo.Status = DownloadStatus.Downloading;
        if (taskInfo.Download())
        {
          taskInfo.Status = DownloadStatus.DownloadComplete;
        }
        else
        {
          taskInfo.Status = DownloadStatus.Downloadblocked;
        }

      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        taskInfo.Status = DownloadStatus.Error;
      }

    }

    // 目前未使用功能
    // public void DownloadTaskAsync(TaskInfo taskInfo)
    // {
    //   var t = new Thread(() =>
    //   {
    //     try
    //     {
    //       taskInfo.Status = DownloadStatus.Downloading;
    //       if (taskInfo.Download())
    //       {
    //         taskInfo.Status = DownloadStatus.DownloadComplete;
    //       }
    //       else
    //       {
    //         taskInfo.Status = DownloadStatus.Downloadblocked;
    //       }

    //     }
    //     catch (Exception ex)
    //     {
    //       _logger.LogError(ex.ToString());
    //       taskInfo.Status = DownloadStatus.Error;
    //     }

    //   })
    //   { IsBackground = true };
    //   t.Start();
    // }



    /// <summary>
    /// 停止任務
    /// </summary>
    /// <param name="taskInfo"></param>
    public void StopTask(TaskInfo taskInfo)
    {
      //只有已開始的任務才能停止
      switch (taskInfo.Status)
      {

        case DownloadStatus.Downloading:
          taskInfo.Status = DownloadStatus.Stopping;

          break;
        default:
          taskInfo.Status = DownloadStatus.TaskPause;
          return;
      }

      //停止任務
      taskInfo.Stop();

      if (taskInfo.Status != DownloadStatus.TaskPause)
      {
        //啟動新執行緒等待任務完全停止
        var t = new Thread(() =>
            {
              //等待時間 (10秒)
              int timeout = 10000;
              //等待停止
              while (taskInfo.Status == DownloadStatus.Stopping)
              {
                Thread.Sleep(500);
                timeout -= 500;
                if (timeout < 0 || taskInfo.HasStopped) //如果到时仍未停止
                {
                  break;
                }
              }

              taskInfo.Status = DownloadStatus.TaskPause;
              //重新整理UI
              // PreDelegates.Refresh(new ParaRefresh(taskInfo));
            })
        { IsBackground = true };
        t.Start();
      }
      //釋放Downloader
      taskInfo.DisposeDownloader();
    }
    /// <summary>
    /// 刪除任任務(自動停止進行中的任務)
    /// </summary>
    /// <param name="taskInfo"></param>
    /// 預定移除功能
    // public void DeleteTask(TaskInfo taskInfo)
    // {
    //   //停止任務
    //   StopTask(taskInfo);

    //   //啟動新執行緒等待任務完全停止
    //   ThreadPool.QueueUserWorkItem(o =>
    //       {
    //         try
    //         {
    //           while (taskInfo.Status == DownloadStatus.Stopping || taskInfo.Status == DownloadStatus.Downloading)
    //           {
    //             Thread.Sleep(50);
    //           }
    //           Monitor.Enter(TaskInfosLock);
    //           TaskInfos.Remove(taskInfo);
    //           Monitor.Exit(TaskInfosLock);
    //           //重新整理UI
    //           // PreDelegates.Refresh(new ParaRefresh(taskInfo));
    //         }
    //         catch (Exception ex)
    //         {
    //           // log.Error(ex.ToString());
    //           _logger.LogError(ex.ToString());
    //           taskInfo.Status = DownloadStatus.Error;
    //           // PreDelegates.Refresh(new ParaRefresh(taskInfo));
    //         }
    //       });
    // }

    /// <summary>
    /// 結束並儲存所有任務
    /// </summary>
    /// 預定移除功能
    // public void BreakAndSaveAllTasks()
    // {

    //   var t = new Thread(() =>
    //   {


    //     try
    //     {
    //       EndSaveBackgroundWorker();
    //       foreach (var taskInfo in TaskInfos.FindAll(taskInfo => taskInfo.Status == DownloadStatus.Downloading))
    //         StopTask(taskInfo);
    //       while (TaskInfos.FindAll(taskInfo => taskInfo.Status == DownloadStatus.Stopping || taskInfo.Status == DownloadStatus.Downloadblocked).Count > 0)
    //       {
    //         Thread.Sleep(50);
    //       }

    //       SaveAllTasks();
    //       // PreDelegates.Exit();

    //     }
    //     catch (Exception ex)
    //     {
    //       // log.Error(ex.ToString());
    //       _logger.LogError(ex.ToString());
    //     }
    //   })
    //   { IsBackground = true };
    //   t.Start();
    // }



    /// <summary>
    /// 用GUID找對應的任務
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    /// 預定移除功能
    // [DebuggerNonUserCode]
    // public TaskInfo GetTask(Guid guid)
    // {

    //   return TaskInfos.Find(taskInfo => taskInfo.TaskId == guid);
    // }



    private bool _bgWorkerContinue;
    private Timer _bgWorker;
    /// <summary>
    /// 啟動背景自動儲存任務
    /// </summary>
    /// 預定移除功能
    // public void StartSaveBackgroundWorker()
    // {
    //   _bgWorkerContinue = true;
    //   if (_bgWorker == null)
    //   {
    //     //每60秒儲存任務資訊
    //     _bgWorker = new Timer(SaveBackgroundWorker, null, 60000, 60000);
    //   }
    // }

    /// <summary>
    /// 停止背景自動儲存任務
    /// </summary>
    /// 預定移除功能
    // public void EndSaveBackgroundWorker()
    // {
    //   _bgWorkerContinue = false;
    // }

    /// <summary>
    /// 背景自動儲存任務
    /// </summary>
    /// <param name="o"></param>
    /// 預定移除功能
    // private void SaveBackgroundWorker(object o)
    // {
    //   if (_bgWorkerContinue)
    //   {
    //     SaveAllTasks();
    //   }
    //   else
    //   {
    //     _bgWorker.Dispose();
    //   }
    // }

    private string _TaskFolderPath { get { return CoreManager.StartupPath; } }
    private string _TaskFileName { get { return "Task.xml"; } }
    private string TaskFullFileName { get { return Path.Combine(_TaskFolderPath, _TaskFileName); } }

    /// <summary>
    /// 儲存任務列表到xml
    /// </summary>
    /// 預定移除功能
    // public void SaveAllTasks()
    // {
    //   Monitor.Enter(SaveTaskLock);
    //   using (var oFileStream = new FileStream(TaskFullFileName, FileMode.Create))
    //   {
    //     var oXmlSerializer = new XmlSerializer(typeof(List<TaskInfo>));
    //     oXmlSerializer.Serialize(oFileStream, TaskInfos);
    //   }
    //   Monitor.Exit(SaveTaskLock);

    // }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // dispose managed resources
        _bgWorker.Dispose();
        TaskInfos.Clear();
      }
      // free native resources
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

  }
}
