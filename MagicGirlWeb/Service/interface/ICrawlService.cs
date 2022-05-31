using System;
using System.IO;
using System.Collections.Generic;
using MagicGirlWeb.Models;


namespace MagicGirlWeb.Service
{
  public interface ICrawlService
  {
    ICollection<PluginInfo> PluginInfos { get; set; }
    Book Analysis(string url);

    Book Download(
      string url,
      int lastPageFrom,
      int lastPageTo,
      string filePath,
      IProgress<int>? progress
    );

    void DeleteLocalFile(string url);

    string Format(string text, FormatType formatTypes);

    public class PluginInfo
    {
      public string Name { get; set; }
      public string AliasName { get; set; }
      public string SupportUrl { get; set; }
      public PluginInfo(string name, string alias, string url)
      {
        Name = name;
        AliasName = alias;
        SupportUrl = url;
      }
    }

  }
  public enum FormatType
  {
    Traditional,
    DoubleEOL
  }
}