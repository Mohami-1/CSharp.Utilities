using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Utilities
{
    /// <summary>
    /// A utility class for tracking the performance of method executions.
    /// <para>This class automatically logs the method name, file path, and execution time when used in a using statement.</para>
    /// <para>Example usage:</para>
    /// <para>using (new PerformanceTracker()) { /* code to measure */ }</para>
    /// </summary>
    public class PerformanceTracker : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _methodName;
        private readonly string _filePath;
        private readonly int _lineNumber;
        private readonly string _category;
        private readonly Action<string, TimeSpan, string, string, int> _customLogger;
        private readonly bool _shouldLog;
        private readonly bool _includeMemoryUsage;
        private readonly long _startMemory;

        #region Constructors

        /// <summary>
        /// Creates a new instance of the PerformanceTracker.
        /// <para>Automatically captures the caller's method name and file information.</para>
        /// </summary>
        /// <param name="shouldLog">Whether to log the performance information (default: true).</param>
        /// <param name="includeMemoryUsage">Whether to include memory usage in the log (default: false).</param>
        /// <param name="category">Optional category for grouping related performance logs.</param>
        /// <param name="customLogger">Optional custom logging action to override the default ConsoleLogger.</param>
        /// <param name="callerMemberName">Automatically populated with the caller's method name.</param>
        /// <param name="callerFilePath">Automatically populated with the caller's file path.</param>
        /// <param name="callerLineNumber">Automatically populated with the caller's line number.</param>
        public PerformanceTracker(
            bool shouldLog = true,
            bool includeMemoryUsage = false,
            string category = null,
            Action<string, TimeSpan, string, string, int> customLogger = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0
        )
        {
            _shouldLog = shouldLog;
            _methodName = callerMemberName;
            _filePath = Path.GetFileName(callerFilePath);
            _lineNumber = callerLineNumber;
            _category = category;
            _customLogger = customLogger;
            _includeMemoryUsage = includeMemoryUsage;

            if (_includeMemoryUsage)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                _startMemory = GC.GetTotalMemory(true);
            }

            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Creates a new instance of the PerformanceTracker with a specific method name.
        /// <para>Use this constructor when you want to provide a custom name rather than the automatic method name.</para>
        /// </summary>
        /// <param name="methodName">The custom method name to log.</param>
        /// <param name="shouldLog">Whether to log the performance information (default: true).</param>
        /// <param name="includeMemoryUsage">Whether to include memory usage in the log (default: false).</param>
        /// <param name="category">Optional category for grouping related performance logs.</param>
        /// <param name="customLogger">Optional custom logging action to override the default ConsoleLogger.</param>
        /// <param name="callerFilePath">Automatically populated with the caller's file path.</param>
        /// <param name="callerLineNumber">Automatically populated with the caller's line number.</param>
        public PerformanceTracker(
            string methodName,
            bool shouldLog = true,
            bool includeMemoryUsage = false,
            string category = null,
            Action<string, TimeSpan, string, string, int> customLogger = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0
        )
        {
            _shouldLog = shouldLog;
            _methodName = methodName;
            _filePath = Path.GetFileName(callerFilePath);
            _lineNumber = callerLineNumber;
            _category = category;
            _customLogger = customLogger;
            _includeMemoryUsage = includeMemoryUsage;

            if (_includeMemoryUsage)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                _startMemory = GC.GetTotalMemory(true);
            }

            _stopwatch = Stopwatch.StartNew();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the stopwatch to start timing again from zero.
        /// </summary>
        public void Reset()
        {
            _stopwatch.Restart();

            if (_includeMemoryUsage)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        /// <summary>
        /// Gets the current elapsed time without stopping the tracker.
        /// </summary>
        /// <returns>The current elapsed time.</returns>
        public TimeSpan GetElapsedTime()
        {
            return _stopwatch.Elapsed;
        }

        /// <summary>
        /// Logs the current performance information without stopping the tracker.
        /// </summary>
        /// <param name="checkpoint">An optional name for this checkpoint.</param>
        public void LogCheckpoint(string checkpoint = null)
        {
            if (!_shouldLog)
                return;

            TimeSpan elapsed = _stopwatch.Elapsed;
            string checkpointText = string.IsNullOrEmpty(checkpoint)
                ? string.Empty
                : $" - Checkpoint: {checkpoint}";

            if (_customLogger != null)
            {
                _customLogger(
                    _methodName + checkpointText,
                    elapsed,
                    _category,
                    _filePath,
                    _lineNumber
                );
            }
            else
            {
                LogPerformanceInfo(_methodName + checkpointText, elapsed, _category);
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Stops the performance tracker and logs the performance information.
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();

            if (!_shouldLog)
                return;

            TimeSpan elapsed = _stopwatch.Elapsed;

            if (_customLogger != null)
            {
                _customLogger(_methodName, elapsed, _category, _filePath, _lineNumber);
            }
            else
            {
                LogPerformanceInfo(_methodName, elapsed, _category);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Default implementation for logging performance information.
        /// </summary>
        /// <param name="methodName">The name of the method being tracked.</param>
        /// <param name="elapsed">The elapsed time.</param>
        /// <param name="category">Optional category for the log.</param>
        private void LogPerformanceInfo(string methodName, TimeSpan elapsed, string category)
        {
            string timeInfo = FormatTime(elapsed);
            string memoryInfo = string.Empty;

            if (_includeMemoryUsage)
            {
                long endMemory = GC.GetTotalMemory(false);
                long memoryDiff = endMemory - _startMemory;
                memoryInfo = $", Memory: {FormatBytes(memoryDiff)}";
            }

            string categoryInfo = string.IsNullOrEmpty(category) ? string.Empty : $" [{category}]";

            string logMessage =
                $"Performance{categoryInfo}: {methodName} in {timeInfo}{memoryInfo} [File: {_filePath}, Line: {_lineNumber}]";

            // Try to use ConsoleLogger if available, otherwise fall back to Console.WriteLine
            try
            {
                // Use reflection to find and invoke ConsoleLogger.LogInfo if it exists
                Type consoleLoggerType = Type.GetType("Utilities.ConsoleLogger, Utilities");
                if (consoleLoggerType != null)
                {
                    MethodInfo logMethod = consoleLoggerType.GetMethod(
                        "LogInfo",
                        new[] { typeof(string) }
                    );
                    if (logMethod != null)
                    {
                        _ = logMethod.Invoke(null, new object[] { logMessage });
                        return;
                    }
                }
            }
            catch
            {
                // If reflection fails, fall back to Console.WriteLine
            }

            // Fallback logging
            Console.WriteLine(logMessage);
        }

        /// <summary>
        /// Formats a TimeSpan into a human-readable string.
        /// </summary>
        /// <param name="timeSpan">The TimeSpan to format.</param>
        /// <returns>A formatted time string.</returns>
        private static string FormatTime(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 1)
                return $"{timeSpan.TotalMilliseconds:F2} ms";
            if (timeSpan.TotalMinutes < 1)
                return $"{timeSpan.TotalSeconds:F2} s";
            if (timeSpan.TotalHours < 1)
                return $"{timeSpan.TotalMinutes:F2} min";

            return $"{timeSpan.TotalHours:F2} h";
        }

        /// <summary>
        /// Formats a byte count into a human-readable string.
        /// </summary>
        /// <param name="bytes">The number of bytes.</param>
        /// <returns>A formatted byte string.</returns>
        private static string FormatBytes(long bytes)
        {
            string sign = bytes < 0 ? "-" : "+";
            bytes = Math.Abs(bytes);

            if (bytes < 1024)
                return $"{sign}{bytes} B";
            if (bytes < 1024 * 1024)
                return $"{sign}{bytes / 1024.0:F2} KB";
            if (bytes < 1024 * 1024 * 1024)
                return $"{sign}{bytes / (1024.0 * 1024.0):F2} MB";

            return $"{sign}{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }

        #endregion
    }
}
