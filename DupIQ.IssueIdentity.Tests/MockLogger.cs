﻿using Microsoft.Extensions.Logging;

namespace DupIQ.IssueIdentity.Tests
{
	internal class MockLogger : ILogger
	{
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{

		}
	}
}
