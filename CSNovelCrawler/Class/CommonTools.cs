using System.Text.RegularExpressions;
namespace CSNovelCrawler.Class
{
  public class CommonTools
  {

    public static int TryParse(string str, int DefaultNum)
    {
      int.TryParse(str, out DefaultNum);
      return DefaultNum;
    }

    public string RemoveSpecialChar(string input)
    {
      return Regex.Replace(input, @"[/\|\\\?""\*:><\.《》]+", "");
    }
  }
}
