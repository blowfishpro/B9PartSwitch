using System;
using Xunit;
using NSubstitute;
using B9PartSwitch.Logging;

namespace B9PartSwitchTests.Logging
{
    public class PrefixLoggerTest
    {
        [Fact]
        public void TestConstructor__LoggerNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new PrefixLogger(null, "prefix");
            });
        }

        [Fact]
        public void TestConstructor__PrefixNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new PrefixLogger(Substitute.For<ILogger>(), null);
            });
        }

        [Fact]
        public void TestConstructor__PrefixEmpty()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(delegate
            {
                new PrefixLogger(Substitute.For<ILogger>(), "");
            });

            Assert.StartsWith("can't be empty", ex.Message);
        }

        [Fact]
        public void TestLog()
        {
            ILogger logger = Substitute.For<ILogger>();
            ILogMessage message = Substitute.For<ILogMessage>();
            message.ToString().Returns("a message");
            PrefixedMessage testLogMessage = null;
            logger.When(x => x.Log(Arg.Any<PrefixedMessage>())).Do(x => testLogMessage = x.ArgAt<PrefixedMessage>(0));

            PrefixLogger prefixLogger = new PrefixLogger(logger, "prefix");
            prefixLogger.Log(message);

            Assert.NotNull(testLogMessage);
            logger.Received().Log(testLogMessage);
            Assert.Equal("[prefix] a message", testLogMessage.ToString());
        }
    }
}
