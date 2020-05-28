using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	public class WrappedDirective
		: TaskQueueDirective
	{
		public WrappedDirective()
		{
		}

		public WrappedDirective(string backgroundOperationName)
			: this()
		{
			// Sanity.
			this.BackgroundOperationName = backgroundOperationName;
		}

		public WrappedDirective
		(
			string backgroundOperationName,
			byte[] internalDirective
		)
			: this(backgroundOperationName)
		{
			this.InternalDirective = internalDirective;
		}
		public WrappedDirective
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
		/// The internal directive information that is passed to this job execution.
		/// </summary>
		public byte[] InternalDirective { get; set; }
	}
}