using System.IO;
using System.Threading;

namespace MagicGirlWeb.Service
{
  public interface IFileService
  {
    bool Download(
        string fileId,
        FileStream fileStream
    );

    string Upload(
        string fileName,
        string fileFolder,
        string mimeType,
        string description
    );

  }
}