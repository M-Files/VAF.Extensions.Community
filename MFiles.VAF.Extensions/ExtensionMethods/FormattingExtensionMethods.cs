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
				return 
					$"{timespan.Value.Milliseconds} {(timespan.Value.Milliseconds == 1 ? Resources.Time.Component_Millisecond : Resources.Time.Component_Milliseconds)}"
						.EscapeXmlForDashboard();
			if (timespan.Value <= TimeSpan.FromSeconds(120))
				return 
					$"{(int)timespan.Value.TotalSeconds} {((int)timespan.Value.TotalSeconds == 1 ? Resources.Time.Component_Second : Resources.Time.Component_Seconds)}"
					.EscapeXmlForDashboard();

			// Build a text representation
			var components = new List<string>();
			if (timespan.Value.Days > 0)
				components.Add($"{timespan.Value.Days} {(timespan.Value.Days == 1 ? Resources.Time.Component_Day : Resources.Time.Component_Days)}");
			if (timespan.Value.Hours > 0)
				components.Add($"{timespan.Value.Hours} {(timespan.Value.Hours == 1 ? Resources.Time.Component_Hour : Resources.Time.Component_Hours)}");
			if (timespan.Value.Minutes > 0)
				components.Add($"{timespan.Value.Minutes} {(timespan.Value.Minutes == 1 ? Resources.Time.Component_Minute : Resources.Time.Component__Minutes)}");
			if (timespan.Value.Seconds > 0)
				components.Add($"{timespan.Value.Seconds} {(timespan.Value.Seconds == 1 ? Resources.Time.Component_Second : Resources.Time.Component_Seconds)}");

			// Build a text representation
			var output = "";
			for (var i = 0; i < components.Count; i++)
			{
				if (i != 0)
				{
					if (i == components.Count - 1)
					{
						output += Resources.Time.Component_SeparatorLast.EscapeXmlForDashboard();
					}
					else
					{
						output += Resources.Time.Component_Separator.EscapeXmlForDashboard();
					}
				}
				output += components[i].EscapeXmlForDashboard();
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
				return $"<p>{Resources.AsynchronousOperations.RepeatType_Interval_NoTimeSpanSpecified.EscapeXmlForDashboard()}<br /></p>";

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
					? Resources.Time.NotRunSinceLastVaultStart.EscapeXmlForDashboard()
					: Resources.Time.NotScheduled.EscapeXmlForDashboard();

			// Find the difference between the scheduled time and now.
			var universalValue = value.Value.ToUniversalTime();
			var localTime = universalValue.ToLocalTime();
			var diff = universalValue.Subtract(DateTime.UtcNow);
			var isInPast = diff < TimeSpan.Zero;
			if (Math.Abs(diff.TotalSeconds) <= 10)
			{
				// Now!
				return representation == DateTimeRepresentationOf.LastRun
					? Resources.Time.LastRunJustNow.EscapeXmlForDashboard()
					: Resources.Time.NextRunDueNow.EscapeXmlForDashboard();
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
							return Resources.Time.Waiting.EscapeXmlForDashboard();

						// It is the next run but it's in the past.
						return Resources.Time.OverdueBySeconds.EscapeXmlForDashboard((int)diff.TotalSeconds);
					}
				}

				// Work out the difference string ("x minutes ago").
				var diffString = "";
				if (diff < TimeSpan.FromSeconds(60))
				{
					// Show the time in seconds.
					diffString = $"{(int)diff.TotalSeconds} {Resources.Time.Component_Seconds}";
				}
				else if (diff < TimeSpan.FromMinutes(60 * 2))
				{
					// Show the time in minutes.
					diffString = $"{(int)diff.TotalMinutes} {Resources.Time.Component__Minutes}";
				}
				else if (diff < TimeSpan.FromHours(24))
				{
					// Show the time in hours.
					diffString = $"{(int)diff.TotalHours} {Resources.Time.Component_Hours}";
				}
				else
				{
					// Default to the specific time.
					return localTime.Date == DateTime.Now.ToLocalTime().Date
						? Resources.Time.AtSpecificTime.EscapeXmlForDashboard(localTime.ToString("HH:mm:ss"))
						: Resources.Time.AtSpecificTimeOnDate.EscapeXmlForDashboard(localTime.ToString("HH:mm:ss"), localTime.ToString("yyyy-MM-dd"));
				}

				// Render out ago vs in.
				if (isInPast)
				{
					// Past.
					if (representation == DateTimeRepresentationOf.NextRun)
					{
						// It is the next run but it's in the past.
						return Resources.Time.Overdue.EscapeXmlForDashboard(diffString);
					}
					return Resources.Time.LastRunInPast.EscapeXmlForDashboard(diffString); 
				}
				else
				{
					// Future.
					return localTime.Date == DateTime.Now.ToLocalTime().Date
						? Resources.Time.AtSpecificTimeWithDifference.EscapeXmlForDashboard(localTime.ToString("HH:mm:ss"), diffString)
						: Resources.Time.AtSpecificTimeOnDateWithDifference.EscapeXmlForDashboard(localTime.ToString("HH:mm:ss"), localTime.ToString("yyyy-MM-dd"), diffString);
				}
			}
		}
	}
}
