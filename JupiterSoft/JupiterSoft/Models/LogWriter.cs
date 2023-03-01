using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.Models
{
    public static class LogWriter
    {
        public static readonly string _logDirectory = @"D:\CheckWeigher\logs";

        public static void LogWrite(string logMessage, string sessionId)
        {

            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
                string fileName = sessionId + "_log.txt";
                string logPath = Path.Combine(_logDirectory, fileName);
                if (!File.Exists(logPath))
                {
                    File.Create(logPath).Dispose();
                }
                using (StreamWriter w = File.AppendText(logPath))
                {
                    Log(logMessage, w);
                }
            }
            catch
            {
            }
        }

        public static void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\n Log Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("\n");
                txtWriter.WriteLine("\r Message Log  : {0}", logMessage);
                txtWriter.WriteLine("\n");
                txtWriter.WriteLine("-------------------------------");
                txtWriter.Dispose();
            }
            catch
            {
            }
        }
    }
}
