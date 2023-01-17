// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions;
using MFiles.VAF;
using MFilesAPI;
using System;
using System.Linq;
using MFiles.VAF.MultiserverMode;
using MFiles.VAF.AppTasks;
using MFiles.VAF.Common;
using System.Reflection;
using System.Collections;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Extensions.Dashboards.AsynchronousDashboardContent;
using System.Collections.Generic;
using MFiles.VAF.Extensions.Dashboards.LoggingDashboardContent;
using MFiles.VAF.Extensions.Dashboards.DevelopmentDashboardContent;
using System.Threading.Tasks;
using MFiles.VAF.Extensions.Logging;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{

		/// <inheritdoc />
		protected override void LoadHandlerMethods(Vault vault)
		{
			base.LoadHandlerMethods(vault);
			this.LoadTypedVaultExtensionMethods(vault, this);
		}

		/// <summary>
		/// Identifies vault extension methods decorated with <see cref="TypedVaultExtensionMethodAttribute"/>
		/// and ensures that they are wired up correctly.
		/// </summary>
		/// <param name="vault">The vault to use for any vault access.</param>
		/// <param name="source">The object to check for vault extension methods.</param>
		protected virtual void LoadTypedVaultExtensionMethods(Vault vault, object source)
		{
			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (null == source)
				return;
			if (null == this.vaultExtensionMethods)
				throw new InvalidOperationException("The vault extensions method dictionary was null.");

			// Add matching methods.
			foreach (var method in source.GetType().GetMethods(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic))
			{
				// Is it one we care about?
				var attribute = method.GetCustomAttribute<TypedVaultExtensionMethodAttribute>();
				if (null == attribute)
					continue;

				// Okay, register it.
				this.vaultExtensionMethods[attribute.VaultExtensionMethodName]
					= attribute.AsVaultExtensionMethodInfo(method, source);
			}
		}
	}
}
