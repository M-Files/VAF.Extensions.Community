using MFiles.VAF.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.ScheduledExecution
{
	/// <summary>
	/// Represents a trigger that runs every day, potentially multiple times per day.
	/// </summary>
	[DataContract]
	public class DailyTrigger
		: TriggerBase
	{
		/// <summary>
		/// The times of day to trigger the schedule.
		/// There must be at least one item in this collection for the trigger to be active.
		/// </summary>
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DailyTrigger_TriggerTimes_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_DailyTrigger_TriggerTimes_HelpText),
			ChildTypeEditor = "time"
		)]
		public List<TimeSpan> TriggerTimes { get; set; } = new List<TimeSpan>();

		/// <summary>
		/// Creates a <see cref="DailyTrigger"/> instance.
		/// </summary>
		public DailyTrigger()
		{
			base.Type = ScheduleTriggerType.Daily;
		}

		/// <inheritdoc />
		public override DateTime? GetNextExecution(DateTime? after = null)
		{
			// Sanity.
			if (
				(null == this.TriggerTimes || 0 == this.TriggerTimes.Count)
				)
				return null;

			// When should we start looking?
			after = (after ?? DateTime.UtcNow).ToLocalTime();

			// Get the next execution time.
			return this.TriggerTimes
				.Select(
					t =>
						after.Value.TimeOfDay < t
							? after.Value.Date.Add(t) // Time is yet to come today.
							: after.Value.Date.AddDays(1).Add(t) // Time has passed - return tomorrow.
				)
				.OrderBy(d => d)
				.FirstOrDefault();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			// Sanity.
			if (null == this.TriggerTimes || this.TriggerTimes.Count == 0)
				return null;

			return Resources.Schedule.Triggers_DailyTrigger.EscapeXmlForDashboard(string.Join(", ", this.TriggerTimes.OrderBy(t => t).Select(t => t.ToString())));
		}

		/// <summary>
		/// Automatically converts <paramref name="trigger"/> to a <see cref="Trigger"/>.
		/// </summary>
		/// <param name="trigger">The trigger to convert.</param>
		public static implicit operator Trigger(DailyTrigger triggerConfiguration)
		{
			// Sanity.
			if (null == triggerConfiguration)
				throw new ArgumentNullException(nameof(triggerConfiguration));
			return new Trigger(triggerConfiguration);
		}

	}
}