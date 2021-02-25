using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	/// <summary>
	/// Defines that specific properties of a class must be decorated with a <see cref="DataMemberAttribute"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class DataMemberRequiredAttribute
		: Attribute
	{
		/// <summary>
		/// The properties that need the data member attribute.
		/// </summary>
		public List<string> PropertyNames {get; set; }
			= new List<string>();

		public DataMemberRequiredAttribute
		(
			params string[] propertyNames
		)
			: base()
		{
			this.PropertyNames.AddRange(propertyNames ?? new string[0]);
		}
	}
}
