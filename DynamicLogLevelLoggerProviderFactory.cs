namespace WebApplication1
{
    public class DynamicLogLevelLoggerProviderFactory : ILoggerProvider
    {
        private readonly ILoggerProvider _baseLoggerProvider;

        public DynamicLogLevelLoggerProviderFactory(ILoggerProvider baseLoggerProvider)
        {
            _baseLoggerProvider = baseLoggerProvider;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DynamicLogLevelLogger(_baseLoggerProvider.CreateLogger(categoryName));
        }

        public void Dispose()
        {
            _baseLoggerProvider.Dispose();
        }
    }
}
