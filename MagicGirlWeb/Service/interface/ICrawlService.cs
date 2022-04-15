using System.IO;
using MagicGirlWeb.Models;


namespace MagicGirlWeb.Service
{
  public interface ICrawlService
  {
    Book Analysis(string url);

    Book Download(
      string url,
      int lastPageFrom,
      int lastPageTo,
      string filePath
    );

    void DeleteLocalFile(string url);

    string Format(string text, FormatType formatTypes);

  }
  public enum FormatType
  {
    Traditional,
    DoubleEOL
  }
}