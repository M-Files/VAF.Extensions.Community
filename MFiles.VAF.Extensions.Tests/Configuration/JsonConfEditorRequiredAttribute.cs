using System;
using MFiles.VAF.Configuration;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	/// <summary>
	/// Defines that specific properties of a class must be decorated with a <see cref="JsonConfEditorAttribute"/>
	/// with specific values.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class JsonConfEditorRequiredAttribute
		: Attribute
	{
		/// <summary>
		/// The property that needs the security attribute.
		/// </summary>
		public string PropertyName { get; set; }

		public object DefaultValue { get; set; }

		public bool Hidden { get; set; }

		public string ShowWhen { get; set; }

		public string HideWhen { get; set; }
		public string ChildTypeEditor { get; set; }

		public JsonConfEditorRequiredAttribute
		(
			string propertyName,
			object defaultValue = null,
			bool hidden = false,
			string showWhen = null,
			string hideWhen = null,
			string childTypeEditor = null
		)
		{
			this.PropertyName = propertyName;
			this.DefaultValue = defaultValue;
			this.Hidden = hidden;
			this.ShowWhen = showWhen;
			this.HideWhen = hideWhen;
			this.ChildTypeEditor = childTypeEditor;
	}
	}
}
