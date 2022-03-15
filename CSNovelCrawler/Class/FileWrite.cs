using System;
using System.IO;
using System.Text;
using CSNovelCrawler.Core;

namespace CSNovelCrawler.Class
{
    public class FileWrite
    {
        //private static LogManager log = new LogManager();

        public static Encoding GetFileEncoding(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return Encoding.Unicode;
            }
            using (var br = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read)))
            {
                Byte[] buffer = br.ReadBytes(2);
                if (buffer[0] >= 0xEF)
                {
                    if (buffer[0] == 0xEF && buffer[1] == 0xBB)
                    {
                        return Encoding.UTF8;
                    }
                    if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                    {
                        return Encoding.BigEndianUnicode;
                    }
                    if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                    {
                        return Encoding.Unicode;
                    }

                }
            }

            return Encoding.Default;
        }
        public static void TxtWrire(string txt, string fileName,Encoding textEncoding)
        {
            try
            {
                
                if (string.IsNullOrEmpty(fileName))
                    throw new FileLoadException("目錄存取有誤");

                string directoryName = Path.GetDirectoryName(fileName);

                if (string.IsNullOrEmpty(directoryName))
                    throw new FileLoadException("目錄存取有誤");

                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }


                if (textEncoding == null) textEncoding = GetFileEncoding(fileName);

                FileStream file = new FileStream(fileName, FileMode.Append);
                using (var sw = new StreamWriter(file, textEncoding))
                {
                    sw.Write(txt);
                    sw.Close();
                }
                file.Close();
            }
            catch (Exception ex)
            {
                //log.Error(ex.ToString());
                throw;
            }
        }
    }
}
