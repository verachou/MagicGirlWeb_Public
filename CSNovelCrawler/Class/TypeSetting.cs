using CSNovelCrawler.Core;
using CSNovelCrawler.Class;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace CSNovelCrawler.Class
{
  public interface ITypeSetting
  {
    void Set(ref string txt);
  }
  public class BrRegex : ITypeSetting
  {
    public void Set(ref string txt)
    {
      //txt = Regex.Replace(txt, "<BR>", "\r\n", RegexOptions.IgnoreCase);
      txt = Regex.Replace(txt, @"(<BR>)|(br)|(<BR*/>)|<br*/>", "\r\n", RegexOptions.IgnoreCase);
    }
  }

  public class PRegex : ITypeSetting
  {
    public void Set(ref string txt)
    {
      txt = Regex.Replace(txt, "<p>", "", RegexOptions.IgnoreCase);
      txt = Regex.Replace(txt, "</p>", "\r\n", RegexOptions.IgnoreCase);
    }
  }

  public class AnnotationRegex : ITypeSetting
  {
    public void Set(ref string txt)
    {
      txt = Regex.Replace(txt, "<!--PAGE.+?-->", "\r\n", RegexOptions.IgnoreCase);
    }
  }


  public class HjwzwRegex : ITypeSetting
  {
    public void Set(ref string txt)
    {
      txt = Regex.Replace(txt, "在搜索引擎輸入(.)*返回書頁", string.Empty, RegexOptions.Singleline);
    }
  }

  public class stoRegex : ITypeSetting
  {
    public void Set(ref string txt)
    {
      txt = Regex.Replace(txt, @"本*作*品*由*思*兔*網*提*供*線*上*閱*讀*", string.Empty, RegexOptions.Singleline);
    }
  }

  public class u8hexRegex : ITypeSetting
  {
    // private LogManager log = new LogManager(typeof(u8hexRegex));
    public void Set(ref string txt)
    {
      Regex r = new Regex(@"(?<U8>(%[a-fA-F0-9]{2}){1,12})");
      MatchCollection matchs = r.Matches(txt);
      foreach (Match m in matchs)
      {
        string tmpStr = m.Groups["U8"].Value;
        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
        string tmpHexStr = tmpStr.Replace("%", "");

        int NumberChars = tmpHexStr.Length / 2;
        byte[] tmpByteStr = new byte[NumberChars];
        using (var sr = new StringReader(tmpHexStr))
        {
          for (int i = 0; i < NumberChars; i++)
            tmpByteStr[i] =
              Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
        }
        string U8Str = System.Text.Encoding.UTF8.GetString(tmpByteStr);
        txt = txt.Replace(tmpStr, U8Str);
      }
    }
  }

  //public class RemoveSpecialCharacters:ITypeSetting
  //{
  //    public void Set(ref string txt)
  //    {
  //        txt = Regex.Replace(txt, "&quot;", "\"");
  //        txt = Regex.Replace(txt, "&nbsp;", " ");
  //        txt = Regex.Replace(txt, "&#65279;", string.Empty);
  //    }
  //}
  public class UniformFormat : ITypeSetting
  {
    public void Set(ref string txt)
    {
      txt = Regex.Replace(txt, @"(^\s+)", string.Empty, RegexOptions.Multiline);
      txt = Regex.Replace(txt, @"^(?=\S+)", @"　　", RegexOptions.Multiline);
      txt = Regex.Replace(txt, @"[\r|\n]*$[\r|\n]*", "\r\n\r\n", RegexOptions.Multiline);
    }
  }

  public class Remove0007 : ITypeSetting
  {
    public void Set(ref string txt)
    {
      txt = Regex.Replace(txt, @"([\x00-\x07])", string.Empty);
    }
  }

  public class EynyTag : ITypeSetting
  {
    public void Set(ref string txt)
    {
      txt = Regex.Replace(txt, @"...<div class='locked'><em>瀏覽完整內容，請先 <a href='member.php\?mod=register'>註冊<\/a> 或 <a href='javascript:;' onclick=""lsSubmit\(\)"">登入會員<\/a><\/em><\/div>", string.Empty);
      txt = Regex.Replace(txt, @"<div><\/div>", string.Empty);
    }
  }

  public class Traditional : ITypeSetting
  {
    public void Set(ref string txt)
    {
      //txt = CharSetConverter.ToTraditional(txt);
      txt = OpenCC.ConvertToTW(txt);
    }
  }

  public class HtmlDecode : ITypeSetting
  {
    public void Set(ref string txt)
    {
      txt = Regex.Replace(txt, @"&#?\w+;", m => HttpUtility.HtmlDecode(m.Value));
    }
  }


  public class SfacgToIndent : ITypeSetting
  {
    public void Set(ref string txt)
    {
      txt = Regex.Replace(txt, "&nbsp; &nbsp; ", "\r\n");
      txt = Regex.Replace(txt, @"<BR>|<br>|<\/?p>", "\r\n");
    }
  }

  public class PttFormat : ITypeSetting
  {
    public void Set(ref string txt)
    {
      //txt = Regex.Replace(txt, @"&#?\w+;", m => HttpUtility.HtmlDecode(m.Value));
      txt = Regex.Replace(txt, "&nbsp;", " ");
      txt = Regex.Replace(txt, @"[\r|\n]*$[\r|\n]*", "\r\n\r\n", RegexOptions.Multiline);
    }
  }

  public class DoubleEOL : ITypeSetting
  {
    public void Set(ref string txt)
    {
    //   txt = Regex.Replace("    \r\n", @"<br />");
    //   txt = Regex.Replace("\r\n", @"<br />");
    //   txt = Regex.Replace("\n", @"<br />");
    //   txt = Regex.Replace("\r", @"<br />");
      txt = Regex.Replace(txt, @"(\s\s\s\s\r\n)|(\r\n)|(\n)|(\r)", @"<br />");   
      txt = Regex.Replace(txt, @"<br />", Environment.NewLine + Environment.NewLine);
    }

}

}
