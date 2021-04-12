using MFiles.VAF.Configuration;
using System;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.ScheduledExecution
{
	[DataContract]
	[JsonConfEditor(NameMember = nameof(Time))]
	public class TriggerTime
	{
		[DataMember]
		[JsonConfEditor(TypeEditor = "time")]
		public TimeSpan Time { get; set; }

		public static implicit operator TriggerTime(TimeSpan input)
		{
			return new TriggerTime() { Time = input };
		}

		public static implicit operator TimeSpan(TriggerTime input)
		{
			return input?.Time ?? TimeSpan.Zero;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return this.Time.ToString();
		}
	}
}