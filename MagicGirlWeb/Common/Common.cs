using CSNovelCrawler.Interface;

namespace MagicGirlWeb
{
  public static class Common
  {
    public static int DownloadStatusAdapter(DownloadStatus status)
    {
      int output = Constant.TASK_STATUS_UNKNOWN;
      switch (status)
      {
        case DownloadStatus.TaskAnalysis:
          output = Constant.TASK_STATUS_ANALYSE_PROCESSING;
          break;
        case DownloadStatus.Error:
          output = Constant.TASK_STATUS_ANALYSE_ERROR;
          break;
        case DownloadStatus.AnalysisComplete:
          output = Constant.TASK_STATUS_ANALYSE_COMPLETE;
          break;
        case DownloadStatus.AnalysisFailed:
          output = Constant.TASK_STATUS_ANALYSE_FAIL;
          break;
        case DownloadStatus.Downloading:
          output = Constant.TASK_STATUS_DOWNLOAD_PROCESSING;
          break;
        case DownloadStatus.DownloadComplete:
          output = Constant.TASK_STATUS_DOWNLOAD_COMPLETE;
          break;
        case DownloadStatus.Stopping:
          output = Constant.TASK_STATUS_STOPPING;
          break;
        case DownloadStatus.TaskPause:
          output = Constant.TASK_STATUS_PENDING;
          break;
        case DownloadStatus.Deleting:
          output = Constant.TASK_STATUS_DELETING;
          break;
        case DownloadStatus.Downloadblocked:
          output = Constant.TASK_STATUS_DOWNLOAD_STOP;
          break;
        case DownloadStatus.SubscribeCheck:
          output = Constant.TASK_STATUS_UNKNOWN;
          break;
        case DownloadStatus.SubscribeUpdate:
          output = Constant.TASK_STATUS_UNKNOWN;
          break;
        case DownloadStatus.SubscribeNoneUpdate:
          output = Constant.TASK_STATUS_UNKNOWN;
          break;
        default:
          output = Constant.TASK_STATUS_UNKNOWN;
          break;
      }
      return output;

    }

  }
}