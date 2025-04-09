using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    /// <summary>
    /// Provides utility methods for logging messages to the console with different colors,
    /// formats, and output options.
    /// </summary>
    public static class ConsoleLogger
    {
        // Default settings
        private static ConsoleColor _defaultColor = ConsoleColor.Gray;
        private static bool _timestampEnabled = false;
        private static string _timestampFormat = "yyyy-MM-dd HH:mm:ss";
        private static LogLevel _minimumLogLevel = LogLevel.Info;
        private static TextWriter _logFile = null;
        private static bool _logToFile = false;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Defines the severity levels for log messages.
        /// </summary>
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Success = 2,
            Warning = 3,
            Error = 4,
            Critical = 5,
        }

        #region Configuration Methods

        /// <summary>
        /// Enables or disables timestamp prefixing for log messages.
        /// </summary>
        /// <param name="enabled">Whether timestamps should be enabled.</param>
        /// <param name="format">Optional custom timestamp format (default: "yyyy-MM-dd HH:mm:ss").</param>
        public static void EnableTimestamps(bool enabled, string format = null)
        {
            _timestampEnabled = enabled;
            if (format != null)
            {
                _timestampFormat = format;
            }
        }

        /// <summary>
        /// Sets the default color to return to after logging a message.
        /// </summary>
        /// <param name="color">The color to use as default.</param>
        public static void SetDefaultColor(ConsoleColor color)
        {
            _defaultColor = color;
            Console.ForegroundColor = color;
        }

        /// <summary>
        /// Sets the minimum log level that will be displayed.
        /// </summary>
        /// <param name="level">The minimum log level to display.</param>
        public static void SetMinimumLogLevel(LogLevel level)
        {
            _minimumLogLevel = level;
        }

        /// <summary>
        /// Enables logging to a file in addition to the console.
        /// </summary>
        /// <param name="filePath">Path to the log file.</param>
        /// <param name="append">Whether to append to an existing file (true) or overwrite it (false).</param>
        /// <returns>True if file logging was successfully enabled, otherwise false.</returns>
        public static bool EnableFileLogging(string filePath, bool append = true)
        {
            try
            {
                _logFile = new StreamWriter(filePath, append, Encoding.UTF8) { AutoFlush = true };
                _logToFile = true;
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to enable file logging: {ex.Message}");
                _logToFile = false;
                return false;
            }
        }

        /// <summary>
        /// Disables logging to file if it was previously enabled.
        /// </summary>
        public static void DisableFileLogging()
        {
            if (_logToFile && _logFile != null)
            {
                _logFile.Flush();
                _logFile.Close();
                _logFile = null;
                _logToFile = false;
            }
        }

        #endregion

        #region Core Logging Methods

        /// <summary>
        /// Core logging method that handles all types of log messages.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The color to use for console output.</param>
        /// <param name="level">The severity level of the message.</param>
        /// <param name="category">Optional category/tag for the log message.</param>
        private static void LogCore(
            string message,
            ConsoleColor color,
            LogLevel level,
            string category = null
        )
        {
            // Skip if below minimum log level
            if (level < _minimumLogLevel)
                return;

            // Build the full log message
            var logBuilder = new StringBuilder();

            // Add timestamp if enabled
            if (_timestampEnabled)
            {
                logBuilder.Append($"[{DateTime.Now.ToString(_timestampFormat)}] ");
            }

            // Add log level
            logBuilder.Append($"[{level.ToString().ToUpper()}] ");

            // Add category if provided
            if (!string.IsNullOrEmpty(category))
            {
                logBuilder.Append($"[{category}] ");
            }

            // Add the actual message
            logBuilder.Append(message);

            string fullMessage = logBuilder.ToString();

            // Thread-safe console output
            lock (_lockObject)
            {
                // Log to console with appropriate color
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(fullMessage);
                Console.ForegroundColor = _defaultColor;

                // Log to file if enabled (without colors)
                if (_logToFile && _logFile != null)
                {
                    _logFile.WriteLine(fullMessage);
                }
            }
        }

        /// <summary>
        /// Logs a message asynchronously, useful for high-volume logging scenarios.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The color to use for console output.</param>
        /// <param name="level">The severity level of the message.</param>
        /// <param name="category">Optional category/tag for the log message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task LogAsync(
            string message,
            ConsoleColor color,
            LogLevel level,
            string category = null
        )
        {
            return Task.Run(() => LogCore(message, color, level, category));
        }

        #endregion

        #region Standard Logging Methods

        /// <summary>
        /// Logs a success message to the console with the color green.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="category">Optional category/tag for the log message.</param>
        public static void LogSuccess(string message, string category = null)
        {
            LogCore(message, ConsoleColor.Green, LogLevel.Success, category);
        }

        /// <summary>
        /// Logs an error message to the console with the color red.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="category">Optional category/tag for the log message.</param>
        public static void LogError(string message, string category = null)
        {
            LogCore(message, ConsoleColor.Red, LogLevel.Error, category);
        }

        /// <summary>
        /// Logs a critical error message to the console with bright red.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="category">Optional category/tag for the log message.</param>
        public static void LogCritical(string message, string category = null)
        {
            // Using DarkRed for better visibility of critical errors
            LogCore(message, ConsoleColor.DarkRed, LogLevel.Critical, category);
        }

        /// <summary>
        /// Logs a warning message to the console with the color yellow.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="category">Optional category/tag for the log message.</param>
        public static void LogWarning(string message, string category = null)
        {
            LogCore(message, ConsoleColor.Yellow, LogLevel.Warning, category);
        }

        /// <summary>
        /// Logs an informational message to the console with the color cyan.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="category">Optional category/tag for the log message.</param>
        public static void LogInfo(string message, string category = null)
        {
            LogCore(message, ConsoleColor.Cyan, LogLevel.Info, category);
        }

        /// <summary>
        /// Logs a debug message to the console with the color gray.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="category">Optional category/tag for the log message.</param>
        public static void LogDebug(string message, string category = null)
        {
            LogCore(message, ConsoleColor.Gray, LogLevel.Debug, category);
        }

        /// <summary>
        /// Logs a message to the console with a specified color.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The color to use for the message.</param>
        /// <param name="level">The severity level of the message (default: Info).</param>
        /// <param name="category">Optional category/tag for the log message.</param>
        public static void LogWithColor(
            string message,
            ConsoleColor color,
            LogLevel level = LogLevel.Info,
            string category = null
        )
        {
            LogCore(message, color, level, category);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Logs an exception to the console with the color red.
        /// </summary>
        /// <param name="e">The exception to log.</param>
        /// <param name="category">Optional category/tag for the log message.</param>
        /// <param name="includeStackTrace">Whether to include the stack trace (default: true).</param>
        public static void LogException(
            Exception e,
            string category = null,
            bool includeStackTrace = true
        )
        {
            string message;
            if (includeStackTrace)
            {
                message = e.ToString();
            }
            else
            {
                message = $"{e.GetType().Name}: {e.Message}";
                if (e.InnerException != null)
                {
                    message += $" (Inner: {e.InnerException.Message})";
                }
            }

            LogCore(message, ConsoleColor.Red, LogLevel.Error, category);
        }

        /// <summary>
        /// Logs only the message of an exception without the stack trace.
        /// </summary>
        /// <param name="e">The exception to log.</param>
        /// <param name="category">Optional category/tag for the log message.</param>
        public static void LogExceptionMessage(Exception e, string category = null)
        {
            LogException(e, category, false);
        }

        #endregion

        #region Special Formatting Methods

        /// <summary>
        /// Logs a section header with a title.
        /// </summary>
        /// <param name="title">The title of the section.</param>
        /// <param name="color">The color to use (default: White).</param>
        public static void LogSectionHeader(string title, ConsoleColor color = ConsoleColor.White)
        {
            int consoleWidth = Math.Min(Console.WindowWidth - 1, 80);
            string separator = new string('-', consoleWidth);

            lock (_lockObject)
            {
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = color;

                Console.WriteLine(separator);
                Console.WriteLine($"| {title.PadRight(consoleWidth - 4)} |");
                Console.WriteLine(separator);

                Console.ForegroundColor = _defaultColor;

                // Log to file if enabled
                if (_logToFile && _logFile != null)
                {
                    _logFile.WriteLine(separator);
                    _logFile.WriteLine($"| {title.PadRight(consoleWidth - 4)} |");
                    _logFile.WriteLine(separator);
                }
            }
        }

        /// <summary>
        /// Logs a table of data with aligned columns.
        /// </summary>
        /// <param name="headers">The column headers.</param>
        /// <param name="rows">The data rows as string arrays.</param>
        /// <param name="color">The color to use for the header (default: White).</param>
        public static void LogTable(
            string[] headers,
            IEnumerable<string[]> rows,
            ConsoleColor color = ConsoleColor.White
        )
        {
            if (headers == null || rows == null)
                return;

            // Calculate column widths
            int[] columnWidths = new int[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                columnWidths[i] = headers[i].Length;
            }

            // Get all rows into a list so we can iterate multiple times
            List<string[]> rowsList = new List<string[]>(rows);

            // Update column widths based on data
            foreach (var row in rowsList)
            {
                if (row.Length != headers.Length)
                    continue;

                for (int i = 0; i < row.Length; i++)
                {
                    columnWidths[i] = Math.Max(columnWidths[i], row[i]?.Length ?? 0);
                }
            }

            // Build header row
            StringBuilder headerBuilder = new StringBuilder("| ");
            StringBuilder separatorBuilder = new StringBuilder("+-");

            for (int i = 0; i < headers.Length; i++)
            {
                headerBuilder.Append(headers[i].PadRight(columnWidths[i]));
                separatorBuilder.Append(new string('-', columnWidths[i]));

                if (i < headers.Length - 1)
                {
                    headerBuilder.Append(" | ");
                    separatorBuilder.Append("-+-");
                }
            }

            headerBuilder.Append(" |");
            separatorBuilder.Append("-+");

            string headerLine = headerBuilder.ToString();
            string separatorLine = separatorBuilder.ToString();

            // Output the table
            lock (_lockObject)
            {
                ConsoleColor previousColor = Console.ForegroundColor;

                // Output header
                Console.ForegroundColor = color;
                Console.WriteLine(separatorLine);
                Console.WriteLine(headerLine);
                Console.WriteLine(separatorLine);
                Console.ForegroundColor = _defaultColor;

                // Output data rows
                foreach (var row in rowsList)
                {
                    if (row.Length != headers.Length)
                        continue;

                    StringBuilder rowBuilder = new StringBuilder("| ");

                    for (int i = 0; i < row.Length; i++)
                    {
                        rowBuilder.Append((row[i] ?? "").PadRight(columnWidths[i]));
                        if (i < row.Length - 1)
                        {
                            rowBuilder.Append(" | ");
                        }
                    }

                    rowBuilder.Append(" |");
                    Console.WriteLine(rowBuilder.ToString());
                }

                // Output footer
                Console.WriteLine(separatorLine);

                // Log to file if enabled
                if (_logToFile && _logFile != null)
                {
                    _logFile.WriteLine(separatorLine);
                    _logFile.WriteLine(headerLine);
                    _logFile.WriteLine(separatorLine);

                    foreach (var row in rowsList)
                    {
                        if (row.Length != headers.Length)
                            continue;

                        StringBuilder rowBuilder = new StringBuilder("| ");

                        for (int i = 0; i < row.Length; i++)
                        {
                            rowBuilder.Append((row[i] ?? "").PadRight(columnWidths[i]));
                            if (i < row.Length - 1)
                            {
                                rowBuilder.Append(" | ");
                            }
                        }

                        rowBuilder.Append(" |");
                        _logFile.WriteLine(rowBuilder.ToString());
                    }

                    _logFile.WriteLine(separatorLine);
                }
            }
        }

        /// <summary>
        /// Logs a progress update with a progress bar.
        /// </summary>
        /// <param name="current">Current progress value.</param>
        /// <param name="total">Total progress value.</param>
        /// <param name="message">Optional message to display with the progress bar.</param>
        /// <param name="color">Color of the progress bar (default: DarkGreen).</param>
        public static void LogProgress(
            int current,
            int total,
            string message = null,
            ConsoleColor color = ConsoleColor.DarkGreen
        )
        {
            if (current < 0)
                current = 0;
            if (total <= 0)
                total = 1;
            if (current > total)
                current = total;

            int percent = (int)((current / (double)total) * 100);
            int barWidth = Math.Min(Console.WindowWidth - 20, 50); // Adjust for console width
            int filledWidth = (int)((current / (double)total) * barWidth);

            StringBuilder progressBar = new StringBuilder("[");
            for (int i = 0; i < barWidth; i++)
            {
                if (i < filledWidth)
                    progressBar.Append("█");
                else
                    progressBar.Append(" ");
            }
            progressBar.Append($"] {percent}%");

            if (!string.IsNullOrEmpty(message))
            {
                progressBar.Append($" - {message}");
            }

            lock (_lockObject)
            {
                ConsoleColor previousColor = Console.ForegroundColor;

                Console.CursorLeft = 0;
                Console.ForegroundColor = color;
                Console.Write(progressBar.ToString());

                // Clear to end of line in case the new text is shorter than previous text
                Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft - 1));

                Console.ForegroundColor = _defaultColor;

                // Log to file if enabled
                if (_logToFile && _logFile != null && current == total)
                {
                    _logFile.WriteLine(progressBar.ToString());
                }
            }
        }

        #endregion

        #region Cleanup and Resource Management

        /// <summary>
        /// Flushes any buffered log data and releases resources.
        /// Call this method before application exit.
        /// </summary>
        public static void Shutdown()
        {
            DisableFileLogging();
        }

        #endregion
    }
}
