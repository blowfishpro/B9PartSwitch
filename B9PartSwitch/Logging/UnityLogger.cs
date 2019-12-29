using System;

namespace B9PartSwitch.Logging
{
    public class UnityLogger : ILogger
    {
        private readonly UnityEngine.ILogger logger;

        public UnityLogger(UnityEngine.ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Log(ILogMessage message)
        {
            logger.Log(message.UnityLogType, message);
        }
    }
}
