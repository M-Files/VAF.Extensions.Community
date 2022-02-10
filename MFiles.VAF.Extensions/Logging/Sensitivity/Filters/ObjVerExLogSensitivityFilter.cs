using MFiles.VAF.Common;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// By placing this in the MFiles.VaultApplications.Logging namespace, the "RenderInternalLogs" will apply to this too.
namespace MFiles.VaultApplications.Logging.Sensitivity
{
	/// <summary>
	/// Implements a log sensitivity filter for <see cref="ObjVerEx>"/>.
	/// </summary>
	public class ObjVerExLogSensitivityFilter
		: LogSensitivityFilterBase<ObjVerEx>
	{
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(ObjVerExLogSensitivityFilter));

		/// <inheritdoc />
		public override string FilterValueForLogging(ObjVerEx value, VaultApplications.Logging.Sensitivity.Sensitivity level, IEnumerable<string> customFlags, string format, IFormatProvider formatProvider)
		{
			// Sanity.
			if (null == value?.Info)
				return String.Empty;

			// Try and get an ObjectVersion filter from the registered factories.
			if (this.TryGetFilter<ObjectVersion>(out ILogSensitivityFilter filter))
				return filter?.FilterValueForLogging(value, level, customFlags, format, formatProvider);

			// No suitable filter.
			this.Logger?.Error($"Could not load required sensitivity filter for {typeof(ObjectVersion).FullName}.  Minimum data will be logged.");
			return $"{value.ObjVer.Type}-{value.ObjVer.ID}-{value.ObjVer.Version}";
		}
	}
}
