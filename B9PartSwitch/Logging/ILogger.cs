using System;

namespace B9PartSwitch.Logging
{
    public interface ILogger
    {
        void Log(ILogMessage message);
    }
}
