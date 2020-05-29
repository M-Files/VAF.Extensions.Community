using MFiles.VAF.MultiserverMode;
using Newtonsoft.Json;
using System;
using System.Text;
using MFiles.VAF.Common;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	/// <summary>
	/// A directive that contains information for a background operation.
	/// </summary>
	internal class BackgroundOperationTaskQueueDirective
		: TaskQueueDirective
	{
		public BackgroundOperationTaskQueueDirective()
		{
		}

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