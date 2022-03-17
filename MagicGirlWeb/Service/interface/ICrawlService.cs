using System.IO;
using MagicGirlWeb.Models;


namespace MagicGirlWeb.Service
{
  public interface ICrawlService
  {
    Book Analysis(string url);

    FileStream Download(
        string url,
        int lastPageFrom,
        int lastPageTo
    );

    void DeleteLocalFile(string url);


  }
}