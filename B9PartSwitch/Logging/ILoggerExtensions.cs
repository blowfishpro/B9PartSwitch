using System;
using UnityEngine;

namespace B9PartSwitch.Logging
{
    public static class ILoggerExtensions
    {
        public static void Info(this ILogger logger, object message) => logger.Log(new LogMessage(LogType.Log, false, false, message));
        public static void Warning(this ILogger logger, object message) => logger.Log(new LogMessage(LogType.Warning, false, false, message));
        public static void Error(this ILogger logger, object message) => logger.Log(new LogMessage(LogType.Error, false, false, message));
        public static void AlertError(this ILogger logger, object message) => logger.Log(new LogMessage(LogType.Error, true, false, message));
        public static void Exception(this ILogger logger, Exception exception) => logger.Log(new ExceptionMessage(false, false, exception));
        public static void FatalException(this ILogger logger, Exception exception) => logger.Log(new ExceptionMessage(false, true, exception));
    }
}
