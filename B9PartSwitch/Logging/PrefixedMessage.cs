using System;
using UnityEngine;

namespace B9PartSwitch.Logging
{
    public class PrefixedMessage : ILogMessage
    {
        private readonly string prefix;
        private readonly ILogMessage logMessage;

        public PrefixedMessage(string prefix, ILogMessage logMessage)
        {
            this.prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            if (prefix == "") throw new ArgumentException("can't be empty", nameof(prefix));
            this.logMessage = logMessage ?? throw new ArgumentNullException(nameof(logMessage));
        }

        public LogType UnityLogType => logMessage.UnityLogType;

        public bool ShouldAlertUser => logMessage.ShouldAlertUser;

        public bool IsFatal => logMessage.IsFatal;

        public override string ToString() => $"[{prefix}] {logMessage.ToString()}";

        public string ToStringShort() => logMessage.ToString();
    }
}
