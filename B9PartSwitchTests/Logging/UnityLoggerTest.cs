using System;
using Xunit;
using NSubstitute;
using UnityEngine;
using B9PartSwitch.Logging;

namespace B9PartSwitchTests.Logging
{
    public class UnityLoggerTest
    {
        [Fact]
        public void TestConstructor__LoggerNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new UnityLogger(null);
            });
        }

        [Fact]
        public void TestLog()
        {
            ILogMessage message = Substitute.For<ILogMessage>();
            message.UnityLogType.Returns(LogType.Assert);
            UnityEngine.ILogger unityEngineLogger = Substitute.For<UnityEngine.ILogger>();
            UnityLogger logger = new UnityLogger(unityEngineLogger);
            logger.Log(message);
            unityEngineLogger.Received().Log(LogType.Assert, message);
        }
    }
}
