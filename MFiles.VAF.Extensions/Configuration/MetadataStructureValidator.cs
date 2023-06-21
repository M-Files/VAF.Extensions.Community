using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.JsonAdaptor;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Validation;
using MFilesAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Configuration
{
	internal class MetadataStructureValidator
		: VAF.Configuration.MetadataStructureValidator
	{
		/// <summary>
		/// The logger for the metadata structure validator.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(MetadataStructureValidator));

		/// <inheritdoc />
		public override bool ValidateItem(Vault vault, IConfiguration configuration, object item, ValidationResultBase validationResult, Assembly[] containingAssemblies = null, int level = 10)
		{
			// Suppress validation exceptions if the configuration is null
			// (which can happen if it fails deserialization).
			if (item == null)
			{
				validationResult.ReportCustomFailure
				(
					configuration,
					MFMetadataStructureItem.MFMetadataStructureItemNone,
					"",
					"The configuration was not provided; possible deserialization error (check configuration class structure).",
					true
				);
				this.Logger?.Warn($"The provided configuration was null; possible deserialization error (check configuration class structure).");
				return true;
			}
			return base.ValidateItem(vault, configuration, item, validationResult, containingAssemblies, level);
		}

		/// <inheritdoc />
		protected virtual void ResolveOwnerOrDefaultPropertyDefs(Vault vault, object item)
		{
			// Sanity.
			if (item == null)
				return;

			// Find child properties/fields that might have the attributes we care about.
			var children = this.GetChildren(item);
			foreach ( var child in children )
			{
				// Sanity.
				if (null == child)
					continue;
				this.Logger?.Trace($"Checking {child.DeclaringType?.FullName}.{child.Name} for OwnerOrDefaultPropertyDefAttribute attributes.");

				// If it doesn't have the attribute we care about then skip.
				if (!(child?.GetCustomAttribute(typeof(OwnerOrDefaultPropertyDefAttribute), true) is OwnerOrDefaultPropertyDefAttribute attr))
				{
					this.Logger?.Trace($"No attribute found; skipping");
					continue;
				}

				// Try to resolve the value.
				this.Logger?.Debug($"{attr.GetType().Name} attribute found on {child.DeclaringType?.FullName}.{child.Name}.");
				var identifier = attr.Resolve(vault, item.GetType(), item);
				if (null == identifier)
				{
					this.Logger?.Info($"Could not resolve the object type associated with {child.DeclaringType?.FullName}.{child.Name} (looked for populated configuration property named {attr.ObjectTypeReference}).");
					continue;
				}

				// Set the value.
				{
					this.Logger?.Debug($"Setting {child.DeclaringType?.FullName}.{child.Name} to {identifier.ID}.");
					if (child.MemberType == MemberTypes.Field)
						((FieldInfo)child).SetValue(item, identifier);
					if (child.MemberType == MemberTypes.Property)
						((PropertyInfo)child).SetValue(item, identifier);
				}
			}
		}

		/// <inheritdoc />
		protected override bool ValidateItemInternal(Vault vault, IConfiguration configuration, object item, ValidationResultBase validationResult, object parent, MemberInfo member, int level, Assembly[] containingAssemblies, HashSet<object> handledItems)
		{
			// Call the base implementation.
			var retValue = base.ValidateItemInternal(vault, configuration, item, validationResult, parent, member, level, containingAssemblies, handledItems);

			// Update the ones we care about.
			this.ResolveOwnerOrDefaultPropertyDefs(vault, item);

			// Return the base implementation return value.
			return retValue;
		}
	}
}
