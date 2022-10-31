using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.ScheduledExecution
{
	/// <summary>
	/// Represents a trigger that runs on some days in a week, potentially multiple times per day.
	/// It always runs at the same time every day.  If different time triggers are required then create
	/// multiple instances of a weekly trigger (e.g. one for Wednesdays at 3pm, one for Fridays at 9pm).
	/// </summary>
	[UsesConfigurationResources]
	public class WeeklyTrigger
		: DailyTrigger
	{
		private ILogger Logger { get; } = LogManager.GetLogger<WeeklyTrigger>();
		/// <summary>
		/// The days on which to trigger the schedule.
		/// There must be at least one item in this collection for the trigger to be active.
		/// </summary>
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_WeeklyTrigger_TriggerDays_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_WeeklyTrigger_TriggerDays_HelpText),
			ChildName = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_WeeklyTrigger_TriggerDays_ChildName)
		)]
		public List<DayOfWeek> TriggerDays { get; set; } = new List<DayOfWeek>();

		/// <summary>
		/// Creates a <see cref="WeeklyTrigger"/> instance.
		/// </summary>
		public WeeklyTrigger()
		{
			base.Type = ScheduleTriggerType.Weekly;
		}

		/// <inheritdoc />
		public override DateTimeOffset? GetNextExecution(DateTimeOffset? after = null, TimeZoneInfo timeZoneInfo = null)
		{
			// Sanity.
			if (
				(null == this.TriggerDays || 0 == this.TriggerDays.Count)
				||
				(null == this.TriggerTimes || 0 == this.TriggerTimes.Count)
				)
				return null;

			// When should we start looking?
			timeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Local;

			// Make sure after is in the correct timezone.
			after = after ?? DateTimeOffset.Now;
			this.Logger?.Trace($"Retrieving next execution after {after}");

			this.Logger.Trace($"There are {this.TriggerTimes.Count} times configured: {string.Join(", ", this.TriggerTimes)}");

			// Get the next execution time (this will not find run times today).
			var potentialMatches = this.TriggerDays
				.SelectMany(d => GetNextDayOfWeek(after.Value, d))
				.Select
				(
					d => new DailyTrigger() { Type = ScheduleTriggerType.Daily, TriggerTimes = this.TriggerTimes }
						.GetNextExecutionIncludingNextDay(d, timeZoneInfo, false)
				)
				.Where(d => d != null)
				.Select(d => d.Value)
				.Where(d => d > after.Value)
				.OrderBy(d => d)
				.ToList();

			this.Logger?.Trace($"These are the potential matches: {string.Join(", ", potentialMatches)}");

			return potentialMatches
				.Select(d => d.ToUniversalTime())
				.FirstOrDefault();
		}

		/// <summary>
		/// Helper method to return the next potential run date within a schedule.
		/// </summary>
		/// <param name="after">The day to start looking on.</param>
		/// <param name="dayOfWeek">The day of the week to return.</param>
		/// <returns>
		/// If <paramref name="after"/> is the same day as <paramref name="dayOfWeek"/> then will return
		/// two items - one for today and one for the same day next week.
		/// If not then it will return one item - for the next time that this day occurs
		/// (later this week or next, depending on parameters).
		/// </returns>
		internal static IEnumerable<DateTimeOffset> GetNextDayOfWeek(DateTimeOffset after, DayOfWeek dayOfWeek)
		{
			// Get the number of days ahead.
			var daysToAdd = (7 - (after.DayOfWeek - dayOfWeek)) % 7;

			// If we get today then return it and also next week.
			if (daysToAdd == 0)
			{
				yield return after;
				daysToAdd = 7;
			}

			// Get the next one of this day.
			yield return after.Date.AddDays(daysToAdd);
		}

		/// <inheritdoc />
		public override string ToString(TriggerTimeType triggerTimeType, TimeZoneInfo customTimeZone)
		{
			// Sanity.
			if (null == this.TriggerDays || this.TriggerDays.Count == 0)
				return null;
			if (null == this.TriggerTimes || this.TriggerTimes.Count == 0)
				return null;

			// Append the time zone if we can.
			var times = string.Join(", ", this.TriggerTimes.OrderBy(t => t).Select(t => t.ToString()));
			if (customTimeZone != null)
				if (customTimeZone == TimeZoneInfo.Local)
					times += " (server time)";
				else if (customTimeZone == TimeZoneInfo.Utc)
					times += " (UTC)";
				else
					times += $" ({customTimeZone.DisplayName})";
			return Resources.Schedule.Triggers_WeeklyTrigger.EscapeXmlForDashboard
				(
					string.Join(", ", this.TriggerDays.OrderBy(t => t)),
					times
				);
		}
	}
}