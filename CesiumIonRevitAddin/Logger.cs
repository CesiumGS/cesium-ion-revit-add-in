using CesiumIonRevitAddin.Model;
using CesiumIonRevitAddin.Utils;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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

        public void Log(GeometryDataObject geometryDataObject, bool printVertices = false)
        {

            double sumX = 0, sumY = 0, sumZ = 0;
            int pointCount = geometryDataObject.Vertices.Count / 3;

            for (int i = 0; i < geometryDataObject.Vertices.Count; i += 3)
            {
                sumX += geometryDataObject.Vertices[i];     // X component
                sumY += geometryDataObject.Vertices[i + 1];  // Y component
                sumZ += geometryDataObject.Vertices[i + 2];  // Z component
            }

            // Calculate the average for each component to get the centroid
            double centroidX = sumX / pointCount;
            double centroidY = sumY / pointCount;
            double centroidZ = sumZ / pointCount;

            string formattedList = "";
            if (printVertices)
            {
                formattedList = string.Join(", ", geometryDataObject.Vertices.Select(d => d.ToString("F2")));
            }
            lock (LogMutex)
            {
                DateTime now = DateTime.Now;
                var timeStr = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                logFile.WriteLine($"{timeStr}: Centroid: <{centroidX:F2}, {centroidY:F2}, {centroidZ:F2}>");            
            if (printVertices)
                {
                    logFile.WriteLine($"{timeStr}: {formattedList}");
                }
            }
        }
    }
}
