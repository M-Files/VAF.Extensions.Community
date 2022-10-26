using MFiles.VAF.Common;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Logging.SensitivityFilters;
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
		/// <summary>
		/// Resolve the delegate filter we use.
		/// </summary>
		/// <returns>A sensitivity filter for <see cref="IObjectVersion"/>s.</returns>
		protected ILogSensitivityFilter<IObjectVersion> GetObjectVersionFilter()
		{
			return ResolveDelegateFilter(() => new ObjectVersionLogSensitivityFilter());
		}

		/// <inheritdoc />
		public override IEnumerable<SensitivityFlag> GetSupportedCustomFlags()
		{
			// Return any sensitivity flags from the underlying implementation.
			foreach (SensitivityFlag sf in GetObjectVersionFilter()?.GetSupportedCustomFlags() ?? Enumerable.Empty<SensitivityFlag>())
				yield return sf;
		}

		/// <inheritdoc />
		public override string FilterValueForLogging(ObjVerEx value, LogSensitivity level, IEnumerable<SensitivityFlag> customFlags, string format, IFormatProvider formatProvider)
		{
			// Sanity.
			if (null == value?.Info)
				return String.Empty;

			// Use the object version filter.
			return this.GetObjectVersionFilter()?
				.FilterValueForLogging(value.Info, level, customFlags, format, formatProvider);
		}
	}
}
