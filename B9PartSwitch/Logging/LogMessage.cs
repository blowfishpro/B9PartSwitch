using System;
using UnityEngine;

namespace B9PartSwitch.Logging
{
    public class LogMessage : ILogMessage
    {
        private readonly object message;

        public LogMessage(LogType logType, bool shouldAlertUser, bool isFatal, object message)
        {
            UnityLogType = logType;
            ShouldAlertUser = shouldAlertUser;
            IsFatal = isFatal;
            this.message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public LogType UnityLogType { get; }

        public bool ShouldAlertUser { get; }

        public bool IsFatal { get; }

        public override string ToString() => message.ToString();

        public string ToStringShort() => ToString();
    }
}
