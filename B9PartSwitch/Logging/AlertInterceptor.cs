using System;

namespace B9PartSwitch.Logging
{
    public class AlertInterceptor : ILogger
    {
        private readonly ILogger logger;

        public AlertInterceptor(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Log(ILogMessage message)
        {
            if (message.IsFatal)
            {
                FatalErrorHandler.HandleFatalError(message.ToStringShort());
            }
            else if (message.ShouldAlertUser)
            {
                SeriousWarningHandler.DisplaySeriousWarning(message.ToStringShort());
            }

            logger.Log(message);
        }
    }
}
