using System;
using System.IO;

namespace iTuneService
{
    public static class Log
    {
        public static void Write(string log)
        {
            using (var fs = new FileStream(Environment.CurrentDirectory + @"\iTuneService.log", FileMode.OpenOrCreate, FileAccess.Write))
            using (var sr = new StreamWriter(fs))
            {                
                sr.BaseStream.Seek(0, SeekOrigin.End);
                sr.WriteLine(DateTime.Now + ": " + log);
                sr.Flush();
                sr.Close();
            }
        }
    }
}
