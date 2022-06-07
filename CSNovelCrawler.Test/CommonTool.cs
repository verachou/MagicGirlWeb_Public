using System.IO;

namespace CSNovelCrawler.Test
{
  public class CommonTool
  {
    public void CleanFileDirectory(string filePath)
    {
      if (Directory.Exists(filePath))
      {
        Directory.Delete(filePath, true); // true: 移除 path 中的目錄、子目錄和檔案        
      }
      Directory.CreateDirectory(filePath);
    }

  }
}