using System;
using UnityEngine;

namespace B9PartSwitch.Logging
{
    public class ExceptionMessage : ILogMessage
    {
        private readonly Exception exception;

        public ExceptionMessage(bool shouldAlertUser, bool isFatal, Exception exception)
        {
            ShouldAlertUser = shouldAlertUser;
            IsFatal = isFatal;
            this.exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        public LogType UnityLogType => LogType.Exception;

        public bool ShouldAlertUser { get; }

        public bool IsFatal { get; }

        public override string ToString() => exception.ToString();

        public string ToStringShort()
        {
            string messageStr = exception.Message;
            Exception innerException = exception.InnerException;
            while (innerException != null)
            {
                messageStr += "\n  ";
                messageStr += innerException.Message;
                innerException = innerException.InnerException;
            }
            return messageStr;
        }
    }
}
