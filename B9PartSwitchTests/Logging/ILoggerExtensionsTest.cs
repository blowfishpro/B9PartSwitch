using System;
using Xunit;
using NSubstitute;
using B9PartSwitch.Logging;

using LogType = UnityEngine.LogType;

namespace B9PartSwitchTests.Logging
{
    public class ILoggerExtensionsTest
    {
        private readonly ILogger logger = Substitute.For<ILogger>();

        [Fact]
        public void TestInfo()
        {
            LogMessage message = null;
            logger.When(x => x.Log(Arg.Any<LogMessage>())).Do(x => message = x.ArgAt<LogMessage>(0));

            logger.Info("a message");

            Assert.NotNull(message);
            Assert.Equal(LogType.Log, message.UnityLogType);
            Assert.False(message.ShouldAlertUser);
            Assert.False(message.IsFatal);
            Assert.Equal("a message", message.ToString());
        }

        [Fact]
        public void TestWarning()
        {
            LogMessage message = null;
            logger.When(x => x.Log(Arg.Any<LogMessage>())).Do(x => message = x.ArgAt<LogMessage>(0));

            logger.Warning("a message");

            Assert.NotNull(message);
            Assert.Equal(LogType.Warning, message.UnityLogType);
            Assert.False(message.ShouldAlertUser);
            Assert.False(message.IsFatal);
            Assert.Equal("a message", message.ToString());
        }

        [Fact]
        public void TestError()
        {
            LogMessage message = null;
            logger.When(x => x.Log(Arg.Any<LogMessage>())).Do(x => message = x.ArgAt<LogMessage>(0));

            logger.Error("a message");

            Assert.NotNull(message);
            Assert.Equal(LogType.Error, message.UnityLogType);
            Assert.False(message.ShouldAlertUser);
            Assert.False(message.IsFatal);
            Assert.Equal("a message", message.ToString());
        }

        [Fact]
        public void TestAlertError()
        {
            LogMessage message = null;
            logger.When(x => x.Log(Arg.Any<LogMessage>())).Do(x => message = x.ArgAt<LogMessage>(0));

            logger.AlertError("a message");

            Assert.NotNull(message);
            Assert.Equal(LogType.Error, message.UnityLogType);
            Assert.True(message.ShouldAlertUser);
            Assert.False(message.IsFatal);
            Assert.Equal("a message", message.ToString());
        }

        [Fact]
        public void TestException()
        {
            ExceptionMessage message = null;
            logger.When(x => x.Log(Arg.Any<ExceptionMessage>())).Do(x => message = x.ArgAt<ExceptionMessage>(0));

            Exception ex = new Exception("some stuff");
            logger.Exception(ex);

            Assert.NotNull(message);
            Assert.Equal(LogType.Exception, message.UnityLogType);
            Assert.False(message.ShouldAlertUser);
            Assert.False(message.IsFatal);
            Assert.Equal(ex.ToString(), message.ToString());
        }

        [Fact]
        public void TestFatalException()
        {
            ExceptionMessage message = null;
            logger.When(x => x.Log(Arg.Any<ExceptionMessage>())).Do(x => message = x.ArgAt<ExceptionMessage>(0));

            Exception ex = new Exception("some stuff");
            logger.FatalException(ex);

            Assert.NotNull(message);
            Assert.Equal(LogType.Exception, message.UnityLogType);
            Assert.False(message.ShouldAlertUser);
            Assert.True(message.IsFatal);
            Assert.Equal(ex.ToString(), message.ToString());
        }
    }
}
