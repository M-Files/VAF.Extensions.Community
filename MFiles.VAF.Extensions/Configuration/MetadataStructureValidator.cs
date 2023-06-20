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
	public abstract class OwnerOrDefaultPropertyDefAttribute
		: Attribute
	{
		protected readonly ILogger Logger;
		public readonly string ObjectTypeReference;
		public OwnerOrDefaultPropertyDefAttribute(string objectTypeReference)
		{
			this.Logger = LogManager.GetLogger(this.GetType());
			this.ObjectTypeReference = objectTypeReference;
		}
		protected virtual ObjType GetObjType(Vault vault, Type t, object instance)
		{
			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));

			// Validate the object type reference.
			if (string.IsNullOrWhiteSpace(this.ObjectTypeReference))
			{
				this.Logger?.Info($"Object type reference was empty");
				return null;
			}

			// Get the value of the property or field.
			MFIdentifier objectTypeIdentifier = null;
			try
			{
				objectTypeIdentifier = t.GetProperty(this.ObjectTypeReference).GetValue(instance) as MFIdentifier
					?? t.GetField(this.ObjectTypeReference).GetValue(instance) as MFIdentifier;
			}
			catch (Exception e)
			{
				this.Logger?.Error(e, $"Could not retrieve object type identifier");
				return null;
			}

			// Ensure we have a resolved object type identifier.
			objectTypeIdentifier?.Resolve(vault, typeof(ObjType));
			if (null == objectTypeIdentifier || false == objectTypeIdentifier.IsResolved)
			{
				this.Logger?.Error($"Could not retrieve object type identifier");
				return null;
			}

			// Get the object type.
			return vault.ObjectTypeOperations.GetObjectType(objectTypeIdentifier.ID);
		}
		protected virtual ObjType GetObjType<T>(Vault vault, T instance)
			=> this.GetObjType(vault, typeof(T), instance);
		public abstract MFIdentifier Resolve(Vault vault, Type t, object instance);
		public MFIdentifier Resolve<T>(Vault vault, T instance)
			=> this.Resolve(vault, typeof(T), instance);
	}
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class DefaultPropertyDefAttribute
		: OwnerOrDefaultPropertyDefAttribute
	{
		public DefaultPropertyDefAttribute(string objectTypeReference)
			: base(objectTypeReference)
		{
		}

		/// <inheritdoc />
		public override MFIdentifier Resolve(Vault vault, Type t, object instance)
		{
			// Get the object type.
			var objType = this.GetObjType(vault, t, instance);

			// Sanity.
			if(null == objType)
			{
				this.Logger?.Error($"Could not load object type, so cannot resolve default property definition");
				return null;
			}

			// Create and return the identifier.
			var identifier = new MFIdentifier(objType.DefaultPropertyDef);
			identifier.Resolve(vault, typeof(PropertyDef));
			return identifier;
		}
	}
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class OwnerPropertyDefAttribute
		: OwnerOrDefaultPropertyDefAttribute
	{
		public OwnerPropertyDefAttribute(string objectTypeReference)
			: base(objectTypeReference)
		{
		}

		/// <inheritdoc />
		public override MFIdentifier Resolve(Vault vault, Type t, object instance)
		{
			// Get the object type.
			var objType = this.GetObjType(vault, t, instance);

			// Sanity.
			if (null == objType)
			{
				this.Logger?.Error($"Could not load object type, so cannot resolve owner property definition");
				return null;
			}

			// Create and return the identifier.
			var identifier = new MFIdentifier(objType.OwnerPropertyDef);
			identifier.Resolve(vault, typeof(PropertyDef));
			return identifier;
		}
	}
	internal class MetadataStructureValidator
		: VAF.Configuration.MetadataStructureValidator
	{
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
