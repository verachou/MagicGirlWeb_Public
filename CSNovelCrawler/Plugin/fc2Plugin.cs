using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;
using CSNovelCrawler.Interface;

namespace CSNovelCrawler.Plugin
{
  [PluginInformation("fc2Downloader", "fc2插件", "JeanLin", "1.0.0.0", "FC2下載插件", "https://blog.fc2.com")]
  public class fc2Plugin : AbstractPlugin
  {
    public fc2Plugin(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
      pattern = @"^http(s*)?:\/\/\w*\.blog.fc2.com\/(?<TID>.*)";
      PluginName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name.Replace("Plugin", "".ToLower());
    }

    public override IDownloader CreateDownloader()
    {
      return new fc2Downloader(_loggerFactory);
    }

    public override string GetHash(string url)
    {
      Regex r = new Regex(pattern);
      Match m = r.Match(url);
      if (m.Success)
      {
        MD5 md5 = MD5.Create();
        byte[] b = md5.ComputeHash(Encoding.Default.GetBytes(m.Groups["TID"].Value));
        string hash = Convert.ToBase64String(b);
        return PluginName + hash;
      }
      return null;
    }

  }
}
