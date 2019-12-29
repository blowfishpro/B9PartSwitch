using System;
using Xunit;
using NSubstitute;
using UnityEngine;
using B9PartSwitch.Logging;

namespace B9PartSwitchTests.Logging
{
    public class PrefixedMessageTest
    {
        [Fact]
        public void TestConstructor__PrefixNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new PrefixedMessage(null, Substitute.For<ILogMessage>());
            });
        }

        [Fact]
        public void TestConstructor__PrefixEmpty()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(delegate
            {
                new PrefixedMessage("", Substitute.For<ILogMessage>());
            });

            Assert.StartsWith("can't be empty", ex.Message);
        }

        [Fact]
        public void TestConstructor__LogMessageNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new PrefixedMessage("stuff", null);
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
            ILogMessage logMessage = Substitute.For<ILogMessage>();
            logMessage.UnityLogType.Returns(logType);
            PrefixedMessage prefixedMessage = new PrefixedMessage("prefix", logMessage);
            Assert.Equal(logType, prefixedMessage.UnityLogType);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void TestShouldAlertUser(bool shouldAlertUser)
        {
            ILogMessage logMessage = Substitute.For<ILogMessage>();
            logMessage.ShouldAlertUser.Returns(shouldAlertUser);
            PrefixedMessage prefixedMessage = new PrefixedMessage("prefix", logMessage);
            Assert.Equal(shouldAlertUser, prefixedMessage.ShouldAlertUser);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void TestIsFatal(bool isFatal)
        {
            ILogMessage logMessage = Substitute.For<ILogMessage>();
            logMessage.IsFatal.Returns(isFatal);
            PrefixedMessage prefixedMessage = new PrefixedMessage("prefix", logMessage);
            Assert.Equal(isFatal, prefixedMessage.IsFatal);
        }

        [Fact]
        public void TestToString()
        {
            ILogMessage logMessage = Substitute.For<ILogMessage>();
            logMessage.ToString().Returns("a message");
            PrefixedMessage prefixedMessage = new PrefixedMessage("prefix", logMessage);
            Assert.Equal("[prefix] a message", prefixedMessage.ToString());
        }

        [Fact]
        public void TestToStringShort()
        {
            ILogMessage logMessage = Substitute.For<ILogMessage>();
            logMessage.ToString().Returns("a message");
            PrefixedMessage prefixedMessage = new PrefixedMessage("prefix", logMessage);
            Assert.Equal("a message", prefixedMessage.ToStringShort());
        }
    }
}
