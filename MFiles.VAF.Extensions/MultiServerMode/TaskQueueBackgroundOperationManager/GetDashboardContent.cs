using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.MultiServerMode.ScheduledExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	public partial class TaskQueueBackgroundOperationManager
	{
		/// <summary>
		/// Returns some dashboard content that shows the background operations and their current status.
		/// </summary>
		/// <returns>The dashboard content.</returns>
		public virtual IDashboardContent GetDashboardContent(string baseContent)
		{
			// Add in any base content, if we have it.
			var contentCollection = new DashboardContentCollection();
			if (null != baseContent)
				contentCollection.Add(new DashboardCustomContent(baseContent));

			// Create the dashboard list.
			var list = new DashboardList();
			if (this.BackgroundOperations.Count == 0)
			{
				// No background operations.
				list.Items.Add(new DashboardListItem()
				{
					InnerContent = new DashboardCustomContent("<em>There are no current background operations.</em>")
				});
			}
			else
			{
				// Output each background operation as a list item.
				foreach (var kvp in this.BackgroundOperations)
				{
					// Create the (basic) list item.
					var listItem = new DashboardListItem()
					{
						Title = kvp.Key,
						StatusSummary = new Configuration.Domain.DomainStatusSummary()
						{
							Label = kvp.Value.Status.ToString()
						}
					};

					// Output the schedule/interval data.
					var htmlString = "Runs ";
					switch (kvp.Value.BackgroundOperation.RepeatType)
					{
						case TaskQueueBackgroundOperationRepeatType.NotRepeating:
							htmlString += "on demand (does not repeat).<br />";
							break;
						case TaskQueueBackgroundOperationRepeatType.Interval:
							htmlString += $"{kvp.Value.BackgroundOperation.Interval.ToDisplayString()}.<br />";
							break;
						case TaskQueueBackgroundOperationRepeatType.Schedule:
							htmlString += $"{kvp.Value.BackgroundOperation.Schedule.ToDisplayString()}";
							break;
						default:
							htmlString = "<em>Unhandled: " + kvp.Value.BackgroundOperation.RepeatType + "</em><br />";
							break;
					}

					// Customise the description based on the last/next run values.
					if (kvp.Value.Status == TaskQueueBackgroundOperationStatus.Running)
					{
						htmlString += "<em>Currently running.</em>";
					}
					else
					{
						htmlString += !kvp.Value.LastRun.HasValue
							? "This background operation has not run since the vault was brought online.<br />"
							: $"Last run was {kvp.Value.LastRun.ToTimeOffset(FormattingExtensionMethods.DateTimeRepresentationOf.LastRun)}.<br />";
						htmlString += !kvp.Value.NextRun.HasValue
							? "The background operation is not scheduled to run again."
							: $"Next run is {kvp.Value.NextRun.ToTimeOffset(FormattingExtensionMethods.DateTimeRepresentationOf.NextRun)}.";
					}
					listItem.InnerContent = new DashboardCustomContent(htmlString);

					// Add the list item.
					list.Items.Add(listItem);
				}

			}

			// Set the panel content and return it.
			contentCollection.Add(new DashboardPanel()
			{
				Title = "Background Operations",
				InnerContent = list
			});
			return contentCollection;
		}
	}
	internal static class FormattingExtensionMethods
	{
		internal enum DateTimeRepresentationOf
		{
			Unknown = 0,
			LastRun = 1,
			NextRun = 2
		}
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
			var output = "";
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
		internal static string ToDisplayString(this Schedule schedule)
		{
			return schedule?.ToString()
				?? "never";
		}
		internal static string ToTimeOffset(this DateTime? value, DateTimeRepresentationOf representation)
		{
			// No value?
			if (null == value)
				return representation == DateTimeRepresentationOf.LastRun
					? "(not since last vault start)"
					: "(not scheduled)";

			// Find the difference between the scheduled time and now.
			var universalValue = value.Value.ToUniversalTime();
			var diff = universalValue.Subtract(DateTime.UtcNow);
			var isInPast = diff < TimeSpan.Zero;
			if (diff.TotalSeconds == 0)
			{
				// Now!
				return "due now";
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
					return "at " + universalValue.ToShortTimeString() + " on " + universalValue.ToString("D");
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
					return "in " + diffString + ", at " + universalValue.ToShortTimeString() + " on " + universalValue.ToString("D");
				}
			}
		}
	}
}
