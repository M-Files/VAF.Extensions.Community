using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.JsonAdaptor;
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
				// If it doesn't have the attribute we care about then skip.
				var attr = child?.GetCustomAttribute(typeof(OwnerOrDefaultPropertyDefAttribute), true) as OwnerOrDefaultPropertyDefAttribute;
				if (null == attr)
					continue;

				// Try to resolve the value.
				var identifier = attr.Resolve(vault, item.GetType(), item);
				if (null == identifier)
					continue;

				// Set the value.
				{
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
