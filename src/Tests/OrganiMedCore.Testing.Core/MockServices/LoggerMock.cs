using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace OrganiMedCore.Testing.Core.MockServices
{
    public class LoggerMock<T> : ILogger<T>
    {
        public List<LogMockItem> Logs { get; }


        public LoggerMock()
        {
            Logs = new List<LogMockItem>();
        }


        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logs.Add(new LogMockItem()
            {
                LogLevel = logLevel,
                EventId = eventId,
                State = state,
                Exception = exception
            });
        }
    }


    public class LogMockItem
    {
        public LogLevel LogLevel { get; set; }

        public EventId EventId { get; set; }

        public Exception Exception { get; set; }

        public object State { get; set; }
    }
}
