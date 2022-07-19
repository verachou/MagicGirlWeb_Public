using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace MagicGirlWeb.Service
{
  public interface IFileService
  {
    bool Download(
      string fileId,
      string filePath
    );

    string Upload(
      string filePath,
      string mimeType,
      string description,
      List<string> cloudFolderIds
    );

    void ClearLocalFolder(string folderPath);

    List<CloudFile> GetFileList(string cloudFolderId);

    public class CloudFile
    {
      public string Id;
      public string Name;
      public int Size;
      public string Description;
    }

  }
}