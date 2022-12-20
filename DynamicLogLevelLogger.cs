namespace WebApplication1;

public class DynamicLogLevelLogger : ILogger
{
    public static DynamicLogLevelLogger Instance { get; private set; }

    private static readonly object ForLock = new();
    private static LogLevel _minLevelToAllowGlobal = LogLevel.Trace;
    private static bool _globalLevelAlreadySet;

    private static readonly ThreadLocal<LogLevel> MinLevelToAllowPerThread = new(() => LogLevel.Information);

    public static IDisposable SetMinLevelForCurrentThread(LogLevel level)
    {
        var resetDisposer = new LoggerLevelResetDisposer(MinLevelToAllowPerThread.Value, 
            logLevel => MinLevelToAllowPerThread.Value = logLevel);
        
        MinLevelToAllowPerThread.Value = level;

        return resetDisposer;
    }

    private class LoggerLevelResetDisposer : IDisposable
    {
        private readonly LogLevel _savedLogLevel;
        private readonly Action<LogLevel> _resetAction;

        public LoggerLevelResetDisposer(LogLevel savedLogLevel, Action<LogLevel> resetAction)
        {
            _savedLogLevel = savedLogLevel;
            _resetAction = resetAction ?? throw new ArgumentNullException(nameof(resetAction));
        }

        public void Dispose()
        {
            _resetAction(_savedLogLevel);
        }
    }

    public static void SetMinLevelGloballyOnce(LogLevel level)
    {
        lock (ForLock)
        {
            if (!_globalLevelAlreadySet)
            {
                _minLevelToAllowGlobal = level;
                _globalLevelAlreadySet = true;
            }
        }
    }

    private readonly ILogger _baseLogger;

    public DynamicLogLevelLogger(ILogger baseLogger)
    {
        Instance = this;
        _baseLogger = baseLogger;
    }

    public IDisposable BeginScope<TState>(TState state) => _baseLogger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _baseLogger.IsEnabled(logLevel) && IsEnabledImpl(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (IsEnabledImpl(logLevel))
        {
            _baseLogger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    private static bool IsEnabledImpl(LogLevel logLevel)
    {
        bool enabledGlobally;
        lock (ForLock)
        {
            enabledGlobally = logLevel >= _minLevelToAllowGlobal;
        }

        return enabledGlobally || logLevel >= MinLevelToAllowPerThread.Value;
    }
}