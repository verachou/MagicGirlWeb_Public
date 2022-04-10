using System.IO;
using System.Threading;

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
      string description
    );

    void ClearLocalFolder(string folderPath);

  }
}