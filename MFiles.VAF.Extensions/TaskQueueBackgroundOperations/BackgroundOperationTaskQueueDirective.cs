using MFiles.VAF;
using Newtonsoft.Json;
using System;
using System.Text;
using MFiles.VAF.Common;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// A directive that contains information for a background operation.
	/// </summary>
	internal class BackgroundOperationTaskQueueDirective
		: TaskQueueDirective
	{
		/// <summary>
		/// Creates an empty background operation task queue directive.
		/// This constructor is mainly used for JSON deserialisation.
		/// Instead use <see cref="BackgroundOperationTaskQueueDirective.BackgroundOperationTaskQueueDirective(string, TaskQueueDirective)"/>.
		/// If this constructor is used in codethen you must explicitly manually set
		/// <see cref="BackgroundOperationName"/> and, optionally, <see cref="InternalDirective"/>.
		/// </summary>
		public BackgroundOperationTaskQueueDirective()
		{
		}

		/// <summary>
		/// Instantiates a background operation task queue directive that
		/// may include a wrapped internal directive.
		/// </summary>
		/// <param name="backgroundOperationName">The name of the background operation on which this task is run.</param>
		/// <param name="internalDirective">The directive - if any - to pass to the job.</param>
		public BackgroundOperationTaskQueueDirective
		(
			string backgroundOperationName,
			TaskQueueDirective internalDirective
		)
		{
			this.BackgroundOperationName = backgroundOperationName;
			this.InternalDirective = internalDirective;
		}

		/// <summary>
		/// The <see cref="TaskQueueBackgroundOperation.Name"/> of the background operation that
		/// will process this task.
		/// </summary>
		public string BackgroundOperationName { get; set; }
		
		/// <summary>
		/// The internal directive information that is passed to this job execution (for serialisation).
		/// </summary>
		public byte[] InternalDirectiveForSerialization { get; set; }

		/// <summary>
		/// The type of the internal directive.
		/// </summary>
		public string InternalDirectiveTypeForSerialization { get; set; }

		/// <summary>
		/// The internal directive information that is passed to this job execution.
		/// </summary>
		private TaskQueueDirective internalDirective = null;

		/// <summary>
		/// The internal directive information that is passed to this job execution.
		/// </summary>
		[JsonIgnore]
		public TaskQueueDirective InternalDirective
		{
			get => this.internalDirective;
			set
			{
				this.internalDirective = value;
				this.InternalDirectiveForSerialization = value?.ToBytes();
				this.InternalDirectiveTypeForSerialization = value?.GetType()?.AssemblyQualifiedName;
			}
		}

		/// <summary>
		/// Returns <see cref="InternalDirective"/> as a parsed/populated instance.
		/// </summary>
		/// <returns>The directive, or null if no directive is found.</returns>
		public TaskQueueDirective GetParsedInternalDirective()
		{
			return this.GetParsedInternalDirective<TaskQueueDirective>();
		}

		/// <summary>
		/// Returns <see cref="InternalDirective"/> as a parsed/populated instance.
		/// </summary>
		/// <typeparam name="TDirective">The directive type.</typeparam>
		/// <returns>The directive, or null if no directive is found.</returns>
		public TDirective GetParsedInternalDirective<TDirective>()
			where TDirective : TaskQueueDirective
		{
			return this.InternalDirectiveForSerialization == null
				? null
				: JsonConvert.DeserializeObject
				(
					this.InternalDirectiveForSerialization.AsString(Encoding.UTF8),
					Type.GetType(this.InternalDirectiveTypeForSerialization)
				) as TDirective;
		}

	}
}