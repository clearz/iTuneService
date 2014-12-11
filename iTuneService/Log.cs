using System;
using System.IO;
using System.Reflection;

namespace iTuneService
{
    public static class Log
    {
        private static readonly string _logFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                                                   "iTuneService.log");

        public static void Write(string log)
        {
            using (var fs = new FileStream(_logFileName, FileMode.OpenOrCreate, FileAccess.Write))
            using (var sr = new StreamWriter(fs))
            {                
                sr.BaseStream.Seek(0, SeekOrigin.End);
                sr.WriteLine(DateTime.Now + ": " + log);
                sr.Flush();
            }
        }
    }
}
