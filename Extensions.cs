using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utilities
{
    /// <summary>
    /// Provides extension methods for various utility operations.
    /// </summary>
    public static class Extensions
    {
        #region Collection Extensions

        /// <summary>
        /// Converts a collection of items to a concatenated string representation.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The collection to convert.</param>
        /// <param name="separator">The separator to use between items (default: ", ").</param>
        /// <param name="prefix">The prefix for the string (default: "[").</param>
        /// <param name="suffix">The suffix for the string (default: "]").</param>
        /// <param name="nullRepresentation">The string to use for null items (default: "null").</param>
        /// <returns>A string representation of the collection.</returns>
        public static string ToConcatenatedString<T>(
            this IEnumerable<T> collection,
            string separator = ", ",
            string prefix = "[",
            string suffix = "]",
            string nullRepresentation = "null"
        )
        {
            if (collection == null)
                return $"{prefix}{suffix}";

            var stringBuilder = new StringBuilder(prefix);
            bool isFirst = true;

            foreach (var item in collection)
            {
                if (!isFirst)
                    stringBuilder.Append(separator);

                stringBuilder.Append(item == null ? nullRepresentation : item.ToString());
                isFirst = false;
            }

            stringBuilder.Append(suffix);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Converts a collection to a pretty-formatted JSON string.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The collection to convert.</param>
        /// <param name="indented">Whether to format the JSON with indentation (default: true).</param>
        /// <returns>A JSON string representation of the collection.</returns>
        public static string ToJson<T>(this IEnumerable<T> collection, bool indented = true)
        {
            if (collection == null)
                return "null";

            var options = new JsonSerializerOptions { WriteIndented = indented };

            return JsonSerializer.Serialize(collection, options);
        }

        /// <summary>
        /// Performs an action on each item in a collection.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The collection to iterate.</param>
        /// <param name="action">The action to perform on each item.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection == null || action == null)
                return;

            foreach (var item in collection)
            {
                action(item);
            }
        }

        /// <summary>
        /// Performs an action on each item in a collection with its index.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The collection to iterate.</param>
        /// <param name="action">The action to perform on each item and its index.</param>
        public static void ForEachWithIndex<T>(
            this IEnumerable<T> collection,
            Action<T, int> action
        )
        {
            if (collection == null || action == null)
                return;

            int index = 0;
            foreach (var item in collection)
            {
                action(item, index++);
            }
        }

        /// <summary>
        /// Returns a random item from a collection.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The collection to get a random item from.</param>
        /// <returns>A random item from the collection.</returns>
        /// <exception cref="ArgumentException">Thrown when the collection is empty.</exception>
        public static T RandomItem<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var list = collection as IList<T> ?? collection.ToList();
            if (list.Count == 0)
                throw new ArgumentException(
                    "Cannot select a random item from an empty collection."
                );

            Random random = new Random();
            int index = random.Next(0, list.Count);
            return list[index];
        }

        /// <summary>
        /// Checks if a collection is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The collection to check.</param>
        /// <returns>True if the collection is null or empty, otherwise false.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Returns distinct elements from a collection based on a specific key.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <typeparam name="TKey">The type of the key used for comparison.</typeparam>
        /// <param name="collection">The collection to filter.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns>A collection with distinct elements based on the specified key.</returns>
        public static IEnumerable<T> DistinctBy<T, TKey>(
            this IEnumerable<T> collection,
            Func<T, TKey> keySelector
        )
        {
            if (collection == null || keySelector == null)
                yield break;

            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (var item in collection)
            {
                if (seenKeys.Add(keySelector(item)))
                {
                    yield return item;
                }
            }
        }

        #endregion

        #region String Extensions

        /// <summary>
        /// Truncates a string to a specified maximum length with optional ellipsis.
        /// </summary>
        /// <param name="str">The string to truncate.</param>
        /// <param name="maxLength">The maximum length of the truncated string.</param>
        /// <param name="addEllipsis">Whether to add ellipsis (...) at the end of truncated string (default: true).</param>
        /// <returns>The truncated string.</returns>
        public static string Truncate(this string str, int maxLength, bool addEllipsis = true)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
                return str;

            string ellipsis = addEllipsis ? "..." : string.Empty;
            int truncateLength = maxLength - ellipsis.Length;
            if (truncateLength <= 0)
                return ellipsis.Substring(0, maxLength);

            return str.Substring(0, truncateLength) + ellipsis;
        }

        /// <summary>
        /// Checks if a string is null, empty, or consists only of whitespace.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <returns>True if the string is null, empty, or whitespace, otherwise false.</returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Converts a string to title case.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>The string in title case.</returns>
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Removes all HTML tags from a string.
        /// </summary>
        /// <param name="html">The HTML string to clean.</param>
        /// <returns>The string with HTML tags removed.</returns>
        public static string StripHtml(this string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;

            return Regex.Replace(html, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Reverses a string.
        /// </summary>
        /// <param name="str">The string to reverse.</param>
        /// <returns>The reversed string.</returns>
        public static string Reverse(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Converts a string to a camelCase identifier.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>The string in camelCase.</returns>
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            // Replace non-alphanumeric chars with spaces
            string processed = Regex.Replace(str, @"[^a-zA-Z0-9]", " ");
            // Split by spaces and capitalize each word except the first
            string[] words = processed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
                return string.Empty;

            string firstWord = words[0].ToLower();
            string[] remainingWords = words
                .Skip(1)
                .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower())
                .ToArray();

            return firstWord + string.Join("", remainingWords);
        }

        /// <summary>
        /// Checks if a string contains only numeric characters and returns the parsed value.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="value">When this method returns, contains the numeric value if the conversion succeeded, or the default value if the conversion failed.</param>
        /// <param name="allowDecimal">Whether to allow decimal points (default: false).</param>
        /// <param name="allowNegative">Whether to allow negative sign (default: false).</param>
        /// <returns>True if the string is numeric and was successfully converted, otherwise false.</returns>
        public static bool IsNumeric<T>(
            this string str,
            out T value,
            bool allowDecimal = false,
            bool allowNegative = false
        )
            where T : struct, IConvertible
        {
            value = default;

            if (string.IsNullOrEmpty(str))
                return false;

            // Check if the string follows numeric format
            bool isValidFormat = true;

            // Handle negative sign if allowed
            if (allowNegative && str.StartsWith("-"))
            {
                // If string is just a negative sign, it's not a valid number
                if (str.Length == 1)
                    return false;
            }

            // If we allow decimals, we permit one decimal point
            bool decimalPointFound = false;

            foreach (char c in str)
            {
                if (c == '-' && allowNegative && str.IndexOf(c) == 0)
                    continue;

                if (c == '.' && allowDecimal && !decimalPointFound)
                {
                    decimalPointFound = true;
                    continue;
                }

                if (!char.IsDigit(c))
                {
                    isValidFormat = false;
                    break;
                }
            }

            if (!isValidFormat)
                return false;

            // Try to parse the value based on the requested type
            try
            {
                if (typeof(T) == typeof(int))
                {
                    if (allowDecimal || !int.TryParse(str, out int intValue))
                        return false;

                    value = (T)(object)intValue;
                }
                else if (typeof(T) == typeof(long))
                {
                    if (allowDecimal || !long.TryParse(str, out long longValue))
                        return false;

                    value = (T)(object)longValue;
                }
                else if (typeof(T) == typeof(float))
                {
                    if (!float.TryParse(str, out float floatValue))
                        return false;

                    value = (T)(object)floatValue;
                }
                else if (typeof(T) == typeof(double))
                {
                    if (!double.TryParse(str, out double doubleValue))
                        return false;

                    value = (T)(object)doubleValue;
                }
                else if (typeof(T) == typeof(decimal))
                {
                    if (!decimal.TryParse(str, out decimal decimalValue))
                        return false;

                    value = (T)(object)decimalValue;
                }
                else
                {
                    // For other numeric types, use Convert
                    value = (T)Convert.ChangeType(str, typeof(T));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region DateTime Extensions

        /// <summary>
        /// Formats a DateTime as a user-friendly relative time string (e.g., "2 days ago").
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>A string representing the relative time.</returns>
        public static string ToRelativeTimeString(this DateTime dateTime)
        {
            TimeSpan timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes == 1 ? "" : "s")} ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours == 1 ? "" : "s")} ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays == 1 ? "" : "s")} ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) == 1 ? "" : "s")} ago";

            return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) == 1 ? "" : "s")} ago";
        }

        /// <summary>
        /// Checks if a DateTime is today.
        /// </summary>
        /// <param name="dateTime">The DateTime to check.</param>
        /// <returns>True if the DateTime is today, otherwise false.</returns>
        public static bool IsToday(this DateTime dateTime)
        {
            return dateTime.Date == DateTime.Now.Date;
        }

        /// <summary>
        /// Gets the start of the week for a given DateTime.
        /// </summary>
        /// <param name="dateTime">The DateTime to get the start of the week for.</param>
        /// <param name="startOfWeek">The day considered as the start of the week (default: Sunday).</param>
        /// <returns>A DateTime representing the start of the week.</returns>
        public static DateTime StartOfWeek(
            this DateTime dateTime,
            DayOfWeek startOfWeek = DayOfWeek.Sunday
        )
        {
            int diff = (7 + (dateTime.DayOfWeek - startOfWeek)) % 7;
            return dateTime.AddDays(-diff).Date;
        }

        #endregion

        #region File and Path Extensions

        /// <summary>
        /// Gets a safe file name from a string by removing invalid characters.
        /// </summary>
        /// <param name="str">The string to convert to a safe file name.</param>
        /// <returns>A safe file name.</returns>
        public static string ToSafeFileName(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", str.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries))
                .Replace(" ", "_");
        }

        /// <summary>
        /// Gets the file size as a user-friendly string (e.g., "4.2 MB").
        /// </summary>
        /// <param name="fileInfo">The FileInfo object.</param>
        /// <returns>A user-friendly string representing the file size.</returns>
        public static string GetFileSizeString(this FileInfo fileInfo)
        {
            if (fileInfo == null || !fileInfo.Exists)
                return "0 B";

            long bytes = fileInfo.Length;
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            int counter = 0;
            decimal number = (decimal)bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n1} {suffixes[counter]}";
        }

        #endregion

        #region Async Extensions

        /// <summary>
        /// Executes an asynchronous action with a timeout.
        /// </summary>
        /// <param name="task">The task to execute.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="TimeoutException">Thrown when the operation times out.</exception>
        public static async Task WithTimeout(this Task task, int timeout)
        {
            if (timeout <= 0)
            {
                await task;
                return;
            }

            var timeoutTask = Task.Delay(timeout);
            var completedTask = await Task.WhenAny(task, timeoutTask);

            if (completedTask == timeoutTask)
                throw new TimeoutException($"Operation timed out after {timeout}ms");

            await task; // This will propagate any exceptions from the original task
        }

        /// <summary>
        /// Executes an asynchronous function with a timeout and returns its result.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to execute.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns>The result of the task.</returns>
        /// <exception cref="TimeoutException">Thrown when the operation times out.</exception>
        public static async Task<T> WithTimeout<T>(this Task<T> task, int timeout)
        {
            if (timeout <= 0)
                return await task;

            var timeoutTask = Task.Delay(timeout);
            var completedTask = await Task.WhenAny(task, timeoutTask);

            if (completedTask == timeoutTask)
                throw new TimeoutException($"Operation timed out after {timeout}ms");

            return await task; // This will propagate any exceptions from the original task
        }

        #endregion

        #region Object Extensions

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <param name="indented">Whether to format the JSON with indentation (default: true).</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string ToJson(this object obj, bool indented = true)
        {
            if (obj == null)
                return "null";

            var options = new JsonSerializerOptions { WriteIndented = indented };

            return JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// Tries to cast an object to a specified type, returning a default value if the cast fails.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <param name="obj">The object to cast.</param>
        /// <param name="defaultValue">The default value to return if the cast fails (default: default(T)).</param>
        /// <returns>The cast object or the default value.</returns>
        public static T As<T>(this object obj, T defaultValue = default)
        {
            if (obj is T variable)
                return variable;

            return defaultValue;
        }

        /// <summary>
        /// Creates a deep clone of an object using JSON serialization.
        /// Note: This method works best with simple data objects.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object to clone.</param>
        /// <returns>A deep clone of the object.</returns>
        public static T DeepClone<T>(this T obj)
        {
            if (obj == null)
                return default;

            string json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json);
        }

        #endregion

        #region Numeric Extensions

        /// <summary>
        /// Clamps a value between a minimum and maximum value.
        /// <para>Note: If value is smaller than min and larger than max, min is returned!</para>
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static T Clamp<T>(this T value, T min, T max)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;
            return value;
        }

        /// <summary>
        /// Checks if a number is between two values (inclusive).
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>True if the value is between min and max, otherwise false.</returns>
        public static bool IsBetween<T>(this T value, T min, T max)
            where T : IComparable<T>
        {
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        /// <summary>
        /// Formats a number as a file size string (e.g., "4.2 MB").
        /// </summary>
        /// <param name="bytes">The number of bytes.</param>
        /// <returns>A formatted file size string.</returns>
        public static string ToFileSizeString(this long bytes)
        {
            if (bytes <= 0)
                return "0 B";

            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n1} {suffixes[counter]}";
        }

        #endregion
    }
}
