using System;

namespace B9PartSwitch.Logging
{
    public interface ILogMessage
    {
        UnityEngine.LogType UnityLogType { get; }
        bool ShouldAlertUser { get; }
        bool IsFatal { get; }

        string ToString();

        string ToStringShort();
    }
}
