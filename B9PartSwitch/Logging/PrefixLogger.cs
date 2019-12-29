using System;

namespace B9PartSwitch.Logging
{
    public class PrefixLogger : ILogger
    {
        private readonly ILogger logger;
        private readonly string prefix;

        public PrefixLogger(ILogger logger, string prefix)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (prefix == "") throw new ArgumentException("can't be empty", nameof(prefix));
            this.prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        }

        public void Log(ILogMessage message)
        {
            logger.Log(new PrefixedMessage(prefix, message));
        }
    }
}
