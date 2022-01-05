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
				return System.Security.SecurityElement.Escape($"{(int)timespan.Value.Milliseconds} {((int)timespan.Value.Milliseconds == 1 ? Resources.TimeResources.Component_Millisecond : Resources.TimeResources.Component_Milliseconds)}");
			if (timespan.Value <= TimeSpan.FromSeconds(120))
				return System.Security.SecurityElement.Escape($"{(int)timespan.Value.TotalSeconds} {((int)timespan.Value.TotalSeconds == 1 ? Resources.TimeResources.Component_Second : Resources.TimeResources.Component_Seconds)}");

			// Build a text representation
			var components = new List<string>();
			if (timespan.Value.Days > 0)
				components.Add(System.Security.SecurityElement.Escape($"{timespan.Value.Days} {((int)timespan.Value.Days == 1 ? Resources.TimeResources.Component_Day : Resources.TimeResources.Component_Days)}"));
			if (timespan.Value.Hours > 0)
				components.Add(System.Security.SecurityElement.Escape($"{timespan.Value.Hours} {((int)timespan.Value.Hours == 1 ? Resources.TimeResources.Component_Hour : Resources.TimeResources.Component_Hours)}"));
			if (timespan.Value.Minutes > 0)
				components.Add(System.Security.SecurityElement.Escape($"{timespan.Value.Minutes} {((int)timespan.Value.Minutes == 1 ? Resources.TimeResources.Component_Minute : Resources.TimeResources.Component__Minutes)}"));
			if (timespan.Value.Seconds > 0)
				components.Add(System.Security.SecurityElement.Escape($"{timespan.Value.Seconds} {((int)timespan.Value.Seconds == 1 ? Resources.TimeResources.Component_Second : Resources.TimeResources.Component_Seconds)}"));

			// Build a text representation
			var output = "";
			for (var i = 0; i < components.Count; i++)
			{
				if (i != 0)
				{
					if (i == components.Count - 1)
					{
						output += System.Security.SecurityElement.Escape(Resources.TimeResources.Component_SeparatorLast);
					}
					else
					{
						output += System.Security.SecurityElement.Escape(Resources.TimeResources.Component_Separator);
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
				return $"<p>{System.Security.SecurityElement.Escape(Resources.AsynchronousOperationsResources.RepeatType_Interval_NoTimeSpanSpecified)}<br /></p>";

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
					? System.Security.SecurityElement.Escape(Resources.TimeResources.NotRunSinceLastVaultStart)
					: System.Security.SecurityElement.Escape(Resources.TimeResources.NotScheduled);

			// Find the difference between the scheduled time and now.
			var universalValue = value.Value.ToUniversalTime();
			var localTime = universalValue.ToLocalTime();
			var diff = universalValue.Subtract(DateTime.UtcNow);
			var isInPast = diff < TimeSpan.Zero;
			if (Math.Abs(diff.TotalSeconds) <= 10)
			{
				// Now!
				return representation == DateTimeRepresentationOf.LastRun
					? System.Security.SecurityElement.Escape(Resources.TimeResources.LastRunJustNow)
					: System.Security.SecurityElement.Escape(Resources.TimeResources.NextRunDueNow);
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
							return System.Security.SecurityElement.Escape(Resources.TimeResources.Waiting);

						// It is the next run but it's in the past.
						return System.Security.SecurityElement.Escape(string.Format(Resources.TimeResources.OverdueBySeconds, (int)diff.TotalSeconds));
					}
				}

				// Work out the difference string ("x minutes ago").
				var diffString = "";
				if (diff < TimeSpan.FromSeconds(60))
				{
					// Show the time in seconds.
					diffString = $"{(int)diff.TotalSeconds} {Resources.TimeResources.Component_Seconds}";
				}
				else if (diff < TimeSpan.FromMinutes(60 * 2))
				{
					// Show the time in minutes.
					diffString = $"{(int)diff.TotalMinutes} {Resources.TimeResources.Component__Minutes}";
				}
				else if (diff < TimeSpan.FromHours(24))
				{
					// Show the time in hours.
					diffString = $"{(int)diff.TotalHours} {Resources.TimeResources.Component_Hours}";
				}
				else
				{
					// Default to the specific time.
					return localTime.Date == DateTime.Now.ToLocalTime().Date
						? System.Security.SecurityElement.Escape(string.Format(Resources.TimeResources.AtSpecificTime, localTime.ToString("HH:mm:ss")))
						: System.Security.SecurityElement.Escape(string.Format(Resources.TimeResources.AtSpecificTimeOnDate, localTime.ToString("HH:mm:ss"), localTime.ToString("yyyy-MM-dd")));
				}

				// Render out ago vs in.
				if (isInPast)
				{
					// Past.
					if (representation == DateTimeRepresentationOf.NextRun)
					{
						// It is the next run but it's in the past.
						return System.Security.SecurityElement.Escape(string.Format(Resources.TimeResources.Overdue, diffString));
					}
					return System.Security.SecurityElement.Escape(string.Format(Resources.TimeResources.LastRunInPast, diffString)); 
				}
				else
				{
					// Future.
					return localTime.Date == DateTime.Now.ToLocalTime().Date
						? System.Security.SecurityElement.Escape(string.Format(Resources.TimeResources.AtSpecificTimeWithDifference, localTime.ToString("HH:mm:ss"), diffString))
						: System.Security.SecurityElement.Escape(string.Format(Resources.TimeResources.AtSpecificTimeOnDateWithDifference, localTime.ToString("HH:mm:ss"), localTime.ToString("yyyy-MM-dd"), diffString));
				}
			}
		}
	}
}
