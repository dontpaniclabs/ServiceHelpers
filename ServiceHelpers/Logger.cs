using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Configuration;

namespace DontPanic.Helpers
{
    public interface ILogger
    {
        void Log(string message);
        void LogCall(string message);
        void LogException(string message, Exception ex);
    }

    public class EnterpriseLogger : ILogger
    {
        private void LogInternal(string text, TraceEventType severity)
        {
            var cs =
                ConfigurationManager.GetSection("loggingConfiguration") as ConfigurationSection;

            if (cs != null)
            {
                var logentryType = 
                    Type.GetType("Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry, Microsoft.Practices.EnterpriseLibrary.Logging");

                if (logentryType != null)
                {
                    dynamic logEntry = Activator.CreateInstance(logentryType);
                    logEntry.Categories.Add("General");
                    logEntry.Severity = severity;
                    logEntry.Message = text;
                    logEntry.TimeStamp = DateTime.Now;

                    var loggerType = Type.GetType("Microsoft.Practices.EnterpriseLibrary.Logging.Logger, Microsoft.Practices.EnterpriseLibrary.Logging");

                    var logWriterFactoryType = Type.GetType("Microsoft.Practices.EnterpriseLibrary.Logging.LogWriterFactory, Microsoft.Practices.EnterpriseLibrary.Logging");
                    if (logWriterFactoryType != null)
                    {
                        var factory = Activator.CreateInstance(logWriterFactoryType);

                        if (factory != null)
                        {
                            var createFactoryMethod = logWriterFactoryType.GetMethod("Create", new Type[] { });
                            var createdFactory = createFactoryMethod.Invoke(factory, new object[] { });

                            var setLogWriterMethod = loggerType.GetMethod("SetLogWriter");
                            if (setLogWriterMethod != null && createdFactory != null)
                                setLogWriterMethod.Invoke(null, new object[] { createdFactory, false });
                        }
                    }

                    if (loggerType != null)
                    {
                        var method = loggerType.GetMethod("Write", new Type[] { logentryType });
                        method.Invoke(null, new object[] { logEntry });
                    }
                }
            }
        }

        public void Log(string message)
        {
            LogInternal(message, TraceEventType.Verbose);
        }

        public void LogCall(string message)
        {
            LogInternal(message, TraceEventType.Verbose);
        }

        public void LogException(string message, Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine("Exception: ");
            sb.AppendLine(ex.ToString());
            LogInternal(sb.ToString(), TraceEventType.Error);
        }
    }

    public static class LoggerCache
    {
        public static ILogger Logger
        {
            get
            {
                ILogger result = new EnterpriseLogger();
                if (ServiceHelpersConfigSection.Settings != null && 
                    !string.IsNullOrWhiteSpace(ServiceHelpersConfigSection.Settings.Logger))
                {
                    Type t = Type.GetType(ServiceHelpersConfigSection.Settings.Logger);
                    if (t != null)
                        result = Activator.CreateInstance(t) as ILogger;
                }
                return result;
            }
        }
    }
}
