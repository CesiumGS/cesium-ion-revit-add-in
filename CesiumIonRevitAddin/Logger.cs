using CesiumIonRevitAddin.Utils;
using System;
using System.IO;

namespace CesiumIonRevitAddin
{
    internal class Logger
    {
        private static Logger instance = null;
        private static readonly object LogMutex = new object();
        private readonly StreamWriter logFile;

        public static bool Enabled { get; set; } = true;

        private Logger()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string addinLogFolderPath = Path.Combine(Util.GetAddinUserDataFolder(), "Logs");
            Directory.CreateDirectory(addinLogFolderPath);
            string logFilePath = Path.Combine(addinLogFolderPath, $"CesiumIonRevitAddinLog_{timestamp}.txt");
            logFile = new StreamWriter(logFilePath, false) { AutoFlush = true };
        }

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (LogMutex)
                    {
                        if (instance == null)
                        {
                            instance = new Logger();
                        }
                    }
                }
                return instance;
            }
        }

        public void Log(string message)
        {
            if (!Enabled)
            {
                return;
            }

            lock (LogMutex)
            {
                DateTime now = DateTime.Now;
                var timeStr = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                logFile.WriteLine($"{timeStr}: {message}");
            }
        }
    }
}
