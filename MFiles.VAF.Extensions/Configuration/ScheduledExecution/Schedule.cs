using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration.ScheduledExecution;
using MFiles.VaultApplications.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.ScheduledExecution
{

	/// <summary>
	/// Represents a schedule in which a job should be re-run.
	/// </summary>
	[DataContract]
	public class Schedule
		: IRecurrenceConfiguration
	{
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(Schedule));

		/// <summary>
		/// Whether the schedule is currently enabled or not.
		/// </summary>
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.General_Enabled_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.General_Enabled_HelpText),
			DefaultValue = true
		)]
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// The rules that should trigger the schedule to run.
		/// </summary>
		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_Triggers_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_Triggers_HelpText)
		)]
		public List<Trigger> Triggers { get; set; } = new List<Trigger>();

		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.General_RunOnVaultStart_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.General_RunOnVaultStart_HelpText),
			DefaultValue = false
		)]
		public bool? RunOnVaultStartup { get; set; }

		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_TriggerTimeType_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_TriggerTimeType_HelpText),
			DefaultValue = TriggerTimeType.ServerTime
		)]
		public TriggerTimeType TriggerTimeType { get; set; } = TriggerTimeType.ServerTime;
		public bool ShouldSerializeTriggerTimeType() => this.TriggerTimeType != TriggerTimeType.ServerTime;

		[DataMember]
		[JsonConfEditor
		(
			Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_TriggerTimeCustomTimeZone_Label),
			HelpText = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_TriggerTimeCustomTimeZone_HelpText),
			TypeEditor = "options",
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'TriggerTimeType' && .value == 'Custom' }"
		)]
		[ValueOptions(typeof(TimeZoneStableValueOptionsProvider))]
		public string TriggerTimeCustomTimeZone { get; set; } = "UTC";
		public bool ShouldSerializeTriggerTimeCustomTimeZone() => !string.IsNullOrWhiteSpace(this.TriggerTimeCustomTimeZone) && this.TriggerTimeCustomTimeZone != "UTC";

		/// <summary>
		/// Gets the next execution datetime for this trigger.
		/// </summary>
		/// <param name="after">The time after which the schedule should run.  Defaults to now (i.e. next-run time) if not provided.</param>
		/// <returns>The next execution time.</returns>
		public DateTimeOffset? GetNextExecution(DateTime? after = null)
		{
			// If we are not enabled then die.
			if (false == this.Enabled)
				return null;

			// What type of time are we using?
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
			switch(this.TriggerTimeType)
			{
				case TriggerTimeType.Utc:
					timeZoneInfo = TimeZoneInfo.Utc;
					break;
				case TriggerTimeType.Custom:
					try
					{
						timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(this.TriggerTimeCustomTimeZone);
					}
					catch
					{
						this.Logger?.Warn($"Could not convert '{this.TriggerTimeCustomTimeZone}' to a time zone.  Reverting to local.");
						timeZoneInfo = TimeZoneInfo.Local;
					}
					break;
			}

			// Get the next execution date from the triggers.
			return this.Triggers?
				.Select(t => t.GetNextExecution(after, timeZoneInfo))
				.Where(d => d.HasValue)
				.OrderBy(d => d)
				.FirstOrDefault();
		}


		/// <inheritdoc />
		public string ToDashboardDisplayString()
		{
			// If the schedule is not enabled then report that it will not run.
			if (!this.Enabled)
				return $"<p>{Resources.Schedule.WillNotRunAsScheduleNotEnabled.EscapeXmlForDashboard()}</p>";

			// If there are no triggers then it will not run on a schedule.
			// Note: may still run on startup.
			if (this.Triggers == null || this.Triggers.Count == 0)
				return this.RunOnVaultStartup.HasValue && this.RunOnVaultStartup.Value
					? $"<p>{Resources.Schedule.DoesNotRepeat_RunsWhenVaultStarts.EscapeXmlForDashboard()}<br /></p>"
					: $"<p>{Resources.Schedule.DoesNotRepeat_DoesNotRunWhenVaultStarts.EscapeXmlForDashboard()}<br /></p>";

			var output = this.RunOnVaultStartup.HasValue && this.RunOnVaultStartup.Value
				? $"<p>{Resources.Schedule.Repeats_Intro_RunsWhenVaultStarts.EscapeXmlForDashboard()}"
				: $"<p>{Resources.Schedule.Repeats_Intro_DoesNotRunWhenVaultStarts.EscapeXmlForDashboard()}";

			// Get the timezone.
			var timeZoneInfo = TimeZoneInfo.Local;
			if (this.TriggerTimeType == TriggerTimeType.Utc)
			{
				timeZoneInfo = TimeZoneInfo.Utc;
			}
			if (this.TriggerTimeType == TriggerTimeType.Custom)
			{
				try
				{
					timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(this.TriggerTimeCustomTimeZone);
				}
				catch
				{
					this.Logger?.Warn($"Could not convert {this.TriggerTimeCustomTimeZone} to a time zone.");
				}
			}

			// Output the triggers as a HTML list.
			output += "<ul>";
			foreach (var trigger in this.Triggers)
			{
				var triggerOutput = trigger.ToString(this.TriggerTimeType, timeZoneInfo);
				if (string.IsNullOrWhiteSpace(triggerOutput))
					continue;
				output += $"<li>{triggerOutput}</li>";
			}
			output += "</ul></p>";

			return output;
		}
	}
}