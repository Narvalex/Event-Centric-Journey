using System;
using System.Data;
using System.Data.SqlClient;

namespace Journey.Database
{
    /// <summary>
    /// Provides usability overloads for <see cref="SqlDataReader"/>.
    /// </summary>
    /// <remarks>
    /// Based on: http://stackoverflow.com/questions/1772025/sql-data-reader-handling-null-column-values
    /// </remarks>
    public static class SqlDataReaderExtensions
    {
        /// <summary>
        /// Gets the value of the specified column as a string in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static string SafeGetString(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetString(i);
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets the value of the specified column as a string in Null-Safe mode. Also trims the string to eliminate white spaces.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static string SafeGetAndTrimString(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetString(i).Trim();
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets the value of the specified column as a an int in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static int SafeGetInt32(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetInt32(i);
            else
                return default(int);
        }

        /// <summary>
        /// Gets the value of the specified column as a long in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static long SafeGetInt64(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetInt64(i);
            else
                return default(int);
        }

        /// <summary>
        /// Gets the value of the specified column as a decimal in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static decimal SafeGetDecimal(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetDecimal(i);
            else
                return default(decimal);
        }

        /// <summary>
        /// Gets the value of the specified column as a float in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static float SafeGetFloat(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetFloat(i);
            else
                return default(float);
        }

        /// <summary>
        /// Gets the value of the specified column as a GUID in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static Guid SafeGetGuid(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetGuid(i);
            else
                return default(Guid);
        }

        /// <summary>
        /// Gets the value of the specified column as a DateTime in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static DateTime SafeGetDateTime(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetDateTime(i);
            else
                return default(DateTime);
        }

        /// <summary>
        /// Gets the value of the specified column as a DateTime in Null-Safe mode.
        /// </summary>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance.</param>
        /// <param name="i">The zero-based column ordinal.</param>
        public static bool SafeGetBool(this IDataReader reader, int i)
        {
            if (!reader.IsDBNull(i))
                return reader.GetBoolean(i);
            else
                return default(bool);
        }
    }
}
