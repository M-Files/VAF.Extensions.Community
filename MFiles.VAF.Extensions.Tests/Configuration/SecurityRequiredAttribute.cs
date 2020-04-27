using System;
using MFiles.VAF.Configuration;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	/// <summary>
	/// Defines that a specific property of a class must be decorated with a <see cref="SecurityAttribute"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class SecurityRequiredAttribute
		: Attribute
	{
		/// <summary>
		/// The property that needs the security attribute.
		/// </summary>
		public string PropertyName { get; set; }

		public bool IsPassword { get;set; }
		public SecurityAttribute.UserLevel ChangeBy { get; set; }
		public SecurityAttribute.UserLevel ViewBy { get; set; }

		public SecurityRequiredAttribute
		(
			string propertyName,
			bool isPassword = false,
			SecurityAttribute.UserLevel changeBy = SecurityAttribute.UserLevel.SystemAdmin,
			SecurityAttribute.UserLevel viewBy = SecurityAttribute.UserLevel.Undefined
		)
			: base()
		{
			this.PropertyName = propertyName;
			this.IsPassword = isPassword;
			this.ChangeBy = changeBy;
			this.ViewBy = viewBy;
		}
	}
}
