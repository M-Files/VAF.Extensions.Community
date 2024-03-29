﻿using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Logging.SensitivityFilters;
using MFiles.VAF.Extensions.Configuration.Upgrading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Logging.Sensitivity.Filters
{
	/// <summary>
	/// Implements a log sensitivity filter for <see cref="INamedValueItem"/>.
	/// </summary>
	public class INamedValueItemSensitivityFilter
		: LogSensitivityFilterBase<INamedValueItem>
	{
		public override string FilterValueForLogging
		(
			INamedValueItem input,
			LogSensitivity level,
			IEnumerable<SensitivityFlag> customFlags,
			string format,
			IFormatProvider formatProvider
		)
		{
			if (null == input)
				return String.Empty;

			if (input is ISingleNamedValueItem singleNamedValueItem)
			{
				return $"value {singleNamedValueItem.Name} in namespace {singleNamedValueItem.Namespace} of type {singleNamedValueItem.NamedValueType}";
			}
			else if (input is IEntireNamespaceNamedValueItem entireNamespaceNamedValueItem)
			{
				return $"all values in namespace {entireNamespaceNamedValueItem.Namespace} of type {entireNamespaceNamedValueItem.NamedValueType}";
			}
			else
			{
				return $"(unhandled input type: {input.GetType().FullName}";
			}

		}
	}
}
