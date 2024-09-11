using CesiumIonRevitAddin.Utils;
using System;
using System.IO;

namespace CesiumIonRevitAddin
{
    internal class Logger
    {
        static Logger instance = null;
        static readonly object LogMutex = new object();
        private readonly StreamWriter logFile;
        private static bool enabled = true;

        public static bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

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
            if (!enabled) return;

            lock (LogMutex)
            {
                DateTime now = DateTime.Now;
                var timeStr = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                logFile.WriteLine($"{timeStr}: {message}");
            }
        }
    }
}
