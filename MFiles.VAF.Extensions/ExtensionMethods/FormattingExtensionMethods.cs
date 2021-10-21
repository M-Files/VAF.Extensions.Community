using MFiles.VAF.Extensions.ScheduledExecution;
using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions
{
	public static class FormattingExtensionMethods
	{
		/// <summary>
		/// A flag to note whether the datetime is in the future or past.
		/// </summary>
		internal enum DateTimeRepresentationOf
		{
			Unknown = 0,

			/// <summary>
			/// The datetime represents when the code last run (i.e. expected to be in the past).
			/// </summary>
			LastRun = 1,

			/// <summary>
			/// The datetime represents when the code will next run (i.e. expected to be in the future).
			/// </summary>
			NextRun = 2
		}

		/// <summary>
		/// Converts <paramref name="timespan"/> to a string.
		/// If <paramref name="timespan"/> is null or zero, returns "no timespan specified".
		/// </summary>
		/// <param name="timespan">The timespan to convert.</param>
		/// <returns>A string in English describing the timespan.</returns>
		internal static string ToDisplayString(this TimeSpan timespan)
		{
			return ((TimeSpan?)timespan).ToDisplayString();
		}

		/// <summary>
		/// Converts <paramref name="timespan"/> to a string.
		/// If <paramref name="timespan"/> is null or zero, returns "no timespan specified".
		/// </summary>
		/// <param name="timespan">The timespan to convert.</param>
		/// <returns>A string in English describing the timespan.</returns>
		internal static string ToDisplayString(this TimeSpan? timespan)
		{
			// Sanity.
			if (false == timespan.HasValue || timespan.Value < TimeSpan.Zero)
				return "";

			// Seconds be easy.
			if (timespan.Value < TimeSpan.FromSeconds(1))
				return $"{(int)timespan.Value.Milliseconds} millisecond{((int)timespan.Value.Milliseconds == 1 ? "" : "s")}";
			if (timespan.Value <= TimeSpan.FromSeconds(120))
				return $"{(int)timespan.Value.TotalSeconds} second{((int)timespan.Value.TotalSeconds == 1 ? "" : "s")}";

			// Build a text representation
			var components = new List<string>();
			if (timespan.Value.Days > 0)
				components.Add($"{timespan.Value.Days} day{((int)timespan.Value.Days == 1 ? "" : "s")}");
			if (timespan.Value.Hours > 0)
				components.Add($"{timespan.Value.Hours} hour{((int)timespan.Value.Hours == 1 ? "" : "s")}");
			if (timespan.Value.Minutes > 0)
				components.Add($"{timespan.Value.Minutes} minute{((int)timespan.Value.Minutes == 1 ? "" : "s")}");
			if (timespan.Value.Seconds > 0)
				components.Add($"{timespan.Value.Seconds} second{((int)timespan.Value.Seconds == 1 ? "" : "s")}");

			// Build a text representation
			var output = "";
			for (var i = 0; i < components.Count; i++)
			{
				if (i != 0)
				{
					if (i == components.Count - 1)
					{
						output += ", and ";
					}
					else
					{
						output += ", ";
					}
				}
				output += components[i];
			}
			return output;
		}

		/// <summary>
		/// Converts <paramref name="timespan"/> to a string for display on a dashboard.
		/// </summary>
		/// <param name="timespan">The timespan to convert.</param>
		/// <returns>A string in English describing the timespan.</returns>
		public static string ToDashboardDisplayString(this TimeSpan timespan)
		{
			return ((TimeSpanEx)timespan).ToDashboardDisplayString();
		}

		/// <summary>
		/// Converts <paramref name="timespan"/> to a string for display on a dashboard.
		/// </summary>
		/// <param name="timespan">The timespan to convert.</param>
		/// <returns>A string in English describing the timespan.</returns>
		public static string ToDashboardDisplayString(this TimeSpan? timespan)
		{
			// Sanity.
			if (false == timespan.HasValue || timespan.Value <= TimeSpan.Zero)
				return "<p>No timespan specified; does not repeat.<br /></p>";

			return ((TimeSpanEx)timespan.Value).ToDashboardDisplayString();
		}


		/// <summary>
		/// Converts <paramref name="value"/> to a representation such as "in 20 minutes".
		/// If <paramref name="value"/> is null then returns a flag stating not scheduled / not run, depending on whether
		/// <paramref name="representation"/> is expected to be in the past or future.
		/// </summary>
		/// <param name="value">The value to represent.</param>
		/// <param name="representation">Whether the value is supposed to be last-run (past) or next-run (future).</param>
		/// <returns>A string in English stating when it should run.</returns>
		internal static string ToTimeOffset(this DateTime value, DateTimeRepresentationOf representation)
		{
			return ((DateTime?)value).ToTimeOffset(representation);
		}

		/// <summary>
		/// Converts <paramref name="value"/> to a representation such as "in 20 minutes".
		/// If <paramref name="value"/> is null then returns a flag stating not scheduled / not run, depending on whether
		/// <paramref name="representation"/> is expected to be in the past or future.
		/// </summary>
		/// <param name="value">The value to represent.</param>
		/// <param name="representation">Whether the value is supposed to be last-run (past) or next-run (future).</param>
		/// <returns>A string in English stating when it should run.</returns>
		internal static string ToTimeOffset(this DateTime? value, DateTimeRepresentationOf representation)
		{
			// No value?
			if (null == value)
				return representation == DateTimeRepresentationOf.LastRun
					? "(not since last vault start)"
					: "(not scheduled)";

			// Find the difference between the scheduled time and now.
			var universalValue = value.Value.ToUniversalTime();
			var localTime = universalValue.ToLocalTime();
			var diff = universalValue.Subtract(DateTime.UtcNow);
			var isInPast = diff < TimeSpan.Zero;
			if (diff.TotalSeconds == 0)
			{
				// Now!
				return representation == DateTimeRepresentationOf.LastRun
					? "Now"
					: "Due now";
			}
			else
			{
				// Convert the diff to a string.
				if (isInPast)
				{
					// It's in the past.  If this is a "next run" then it's overdue.
					diff = new TimeSpan(diff.Ticks * -1);
					if (representation == DateTimeRepresentationOf.NextRun)
					{
						// If it's <= 15 seconds then we may just be waiting to be notified.
						if (diff <= TimeSpan.FromSeconds(15))
							return "Waiting to be run";

						// It is the next run but it's in the past.
						return $"Overdue by {(int)diff.TotalSeconds}s";
					}
				}

				// Work out the difference string ("x minutes ago").
				var diffString = "";
				if (diff < TimeSpan.FromSeconds(60))
				{
					// Show the time in seconds.
					diffString = ((int)diff.TotalSeconds).ToString() + " seconds";
				}
				else if (diff < TimeSpan.FromMinutes(60 * 2))
				{
					// Show the time in minutes.
					diffString = ((int)diff.TotalMinutes).ToString() + " minutes";
				}
				else if (diff < TimeSpan.FromHours(24))
				{
					// Show the time in hours.
					diffString = ((int)diff.TotalHours).ToString() + " hours";
				}
				else
				{
					// Default to the specific time.
					return localTime.Date == DateTime.Now.ToLocalTime().Date
						? $"At {localTime.ToString("HH:mm:ss")} server-time"
						: $"At {localTime.ToString("HH:mm:ss")} server-time on {localTime.ToString("yyyy-MM-dd")}";
				}

				// Render out ago vs in.
				if (isInPast)
				{
					// Past.
					if (representation == DateTimeRepresentationOf.NextRun)
					{
						// It is the next run but it's in the past.
						return "Overdue (expected " + diffString + " ago)";
					}
					return diffString + " ago";
				}
				else
				{
					// Future.
					return localTime.Date == DateTime.Now.ToLocalTime().Date
						? $"At {localTime.ToString("HH:mm:ss")} server-time (in {diffString})"
						: $"At {localTime.ToString("HH:mm:ss")} server-time on {localTime.ToString("yyyy-MM-dd")} (in {diffString})";
				}
			}
		}
	}
}
