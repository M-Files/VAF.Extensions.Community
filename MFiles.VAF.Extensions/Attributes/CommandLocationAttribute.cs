using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Interfaces.Domain;
using System;

namespace MFiles.VAF.Extensions
{
	public abstract class CommandLocationAttribute
		: Attribute
	{
		public abstract ICommandLocation ToCommandLocation();
	}

	/// <summary>
	/// Defines that the custom command should be shown on the bar at the top of the dashboard.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class ButtonBarCommandLocationAttribute
		: CommandLocationAttribute
	{
		/// <inheritdoc cref="ButtonBarCommandLocation.Priority"/>
		public int Priority { get; set; }

		/// <inheritdoc cref="ButtonBarCommandLocation.Style"/>
		public CommandButtonStyle Style { get; set; }

		public override ICommandLocation ToCommandLocation()
		{
			return new ButtonBarCommandLocation()
			{
				Priority = this.Priority,
				Style = this.Style

			};
		}
	}

	/// <summary>
	/// Defines that the custom command should be shown in the context menu when a user
	/// right-clicks on the "Configuration" node for this application.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class ConfigurationMenuCommandLocationAttribute
		: AbstractMenuCommandLocationAttribute
	{
		public override ICommandLocation ToCommandLocation()
		{
			return new ConfigurationMenuCommandLocation()
			{
				Priority = this.Priority,
				Icon = this.Icon,
				NoSeparatorAfter = this.NoSeparatorAfter,
				SeparatorBefore = this.SeparatorBefore
			};
		}
	}

	/// <summary>
	/// Defines that the custom domain should be shown in the context menu when a user
	/// right-clicks on the root domain node for this application.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class DomainMenuCommandLocationAttribute
		: AbstractMenuCommandLocationAttribute
	{
		public override ICommandLocation ToCommandLocation()
		{
			return new DomainMenuCommandLocation()
			{
				Priority = this.Priority,
				Icon = this.Icon,
				NoSeparatorAfter = this.NoSeparatorAfter,
				SeparatorBefore = this.SeparatorBefore
			};
		}
	}

	public abstract class AbstractMenuCommandLocationAttribute
		: CommandLocationAttribute
	{
		/// <inheritdoc cref="AbstractMenuCommandLocation.Priority"/>
		public int Priority { get; set; }

		/// <inheritdoc cref="AbstractMenuCommandLocation.Icon"/>
		public string Icon { get; set; }

		/// <inheritdoc cref="AbstractMenuCommandLocation.NoSeparatorAfter"/>
		public bool NoSeparatorAfter { get; set; }

		/// <inheritdoc cref="AbstractMenuCommandLocation.SeparatorBefore"/>
		public bool SeparatorBefore { get; set; }
	}
}
