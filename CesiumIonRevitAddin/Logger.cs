using System;
using System.IO;
using System.Threading;

namespace CesiumIonRevitAddin
{
    internal class Logger
    {
        static Logger instance = null;
        static readonly object LogMutex = new object();
        private StreamWriter logFile;
        private static bool enabled = true;

        public static bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private Logger()
        {
            string logFilePath = "C:\\Scratch\\CesiumIonRevitAddin\\CesiumIonRevitAddinLog.txt";
            logFile = new StreamWriter(logFilePath, true) { AutoFlush = true };
        }

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (LogMutex)
                    {
                        if (instance == null) // Double-check locking
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
                var now = DateTime.Now;
                var timeStr = now.ToString("yyyy-MM-dd HH:mm:ss");
                logFile.WriteLine($"{timeStr}: {message}");
            }
        }
    }
}
