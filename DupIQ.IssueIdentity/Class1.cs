using Microsoft.Extensions.Logging;

namespace DupIQ.IssueIdentity
{
	public class Class1
	{
		private ILogger logger;

		public Class1(ILogger logger)
		{
			this.logger = logger;
		}
	}

	public class logthing : ILogger
	{
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			throw new NotImplementedException();
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			throw new NotImplementedException();
		}
	}
}