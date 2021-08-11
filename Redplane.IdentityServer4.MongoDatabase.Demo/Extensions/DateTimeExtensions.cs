using System;
using System.Globalization;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        ///     Calculate the unix time from UTC DateTime.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static double ToUnixTime(this DateTime dateTime)
        {
            return (dateTime - _utcDateTime).TotalMilliseconds;
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static double? ToUnixTime(this string dateTime, string format)
        {
            if (!DateTime.TryParseExact(dateTime, format, null, DateTimeStyles.None,
                out var birthdate))
                return null;


            return ToUnixTime(birthdate);
        }

        /// <summary>
        ///     Convert unix time to UTC time.
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        public static DateTime ToDateTimeUtc(this double unixTime)
        {
            return _utcDateTime.AddMilliseconds(unixTime);
        }

        /// <summary>
        /// To formatted date time utc string.
        /// </summary>
        /// <param name="unixTime"></param>
        /// <param name="timeFormat"></param>
        /// <returns></returns>
        public static string ToDateTimeUtc(this double unixTime, string timeFormat)
        {
            return unixTime.ToDateTimeUtc()
                .ToString(timeFormat);
        }

        #region Properties

        // ReSharper disable once InconsistentNaming
        private static readonly DateTime _utcDateTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #endregion
    }
}