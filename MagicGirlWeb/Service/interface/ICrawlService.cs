using System.IO;
using MagicGirlWeb.Models;


namespace MagicGirlWeb.Service
{
  public interface ICrawlService
  {
    Book Analysis(string url);

    bool Download(
        string url,
        int lastPageFrom,
        int lastPageTo,
        FileStream fileStream
    );

    void DeleteLocalFile(string url);


  }
}