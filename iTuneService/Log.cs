using System;
using System.IO;

namespace iTuneService
{
    public static class Log
    {
        public static void Write(string log)
        {
            FileStream fs = new FileStream(Environment.CurrentDirectory + @"\iTuneService.log", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter m_streamWriter = new StreamWriter(fs);
            m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
            m_streamWriter.WriteLine(DateTime.Now.ToString() + ": " + log);
            m_streamWriter.Flush();
            m_streamWriter.Close();
        }
    }
}
