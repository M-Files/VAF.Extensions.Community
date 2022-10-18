using MFiles.VAF.Configuration;
using MFiles.VaultApplications.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MFiles.VAF.Extensions.Configuration.Upgrading.Rules
{
	/// <summary>
	/// An upgrade rule that has been declared via attributes.
	/// </summary>
	public class DeclaredConfigurationUpgradeRule
		: SingleNamedValueItemUpgradeRuleBase
	{
		/// <summary>
		/// The logger for this class.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger<DeclaredConfigurationUpgradeRule>();

		public DeclaredConfigurationUpgradeRule(VaultApplicationBase vaultApplication, Version migrateFromVersion, Version migrateToVersion, MethodInfo methodInfo)
			: base(vaultApplication, migrateFromVersion, migrateToVersion)
		{
			this.MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
		}

		public DeclaredConfigurationUpgradeRule(ISingleNamedValueItem readFromAndWriteTo, Version migrateFromVersion, Version migrateToVersion, MethodInfo methodInfo)
			: base(readFromAndWriteTo, migrateFromVersion, migrateToVersion)
		{
			this.MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
		}

		public DeclaredConfigurationUpgradeRule(ISingleNamedValueItem readFrom, ISingleNamedValueItem writeTo, Version migrateFromVersion, Version migrateToVersion, MethodInfo methodInfo)
			: base(readFrom, writeTo, migrateFromVersion, migrateToVersion)
		{
			this.MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
		}

		/// <summary>
		/// The .NET type of the configuration class that this rule upgrades to.
		/// </summary>
		public Type UpgradeToType { get; set; }

		/// <summary>
		/// The .NET type of the configuration class that this rule upgrades from.
		/// </summary>
		public Type UpgradeFromType { get; set; }

		/// <summary>
		/// The defined method that will execute the upgrade.
		/// </summary>
		public MethodInfo MethodInfo { get; set; }

		/// <inheritdoc />
		protected override string Convert(string input)
		{
			// Sanity.
			if (null == this.UpgradeFromType)
			{
				this.Logger?.Fatal($"Cannot upgrade configuration as from type is null.");
				throw new InvalidOperationException("Cannot upgrade configuration as from type is null.");
			}
			if (null == this.UpgradeToType)
			{
				this.Logger?.Fatal($"Cannot upgrade configuration as to type is null.");
				throw new InvalidOperationException("Cannot upgrade configuration as to type is null.");
			}
			if (null == this.MethodInfo)
			{
				this.Logger?.Fatal($"Cannot upgrade configuration as upgrade method is null.");
				throw new InvalidOperationException("Cannot upgrade configuration as upgrade method is null.");
			}

			// Handle empty data.
			if (string.IsNullOrWhiteSpace(input))
				input = "{}";

			this.Logger?.Info($"Converting configuration version from {this.MigrateFromVersion} to {this.MigrateToVersion}.");

			// Get the input object.
			var inputObj = base.JsonConvert.Deserialize(input, this.UpgradeFromType);

			// Convert it.
			object outputObj = null;
			try
			{
				// What arguments need to be passed?
				var arguments = new List<object>();
				{
					var parameters = this.MethodInfo.GetParameters();
					switch (parameters?.Length ?? 0)
					{
						case 1:
							if (parameters[0].ParameterType == typeof(string))
							{
								this.Logger?.Trace("Parameter is of type string, so passing raw NVS data.");
								arguments.Add(input);
							}
							else if (parameters[0].ParameterType == typeof(JObject))
							{
								this.Logger?.Trace("Parameter is of type JObject, so parsing JObject.");
								arguments.Add(JObject.Parse(input));
							}
							else
							{
								arguments.Add(inputObj);
							}
							break;
						case 2:
							arguments.Add(inputObj);
							arguments.Add(JObject.Parse(input));
							break;
						default:
							this.Logger?.Fatal($"Cannot upgrade configuration from {this.UpgradeFromType} to {this.UpgradeToType} as method {this.MethodInfo.DeclaringType.FullName}.{this.MethodInfo.Name} has an unexpected number of parameters ({(parameters?.Length ?? 0)}).");
							return input;
					}
				}
				if (this.MethodInfo.IsStatic)
				{
					// If static we don't need an object reference.
					outputObj = this.MethodInfo.Invoke(null, arguments.ToArray());
				}
				else
				{
					// Create an instance.
					outputObj = Activator.CreateInstance(this.UpgradeToType);

					// If it returns something then store it.
					if (this.MethodInfo.ReturnType != typeof(void))
					{
						outputObj = this.MethodInfo.Invoke(outputObj, arguments.ToArray());
					}
					else
					{
						// Throw away the return value.
						this.MethodInfo.Invoke(outputObj, arguments.ToArray());
					}
					
					
				}
			}
			catch (Exception ex)
			{
				this.Logger?.Fatal(ex, $"Exception whilst upgrading configuration from {this.UpgradeFromType} to {this.UpgradeToType}.");
			}

			// Deserialise back.
			return base.JsonConvert.Serialize(outputObj);

		}
	}
}
