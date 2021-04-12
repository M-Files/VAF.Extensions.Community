using MFiles.VAF.Extensions.ScheduledExecution;
using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions
{
	internal static class FormattingExtensionMethods
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
		internal static string ToDisplayString(this TimeSpan? timespan)
		{
			// Sanity.
			if (false == timespan.HasValue || timespan.Value <= TimeSpan.Zero)
				return "no timespan specified";

			// Seconds be easy.
			if (timespan.Value <= TimeSpan.FromSeconds(120))
				return $"every {(int)timespan.Value.TotalSeconds} seconds";

			// Build a text representation
			var components = new List<string>();
			if (timespan.Value.Days > 0)
				components.Add($"{timespan.Value.Days} days");
			if (timespan.Value.Hours > 0)
				components.Add($"{timespan.Value.Hours} hours");
			if (timespan.Value.Minutes > 0)
				components.Add($"{timespan.Value.Minutes} minutes");
			if (timespan.Value.Seconds > 0)
				components.Add($"{timespan.Value.Seconds} seconds");

			// Build a text representation
			var output = "every ";
			for (var i = 0; i < components.Count; i++)
			{
				if (i == 0)
				{
					output += components[i];
				}
				else if (i == components.Count - 1)
				{
					output += ", and " + components[i];
				}
				else
				{
					output += ", ";
				}
			}
			return output;
		}

		/// <summary>
		/// Converts <paramref name="schedule"/> to a string.
		/// If <paramref name="schedule"/> is null, returns "never", otherwise returns <see cref="Schedule.ToString"/>.
		/// </summary>
		/// <param name="schedule">The schedule to convert.</param>
		/// <returns>A string in English describing the schedule.</returns>
		internal static string ToDisplayString(this Schedule schedule)
		{
			return schedule?.ToString()
				?? "never";
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
					? "now"
					: "due now";
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
						// It is the next run but it's in the past.
						return $"overdue by {(int)diff.TotalSeconds}s";
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
						? $"at {localTime.ToString("HH:mm:ss")} server-time"
						: $"at {localTime.ToString("HH:mm:ss")} server-time on {localTime.ToString("yyyy-MM-dd")}";
				}

				// Render out ago vs in.
				if (isInPast)
				{
					// Past.
					if (representation == DateTimeRepresentationOf.NextRun)
					{
						// It is the next run but it's in the past.
						return "overdue (expected " + diffString + " ago)";
					}
					return diffString + " ago";
				}
				else
				{
					// Future.
					return localTime.Date == DateTime.Now.ToLocalTime().Date
						? $"at {localTime.ToString("HH:mm:ss")} server-time (in {diffString})"
						: $"at {localTime.ToString("HH:mm:ss")} server-time on {localTime.ToString("yyyy-MM-dd")} (in {diffString})";
				}
			}
		}
	}
}
