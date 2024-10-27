using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace cs_third_party_web.Services
{
    public static class LogHandler
    {
        private static readonly BlockingCollection<LogEntry> logQueue = new BlockingCollection<LogEntry>();
        private static readonly Task logProcessorTask;

        static LogHandler()
        {
            logProcessorTask = Task.Run(ProcessLogEntries);
        }

        public static void WriteErrorLog(Exception ex, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string methodName = "")
        {
            logQueue.Add(new LogEntry
            {
                Type = LogType.Error,
                Exception = ex,
                FilePath = filePath,
                LineNumber = lineNumber,
                MethodName = methodName,
                Timestamp = DateTime.Now
            });
        }

        public static void WriteLog(string? Message)
        {
            logQueue.Add(new LogEntry
            {
                Type = LogType.Info,
                Message = Message,
                Timestamp = DateTime.Now
            });
        }
        public static void WriteDebugLog(string? Message)
        {
            logQueue.Add(new LogEntry
            {
                Type = LogType.Debug,
                Message = Message,
                Timestamp = DateTime.Now
            });
        }

        private static async Task ProcessLogEntries()
        {
            while (true)
            {
                try
                {
                    LogEntry logEntry = logQueue.Take();
                    await ProcessLogEntry(logEntry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing log entry: {ex.Message}");
                }
            }
        }

        private static async Task ProcessLogEntry(LogEntry logEntry)
        {
            try
            {
                string logPath = GetLogPath(logEntry);
                using (StreamWriter sw = new StreamWriter(logPath, true))
                {
                    switch (logEntry.Type)
                    {
                        case LogType.Info:
                            await sw.WriteLineAsync($"{logEntry.Timestamp.ToString("dd/MM/yyy HH:mm:ss.ff")} - {logEntry.Message}");
                            break;
                        case LogType.Debug:
                            await sw.WriteLineAsync($"{logEntry.Timestamp.ToString("dd/MM/yyy HH:mm:ss.ff")} - {logEntry.Message}");
                            break;
                        case LogType.Error:
                            string? fileName = Path.GetFileName(logEntry.FilePath);
                            await sw.WriteLineAsync($"{logEntry.Timestamp.ToString("dd/MM/yyy HH:mm:ss.ff")} - {fileName} - {logEntry.MethodName} - {logEntry.LineNumber} - {logEntry.Exception?.Message}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing log entry: {ex.Message}");
            }
        }

        private static string GetLogPath(LogEntry logEntry)
        {
            string logType = "";

            switch (logEntry.Type)
            {
                case LogType.Info:
                    logType = "Info";
                    break;
                case LogType.Error:
                    logType = "Error";
                    break;
                case LogType.Debug:
                    logType = "Debug";
                    break;
            }

            string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }
            return Path.Combine(logsDirectory, $"Log_{logType}_{logEntry.Timestamp.ToString("dd_MM_yyyy")}.txt");
        }

        private class LogEntry
        {
            public LogType Type { get; set; }
            public string? Message { get; set; }
            public Exception? Exception { get; set; }
            public string? FilePath { get; set; }
            public int LineNumber { get; set; }
            public string? MethodName { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private enum LogType
        {
            Info,
            Error,
            Debug
        }
    }
}
