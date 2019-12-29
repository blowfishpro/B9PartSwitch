using System;
using Xunit;
using UnityEngine;
using B9PartSwitch.Logging;

namespace B9PartSwitchTests.Logging
{
    public class LogMessageTest
    {
        [Fact]
        public void TestConstructor__MessageNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new LogMessage(LogType.Assert, false, false, null);
            });
        }

        [InlineData(LogType.Error)]
        [InlineData(LogType.Assert)]
        [InlineData(LogType.Warning)]
        [InlineData(LogType.Log)]
        [InlineData(LogType.Exception)]
        [Theory]
        public void TestUnityLogType(LogType logType)
        {
            LogMessage message = new LogMessage(logType, false, false, new object());
            Assert.Equal(logType, message.UnityLogType);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void TestShouldAlertUser(bool shouldAlertUser)
        {
            LogMessage message = new LogMessage(LogType.Assert, shouldAlertUser, false, new object());
            Assert.Equal(shouldAlertUser, message.ShouldAlertUser);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void TestIsFatal(bool isFatal)
        {
            LogMessage message = new LogMessage(LogType.Assert, false, isFatal, new object());
            Assert.Equal(isFatal, message.IsFatal);
        }

        [Fact]
        public void TestToString()
        {
            LogMessage message = new LogMessage(LogType.Assert, false, false, "a message");
            Assert.Equal("a message", message.ToString());
        }

        [Fact]
        public void TestToStringShort()
        {
            LogMessage message = new LogMessage(LogType.Assert, false, false, "a message");
            Assert.Equal("a message", message.ToStringShort());
        }
    }
}
