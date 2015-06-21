using System;
using System.Collections;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using NLog;

namespace LeagueRecorder.Shared.Implementations.LoggingTargets
{
    public class LogEntity : TableEntity
    {
        public LogEntity(LogEventInfo logEvent, string layoutMessage)
        {
            this.LoggerName = logEvent.LoggerName;
            this.LogTimeStamp = logEvent.TimeStamp.ToString("G");
            this.Level = logEvent.Level.Name;
            this.Message = logEvent.FormattedMessage;
            this.MessageWithLayout = layoutMessage;
            if (logEvent.Exception != null)
            {
                this.Exception = logEvent.Exception.ToString();
                if (logEvent.Exception.Data.Count > 0)
                {
                    this.ExceptionData = GetExceptionDataAsString(logEvent.Exception);
                }
                if (logEvent.Exception.InnerException != null)
                {
                    this.InnerException = logEvent.Exception.InnerException.ToString();
                }
            }
            if (logEvent.StackTrace != null)
            {
                this.StackTrace = logEvent.StackTrace.ToString();
            }
            this.RowKey = Guid.NewGuid().ToString("B");
            this.PartitionKey = this.LoggerName;
            this.MachineName = Environment.MachineName;
        }

        private static string GetExceptionDataAsString(Exception exception)
        {
            var data = new StringBuilder();
            foreach (DictionaryEntry entry in exception.Data)
            {
                data.AppendLine(entry.Key + "=" + entry.Value);
            }
            return data.ToString();
        }

        public LogEntity()
        {
        }

        public string LogTimeStamp { get; set; }
        public string Level { get; set; }
        public string LoggerName { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string InnerException { get; set; }
        public string StackTrace { get; set; }
        public string MessageWithLayout { get; set; }
        public string ExceptionData { get; set; }
        public string MachineName { get; set; }
    }
}