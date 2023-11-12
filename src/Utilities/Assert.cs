using Serilog.Events;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UIXTool.Utilities
{
    internal class Assert
    {
        private static void Log(bool condition, LogEventLevel level, string message, params object[] properties)
        {
            if (condition)
                return;

            Serilog.Log.Write(level, message, properties);
        }

        public static void LogFatal(bool condition, string message, params object[] properties)
        {
            Log(condition, LogEventLevel.Fatal, message, properties);
        }

        public static void LogError(bool condition, string message, params object[] properties)
        {
            Log(condition, LogEventLevel.Error, message, properties);
        }

        public static void LogWarning(bool condition, string message, params object[] properties)
        {
            Log(condition, LogEventLevel.Warning, message, properties);
        }

        public static void LogInfo(bool condition, string message, params object[] properties)
        {
            Log(condition, LogEventLevel.Information, message, properties);
        }

        public static void LogDebug(bool condition, string message, params object[] properties)
        {
            Log(condition, LogEventLevel.Debug, message, properties);
        }

        public static void LogVerbose(bool condition, string message, params object[] properties)
        {
            Log(condition, LogEventLevel.Verbose, message, properties);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Throw(bool condition)
        {
            if (condition)
                return;

            throw new Exception();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Throw(bool condition, string message, params object[] properties)
        {
            if (condition)
                return;

            throw new Exception(string.Format(message, properties));
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Throw<T>(bool condition) where T : Exception, new()
        {
            if (condition)
                return;

            throw new T();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Throw<T>(bool condition, string message, params object[] properties) where T : Exception, new()
        {
            if (condition)
                return;

            // manually match against the base Exception method that allows specifying a message
            var ctor = typeof(T).GetConstructor(new[] { typeof(string), typeof(Exception) });
            if (ctor != null)
            {
                throw (T)ctor.Invoke(new object?[] { string.Format(message, properties), null});
            }
            else
            {
                throw (T)(Activator.CreateInstance(typeof(T)) ?? new T());
            }
        }
    }
}
