﻿namespace Divergic.Logging.Xunit.UnitTests
{
    using System;
    using FluentAssertions;
    using global::Xunit;
    using Microsoft.Extensions.Logging;

    public class FilterLoggerTests
    {
        [Fact]
        public void FormatMessageThrowsExceptionWithNullFormatter()
        {
            var state = Guid.NewGuid().ToString();

            var sut = new IsEnabledWrapper();

            Action action = () => sut.RunFormatMessage(state, null, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void LogDoesNotWriteNullFormattedMessage()
        {
            var state = Guid.NewGuid().ToString();

            var sut = new IsEnabledWrapper();

            sut.Log(LogLevel.Critical, default, state, null, (data, ex) => "[null]");

            sut.LogWritten.Should().BeFalse();
        }

        [Fact]
        public void LogDoesNotWriteWhenExceptionAndMessageAreNull()
        {
            var sut = new IsEnabledWrapper();

            sut.Log<string>(LogLevel.Critical, default, null, null, (data, ex) => null);

            sut.LogWritten.Should().BeFalse();
        }

        [Fact]
        public void LogThrowsExceptionWithNullFormatter()
        {
            var state = Guid.NewGuid().ToString();
            var exception = new TimeoutException();

            var sut = new IsEnabledWrapper();

            Action action = () => sut.Log(LogLevel.Critical, default, state, exception, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void LogWritesException()
        {
            var state = Guid.NewGuid().ToString();
            var exception = new TimeoutException();

            var sut = new IsEnabledWrapper();

            sut.Log(LogLevel.Critical, default, state, exception, (data, ex) => null);

            sut.LogWritten.Should().BeTrue();
        }

        [Fact]
        public void LogWritesFormattedMessage()
        {
            var state = Guid.NewGuid().ToString();
            var exception = new TimeoutException();
            var message = Guid.NewGuid().ToString();

            var sut = new MessageWrapper();

            sut.Log(LogLevel.Critical, default, state, exception, (data, ex) => message);

            sut.Message.Should().Be(message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LogWritesMessageBasedOnIsEnabled(bool isEnabled)
        {
            var message = Guid.NewGuid().ToString();

            var sut = new IsEnabledWrapper(isEnabled);

            sut.LogInformation(message);

            sut.LogWritten.Should().Be(isEnabled);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LogWritesMessageBasedOnShouldFilter(bool shouldFilter)
        {
            var message = Guid.NewGuid().ToString();

            var sut = new ShouldFilterWrapper(shouldFilter);

            sut.LogInformation(message);

            sut.LogWritten.Should().Be(shouldFilter == false);
        }

        private class IsEnabledWrapper : FilterLogger
        {
            private readonly bool _isEnabled;

            public IsEnabledWrapper(bool isEnabled = true)
            {
                _isEnabled = isEnabled;
            }

            public override IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public override bool IsEnabled(LogLevel logLevel)
            {
                return _isEnabled;
            }

            public string RunFormatMessage<T>(T state, Exception exception, Func<T, Exception, string> formatter)
            {
                return FormatMessage(state, exception, formatter);
            }

            protected override void WriteLogEntry<TState>(LogLevel logLevel, EventId eventId, TState state,
                string message, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                LogWritten = true;
            }

            public bool LogWritten { get; set; }
        }

        private class MessageWrapper : FilterLogger
        {
            public override IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public override bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            protected override void WriteLogEntry<TState>(LogLevel logLevel, EventId eventId, TState state,
                string message, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                Message = message;
            }

            public string Message { get; set; }
        }

        private class ShouldFilterWrapper : FilterLogger
        {
            private readonly bool _shouldFilter;

            public ShouldFilterWrapper(bool shouldFilter = true)
            {
                _shouldFilter = shouldFilter;
            }

            public override IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public override bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public string RunFormatMessage<T>(T state, Exception exception, Func<T, Exception, string> formatter)
            {
                return FormatMessage(state, exception, formatter);
            }

            protected override bool ShouldFilter(string message, Exception exception)
            {
                return _shouldFilter;
            }

            protected override void WriteLogEntry<TState>(LogLevel logLevel, EventId eventId, TState state,
                string message, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                LogWritten = true;
            }

            public bool LogWritten { get; set; }
        }
    }
}