using MFiles.VAF.Configuration.AdminConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards.Commands.CustomDomainCommandResolution
{
	public class ImportPackageCustomDomainCommandResolver<TSecureConfiguration>
		: AttributeCustomDomainCommandResolver
		where TSecureConfiguration : class, new()
	{
		/// <summary>
		/// The vault application that this resolver is running within.
		/// </summary>
		protected ConfigurableVaultApplicationBase<TSecureConfiguration> VaultApplication { get; }

		/// <summary>
		/// Creates an instance of <see cref="DefaultCustomDomainCommandResolver{TSecureConfiguration}"/>
		/// and includes the provided <paramref name="vaultApplication"/> in the list of things to resolve against.
		/// </summary>
		/// <param name="vaultApplication">The vault application this is running within.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="vaultApplication"/> is <see langword="null"/>.</exception>
		public ImportPackageCustomDomainCommandResolver(ConfigurableVaultApplicationBase<TSecureConfiguration> vaultApplication)
			: base(vaultApplication)
		{
			this.VaultApplication = vaultApplication
				?? throw new ArgumentNullException(nameof(vaultApplication));
		}

		protected virtual string GetImportPackageDomainCommandId(Type type, ReplicationPackageAttribute attribute)
		{
			if (null == type)
				throw new ArgumentNullException(nameof(type));
			if (null == attribute)
				throw new ArgumentNullException(nameof(attribute));
			return $"{type.FullName}-{attribute.PackagePath}-Import";
		}

		protected virtual string GetPreviewPackageDomainCommandId(Type type, ReplicationPackageAttribute attribute)
		{
			if (null == type)
				throw new ArgumentNullException(nameof(type));
			if (null == attribute)
				throw new ArgumentNullException(nameof(attribute));
			return $"{type.FullName}-{attribute.PackagePath}-Preview";
		}

		/// <summary>
		/// Gets import-package commands from the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type to load the commands from.</param>
		/// <param name="instance">The instance of the type.</param>
		/// <returns>Any commands exposed via attributes.</returns>
		protected override IEnumerable<CustomDomainCommand> GetCustomDomainCommandsFromType(Type type, object instance)
		{
			// Sanity.
			if (null == type)
				yield break;

			// Get all [ReplicationPackageAttributes] on the class.
			var attributes = type.GetCustomAttributes<ReplicationPackageAttribute>();
			if ((attributes?.Count() ?? 0) == 0)
				yield break;

			// Return commands as appropriate.
			foreach (var attribute in attributes)
			{

				// Set the command IDs.
				if (string.IsNullOrWhiteSpace(attribute.ImportCommandId))
					attribute.ImportCommandId = this.GetImportPackageDomainCommandId(type, attribute);
				if (string.IsNullOrWhiteSpace(attribute.PreviewCommandId))
					attribute.PreviewCommandId = this.GetPreviewPackageDomainCommandId(type, attribute);

				// Generate the import command.
				var importCommand = new ImportReplicationPackageDashboardCommand<TSecureConfiguration>
				(
					this.VaultApplication,
					attribute.ImportCommandId,
					attribute.ImportLabel,
					attribute.PackagePath
				)
				{
					Blocking = true,
					RequiresImporting = true
				};
				yield return importCommand;

				// Should we also do a preview command?
				if (!attribute.PreviewPackageBeforeImport)
					continue;

				// Create the preview command.
				var previewCommand = new PreviewReplicationPackageDashboardCommand<TSecureConfiguration>
				(
					this.VaultApplication,
					attribute.PreviewCommandId,
					attribute.PreviewLabel,
					attribute.PackagePath,
					importCommand
				)
				{
					Blocking = true
				};
				importCommand.PreviewCommand = previewCommand;
				yield return previewCommand;
			}
		}
	}
}
