using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesLinkTrackingOnline
{
    public class Logs
    {
        private static string filePath = null;
        public static void InitialLogFile()
        {
            var fileName = String.Format("Logs_{0}.log", DateTime.Now.ToString("yyyyMMddhhmmss"));
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (!File.Exists(filePath))
            {
                using (var fs = File.Create(filePath))
                { }
            }
        }

        public static void LogToFile(LogLevel level, DateTime occurred, string message)
        {
            var logInfo = String.Format("{0}\t{1}\t{2}", level.ToString(), occurred.ToString("o"), message);
            using (StreamWriter writer = new StreamWriter(filePath, true, Encoding.UTF8))
            {
                writer.WriteLine(logInfo);
            }
        }

        public static void LogMessage(string message)
        {
            using (StreamWriter writer = new StreamWriter(filePath, true, Encoding.UTF8))
            {
                writer.WriteLine("----------------------------------------------------------------------");
                writer.WriteLine(message);
                writer.WriteLine("----------------------------------------------------------------------");
            }
        }
    }

    public enum LogLevel
    {
        Info = 0,
        Warning,
        Error
    }
}
