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
	{
		/// <summary>
		/// Whether the schedule is currently enabled or not.
		/// </summary>
		[DataMember]
		[JsonConfEditor(DefaultValue = true)]
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// The rules that should trigger the schedule to run.
		/// </summary>
		[DataMember]
		public List<Trigger> Triggers { get; set; } = new List<Trigger>();

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
		public override string ToString()
		{
			if (this.Triggers == null || this.Triggers.Count == 0)
				return "no schedule triggers specified.<br />";

			var output = "according to the following schedule:";

			// Output the triggers as a HTML list.
			output += "<ul>";
			foreach (var trigger in this.Triggers)
			{
				var triggerOutput = trigger.ToString();
				if (string.IsNullOrWhiteSpace(triggerOutput))
					continue;
				output += $"<li>{triggerOutput}</li>";
			}
			output += "</ul>";

			return output;
		}
	}
}