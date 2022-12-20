using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;
using log4net.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;
// ReSharper disable PossibleNullReferenceException

namespace WebApplication1
{
    public class StartupLogging
    {
        const String Prefix = "<";
        const String Suffix = ">k__BackingField";

        private static int _logLevelRestored;
        private static Level _savedLevel;

        private static void ChangeMinimumLevel(ILogger logger, LogLevel level)
        {
            //var _logger = logger.GetType()
            //    .GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance)
            //    .GetValue(logger);

            //var loggersArray = (Array)_logger.GetType().GetProperty("MessageLoggers").GetValue(_logger);
            var loggersArray = (Array)logger.GetType().GetProperty("MessageLoggers").GetValue(logger);


            var enu = loggersArray.GetEnumerator();
            var i = 0;
            while (enu.MoveNext())
            {
                var x = enu.Current;
                var piMinLevel = x.GetType().GetProperty("MinLevel");
                var fiMinLevel = GetBackingField(piMinLevel);
                fiMinLevel.SetValue(x, level);
                loggersArray.SetValue(x, i);
                i++;
            }
        }

        private static FieldInfo GetBackingField(PropertyInfo propertyInfo) {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));
            if (!propertyInfo.CanRead || !propertyInfo.GetGetMethod(nonPublic: true).IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                return null;
            var backingFieldName = GetBackingFieldName(propertyInfo.Name);
            var backingField = propertyInfo.DeclaringType?.GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (backingField == null)
                return null;
            if (!backingField.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                return null;
            return backingField;
        }

        private static String GetBackingFieldName(String propertyName) => $"{Prefix}{propertyName}{Suffix}";

        /// <summary>
        /// Az AddLog4Net sor UTÁN hívjátok meg, másképp hatástalan
        /// </summary>
        /// <param name="logger"></param>
        public static void SetToTraceLevel(ILogger logger)
        {
            _savedLevel = GetRootLogLevel();
            ChangeMinimumLevel(logger, LogLevel.Trace);
            //SetLogLevel(Level.Trace);
        }

        public static void RestoreLogLevel(ILogger logger)
        {
            //Csak egyszer állítjuk vissza
            if (Interlocked.CompareExchange(ref _logLevelRestored, 1, 0) == 0)
            {
                //ChangeMinimumLevel(logger, LogLevel.Information);

                SetLogLevel(_savedLevel);
            }
        }

        private static void SetLogLevel(Level rootLogLevel)
        {
            var repository = ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository());
            repository.Root.Level = rootLogLevel;
            repository.RaiseConfigurationChanged(EventArgs.Empty);
        }

        private static Level GetRootLogLevel()
        {
            var repository = ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository());
            return repository.Root.Level;
        }

    }

}
