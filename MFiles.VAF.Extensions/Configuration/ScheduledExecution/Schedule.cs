﻿using MFiles.VAF.Configuration;
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

		/// <summary>
		/// Gets the next execution datetime for this trigger.
		/// </summary>
		/// <param name="after">The time after which the schedule should run.  Defaults to now (i.e. next-run time) if not provided.</param>
		/// <returns>The next execution time.</returns>
		public DateTime? GetNextExecution(DateTime? after = null)
		{
			// If we are not enabled then die.
			if (false == this.Enabled)
				return null;
			
			// Get the next execution date from the triggers.
			return this.Triggers?
				.Select(t => t.GetNextExecution(after))
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

			// Output the triggers as a HTML list.
			output += "<ul>";
			foreach (var trigger in this.Triggers)
			{
				var triggerOutput = trigger.ToString();
				if (string.IsNullOrWhiteSpace(triggerOutput))
					continue;
				output += $"<li>{triggerOutput}</li>";
			}
			output += "</ul></p>";

			return output;
		}
	}
}