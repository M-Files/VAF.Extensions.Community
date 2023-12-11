using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.ExtensionMethods
{
	public static class TimestampExtensionMethods
	{
		/// <summary>
		/// Converts a <paramref name="dateTime"/> to a <see cref="Timestamp"/>
		/// instance, maintaining the highest level of precision we can.
		/// </summary>
		/// <param name="dateTime">The datetime to represent.</param>
		/// <returns>The timestamp.</returns>
		public static Timestamp ToPreciseTimestamp(this DateTime dateTime)
		{
			return new TimestampClass
			{
				Year = (uint)dateTime.Year,
				Month = (uint)dateTime.Month,
				Day = (uint)dateTime.Day,
				Hour = (uint)dateTime.Hour,
				Minute = (uint)dateTime.Minute,
				Second = (uint)dateTime.Second,
				Fraction = (uint)(dateTime.Ticks % 1e7M * 100)  // 10,000,000 ticks in a second, 100 nanoseconds in a tick
			};
		}

		/// <summary>
		/// Converts a <paramref name="timestamp"/> to a <see cref="DateTime"/>
		/// instance, maintaining the highest level of precision we can.
		/// </summary>
		/// <param name="timestamp">The timestamp to represent.</param>
		/// <returns>The DateTime.</returns>
		public static DateTime ToPreciseDateTime
		(
			this Timestamp timestamp,
			DateTimeKind kind = DateTimeKind.Local
		)
		{
			return new DateTime
			(
				(int)timestamp.Year,
				(int)timestamp.Month,
				(int)timestamp.Day,
				(int)timestamp.Hour,
				(int)timestamp.Minute,
				(int)timestamp.Second,
				kind
			)
			.AddTicks(timestamp.Fraction / 100);   // 100 nanoseconds in a tick.
		}
	}
}
