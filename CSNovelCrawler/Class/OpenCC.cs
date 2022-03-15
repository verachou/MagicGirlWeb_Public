using CSNovelCrawler.Core;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace CSNovelCrawler.Class
{
  static class OpenCC
  {
    [DllImport("x86\\opencc.dll", EntryPoint = "opencc_open", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr opencc_open_32([MarshalAs(UnmanagedType.LPStr)] string configFileName);

    [DllImport("x64\\opencc.dll", EntryPoint = "opencc_open", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr opencc_open_64([MarshalAs(UnmanagedType.LPStr)] string configFileName);

    public static IntPtr opencc_open([MarshalAs(UnmanagedType.LPStr)] string configFileName)
    {
    //   string path = System.Web.HttpRuntime.AppDomainAppPath + "opencc\\";
      string path = Directory.GetCurrentDirectory() + @"\packages\opencc\";
      if (!Directory.Exists(path))
      {
        Console.WriteLine("{0} is not exist.", path);
      }
      // Console.WriteLine(Directory.GetCurrentDirectory());
      return IntPtr.Size == 8 /* 64bit */ ? opencc_open_64(path + "x64\\" + configFileName) : opencc_open_32(path + "x86\\" + configFileName);
    }

    [DllImport("x86\\opencc.dll", EntryPoint = "opencc_close", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern int opencc_close_32(IntPtr opencc);
    [DllImport("x64\\opencc.dll", EntryPoint = "opencc_close", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern int opencc_close_64(IntPtr opencc);
    public static int opencc_close(IntPtr opencc)
    {
      return IntPtr.Size == 8 /* 64bit */ ? opencc_close_64(opencc) : opencc_close_32(opencc);
    }

    [DllImport("opencc.dll", EntryPoint = "opencc_convert_utf8", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr opencc_convert_utf8_32(IntPtr opencc, IntPtr input, int length);
    [DllImport("opencc.dll", EntryPoint = "opencc_convert_utf8", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr opencc_convert_utf8_64(IntPtr opencc, IntPtr input, int length);
    public static IntPtr opencc_convert_utf8(IntPtr opencc, IntPtr input, int length)
    {
      return IntPtr.Size == 8 /* 64bit */ ? opencc_convert_utf8_64(opencc, input, length) : opencc_convert_utf8_32(opencc, input, length);
    }

    [DllImport("x86\\opencc.dll", EntryPoint = "opencc_convert_utf8_free", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern void opencc_convert_utf8_free_32(IntPtr input);
    [DllImport("x64\\opencc.dll", EntryPoint = "opencc_convert_utf8_free", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern void opencc_convert_utf8_free_64(IntPtr input);
    public static void opencc_convert_utf8_free(IntPtr input)
    {
      if (IntPtr.Size == 8)
      {
        opencc_convert_utf8_free_64(input);
      }
      else
      {
        opencc_convert_utf8_free_32(input);
      }
    }

    [DllImport("x86\\opencc.dll", EntryPoint = "opencc_error", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr opencc_error_32();
    [DllImport("x64\\opencc.dll", EntryPoint = "opencc_error", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr opencc_error_64();
    public static IntPtr opencc_error()
    {
      return IntPtr.Size == 8 /* 64bit */ ? opencc_error_64() : opencc_error_32();
    }

    static IntPtr OpenCCInstance = IntPtr.Zero;

    static OpenCC()
    {
    }

    public static IntPtr NativeUtf8FromString(string managedString)
    {
      int len = Encoding.UTF8.GetByteCount(managedString);
      byte[] buffer = new byte[len + 1];
      Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
      IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
      Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
      return nativeUtf8;
    }

    private static string StringFromNativeUtf8(IntPtr nativeUtf8)
    {
      int len = 0;
      while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
      byte[] buffer = new byte[len];
      Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
      return Encoding.UTF8.GetString(buffer);
    }

    public static string ConvertToTW(string text)
    {
      OpenCCInstance = opencc_open("s2tw.json");

      if (OpenCCInstance.ToInt64() == -1)
      {
        IntPtr ptrmsg = opencc_error();
        string msg = StringFromNativeUtf8(ptrmsg);
        // log.Error("OpenCCInstance error, " + msg);
        Marshal.FreeHGlobal(ptrmsg);
      }
      else
      {
        IntPtr inStr = NativeUtf8FromString(text);
        IntPtr outStr = opencc_convert_utf8(OpenCCInstance, inStr, -1);
        if (outStr == IntPtr.Zero)
        {
          IntPtr ptrmsg = opencc_error();
          string msg = StringFromNativeUtf8(ptrmsg);
          // log.Error("opencc_convert_utf8 error, " + msg);
        }
        else
        {
          //log.Debug("opencc_convert_utf8 outStr, " + outStr.ToString());
          text = StringFromNativeUtf8(outStr);

          opencc_convert_utf8_free(outStr);
          Marshal.FreeHGlobal(inStr);
        }

        opencc_close(OpenCCInstance);
      }

      return text;
    }

    public static string ConvertToSP(string text)
    {
      IntPtr OpenCCInstance = IntPtr.Zero;
      OpenCCInstance = opencc_open("tw2sp.json");

      if (OpenCCInstance.ToInt64() == -1)
      {
        IntPtr ptrmsg = opencc_error();
        string msg = StringFromNativeUtf8(ptrmsg);
        // log.Error("OpenCCInstance error, " + msg);
        Marshal.FreeHGlobal(ptrmsg);
      }
      else
      {
        IntPtr inStr = NativeUtf8FromString(text);
        IntPtr outStr = opencc_convert_utf8(OpenCCInstance, inStr, -1);
        if (outStr == IntPtr.Zero)
        {
          IntPtr ptrmsg = opencc_error();
          string msg = StringFromNativeUtf8(ptrmsg);
          // log.Error("opencc_convert_utf8 error, " + msg);
        }
        else
        {
          // log.Debug("opencc_convert_utf8 outStr, " + outStr.ToString());
          text = StringFromNativeUtf8(outStr);

          opencc_convert_utf8_free(outStr);
          Marshal.FreeHGlobal(inStr);
        }

        opencc_close(OpenCCInstance);
      }

      return text;
    }
  }
}