using MFiles.VAF.MultiserverMode;

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

		public BackgroundOperationTaskQueueDirective(string backgroundOperationName)
			: this()
		{
			// Sanity.
			this.BackgroundOperationName = backgroundOperationName;
		}

		public BackgroundOperationTaskQueueDirective
		(
			string backgroundOperationName,
			byte[] internalDirective
		)
			: this(backgroundOperationName)
		{
			this.InternalDirective = internalDirective;
		}
		public BackgroundOperationTaskQueueDirective
		(
			string backgroundOperationName,
			TaskQueueDirective internalDirective
		)
			: this(backgroundOperationName, internalDirective?.ToBytes())
		{
		}

		/// <summary>
		/// The <see cref="TaskQueueBackgroundOperation.Name"/> of the background operation that
		/// will process this task.
		/// </summary>
		public string BackgroundOperationName { get; set; }
		
		/// <summary>
		/// The internal directive information that is passed to this job execution (for serialisation).
		/// </summary>
		public byte[] InternalDirective { get; set; }

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
			return this.InternalDirective == null
				? null
				: TaskQueueDirective.Parse<TDirective>(this.InternalDirective);
		}

	}
}