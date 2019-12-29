using System;
using Xunit;
using UnityEngine;
using B9PartSwitch.Logging;

namespace B9PartSwitchTests.Logging
{
    public class ExceptionMessageTest
    {
        [Fact]
        public void TestConstructor__MessageNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ExceptionMessage(false, false, null);
            });
        }

        [Fact]
        public void TestUnityLogType()
        {
            ExceptionMessage message = new ExceptionMessage(false, false, new Exception());
            Assert.Equal(LogType.Exception, message.UnityLogType);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void TestShouldAlertUser(bool shouldAlertUser)
        {
            ExceptionMessage message = new ExceptionMessage(shouldAlertUser, false, new Exception());
            Assert.Equal(shouldAlertUser, message.ShouldAlertUser);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void TestIsFatal(bool isFatal)
        {
            ExceptionMessage message = new ExceptionMessage(false, isFatal, new Exception());
            Assert.Equal(isFatal, message.IsFatal);
        }

        [Fact]
        public void TestToString()
        {
            Exception ex = new Exception("some stuff");
            ExceptionMessage message = new ExceptionMessage(false, false, ex);
            Assert.Equal(ex.ToString(), message.ToString());
        }

        [Fact]
        public void TestToStringShort()
        {
            Exception ex = new Exception("some stuff");
            ExceptionMessage message = new ExceptionMessage(false, false, ex);
            Assert.Equal("some stuff", message.ToStringShort());
        }
    }
}
