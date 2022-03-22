using MFiles.VAF.Common;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// By placing this in the MFiles.VaultApplications.Logging namespace, the "RenderInternalLogs" will apply to this too.
namespace MFiles.VaultApplications.Logging.Sensitivity.Filters
{
	/// <summary>
	/// Implements a log sensitivity filter for <see cref="ObjVerEx>"/>.
	/// </summary>
	public class ObjVerExLogSensitivityFilter
		: LogSensitivityFilterBase<ObjVerEx>
	{
		/// <inheritdoc />
		public override string FilterValueForLogging(ObjVerEx value, Sensitivity level, IEnumerable<string> customFlags, string format, IFormatProvider formatProvider)
		{
			// Sanity.
			if (null == value?.Info)
				return String.Empty;

			// Try and get an ObjectVersion filter from the registered factories.
			if (false == this.TryGetFilter(out ILogSensitivityFilter<IObjectVersion> filter) || null == filter)
			{
				this.Logger?.Error($"Could not load required sensitivity filter for {typeof(IObjectVersion).FullName}.  {typeof(ObjectVersionLogSensitivityFilter).FullName} will be used.");
				filter = new ObjectVersionLogSensitivityFilter();
			}

			return filter.FilterValueForLogging(value.Info, level, customFlags, format, formatProvider);
		}
	}
}
