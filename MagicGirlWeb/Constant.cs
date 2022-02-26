namespace MagicGirlWeb
{
  public class Constant
  {
    // Book Status: 連載狀態
    public const int BOOK_STATUS_UNKNOWN = -1;
    public const int BOOK_STATUS_CONTINUE = 1;
    public const int BOOK_STATUS_ENDING = 2;
    public const int BOOK_STATUS_PENDING = 3;
    

    // Book Type
    public const int BOOK_TYPE_NOVEL = 1;
    public const int BOOK_TYPE_COMIC = 2;

    // Download Status
    public const int DOWNLOAD_STATUS_SUCCESS = 0;
    public const int DOWNLOAD_STATUS_FAIL = 1;

    // Authentication Provider
    public const string PROVIDER_GOOGLE = "Google";

    // Task Status
    public const int TASK_STATUS_UNKNOWN = -1;
    public const int TASK_STATUS_ANALYSE_PROCESSING = 0;
    public const int TASK_STATUS_ANALYSE_ERROR = 1;
    public const int TASK_STATUS_ANALYSE_COMPLETE =2;
    public const int TASK_STATUS_ANALYSE_FAIL = 3;
    public const int TASK_STATUS_DOWNLOAD_PROCESSING = 4;
    public const int TASK_STATUS_DOWNLOAD_COMPLETE = 5;
    public const int TASK_STATUS_STOPPING = 6;
    public const int TASK_STATUS_PENDING = 7;
    public const int TASK_STATUS_DELETING = 8;
    public const int TASK_STATUS_DOWNLOAD_STOP = 9;





  }
}