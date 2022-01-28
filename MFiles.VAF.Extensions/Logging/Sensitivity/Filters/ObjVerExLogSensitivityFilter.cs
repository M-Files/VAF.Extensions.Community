using MFiles.VAF.Common;
using MFiles.VaultApplications.Logging.Sensitivity;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Logging.Sensitivity.Filters
{
	/// <summary>
	/// Implements a log sensitivity filter for <see cref="ObjVerEx>"/>.
	/// </summary>
	public class ObjVerExLogSensitivityFilter
		: LogSensitivityFilterBase<ObjVerEx>
	{
		/// <inheritdoc />
		public override string FilterValueForLogging(ObjVerEx value, VaultApplications.Logging.Sensitivity.Sensitivity level, string format, IFormatProvider formatProvider)
		{
			// Sanity.
			if (null == value?.Info)
				return String.Empty;

			// Try and get an ObjectVersion filter from the registered factories.
			if (this.TryGetFilterFromFactories<ObjectVersion>(out ILogSensitivityFilter filter))
				return filter?.FilterValueForLogging(value, level, format, formatProvider);

			// No suitable filter.
			return String.Empty;
		}
	}
}
