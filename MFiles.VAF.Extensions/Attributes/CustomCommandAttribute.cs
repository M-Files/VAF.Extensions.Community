using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Interfaces.Domain;
using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Declares that the associated method should be exposed via a command,
	/// and run when the command is executed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true)]
	public class CustomCommandAttribute
		: Attribute
	{
		/// <inheritdoc cref="CustomDomainCommand.ID"/>
		public string CommandId { get; set; }

		/// <inheritdoc cref="CustomDomainCommand.Label"/>
		public string Label { get; set; }

		/// <inheritdoc cref="CustomDomainCommand.HelpText"/>
		public string HelpText { get; set; }

		/// <inheritdoc cref="CustomDomainCommand.ConfirmMessage"/>
		public string ConfirmMessage { get; set; }

		/// <inheritdoc cref="CustomDomainCommand.Blocking"/>
		public bool Blocking { get; set; }

		public CustomCommandAttribute(string label)
		{
			this.Label = label;
		}
	}
}
