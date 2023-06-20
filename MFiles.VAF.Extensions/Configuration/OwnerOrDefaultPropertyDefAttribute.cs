using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Logging;
using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.Configuration
{
	/// <summary>
	/// A base class used for <see cref="OwnerPropertyDefAttribute"/> and <see cref="DefaultPropertyDefAttribute"/>.
	/// Collates common functionality.
	/// </summary>
	public abstract class OwnerOrDefaultPropertyDefAttribute
		: Attribute
	{
		/// <summary>
		/// The logger to use for logging issues.
		/// </summary>
		protected readonly ILogger Logger;

		/// <summary>
		/// The name of the object type property or field that will be used
		/// to retrieve the owner or default property definitions.
		/// </summary>
		public readonly string ObjectTypeReference;

		/// <summary>
		/// Instantiates an <see cref="OwnerOrDefaultPropertyDefAttribute"/>
		/// and sets <see cref="ObjectTypeReference"/> to <paramref name="objectTypeReference"/>.
		/// </summary>
		/// <param name="objectTypeReference"></param>
		public OwnerOrDefaultPropertyDefAttribute(string objectTypeReference)
		{
			this.Logger = LogManager.GetLogger(this.GetType());
			this.ObjectTypeReference = objectTypeReference;
		}

		/// <summary>
		/// Returns the object type to be used to find the default or owner property definitions.
		/// </summary>
		/// <param name="vault">The vault to resolve the object type in.</param>
		/// <param name="t">The type of <paramref name="instance"/>.</param>
		/// <param name="instance">The instance.  Must have a property or field with the name of <see cref="ObjectTypeReference"/>
		/// that is of type <see cref="MFIdentifier"/>.</param>
		/// <returns>The object type, or null if none is found.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="vault"/> is null.</exception>
		protected virtual ObjType GetObjType(Vault vault, Type t, object instance)
		{
			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (null == t && null != instance)
				t = instance.GetType();

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
				objectTypeIdentifier = t.GetProperty(this.ObjectTypeReference).GetValue(instance) as MFIdentifier;
				if (null == objectTypeIdentifier)
					return null;
			}
			catch
			{
				this.Logger?.Debug($"Could not find property {this.ObjectTypeReference} on {t.FullName}.");
			}
			try
			{
				objectTypeIdentifier = t.GetField(this.ObjectTypeReference).GetValue(instance) as MFIdentifier;
				if (null == objectTypeIdentifier)
					return null;
			}
			catch
			{
				this.Logger?.Debug($"Could not find field {this.ObjectTypeReference} on {t.FullName}.");
			}

			// Ensure we have a resolved object type identifier.
			objectTypeIdentifier?.Resolve(vault, typeof(ObjType));
			if (null == objectTypeIdentifier || false == objectTypeIdentifier.IsResolved)
			{
				this.Logger?.Warn($"Could not retrieve object type identifier ({t.FullName}.{this.ObjectTypeReference} = '{objectTypeIdentifier?.Alias}')");
				return null;
			}

			// Get the object type.
			return vault.ObjectTypeOperations.GetObjectType(objectTypeIdentifier.ID);
		}

		/// <summary>
		/// Returns the object type to be used to find the default or owner property definitions.
		/// </summary>
		/// <param name="vault">The vault to resolve the object type in.</param>
		/// <param name="instance">The instance.  Must have a property or field with the name of <see cref="ObjectTypeReference"/>
		/// that is of type <see cref="MFIdentifier"/>.</param>
		/// <returns>The object type, or null if none is found.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="vault"/> is null.</exception>
		protected virtual ObjType GetObjType(Vault vault, object instance)
			=> this.GetObjType(vault, instance?.GetType(), instance);

		/// <summary>
		/// Retrieves the owner or property definition (as defined by the class that implements
		/// this abstract method).
		/// </summary>
		/// <param name="vault">The vault to resolve the object type in.</param>
		/// <param name="t">The type of <paramref name="instance"/>.</param>
		/// <param name="instance">The instance.  Must have a property or field with the name of <see cref="ObjectTypeReference"/>
		/// that is of type <see cref="MFIdentifier"/>.</param>
		/// <returns>A <see cref="MFIdentifier"/> that represents the correct property definition, or <see langword="null"/> if not found.</returns>
		public abstract MFIdentifier Resolve(Vault vault, Type t, object instance);

		/// <summary>
		/// Retrieves the owner or property definition (as defined by the class that implements
		/// this abstract method).
		/// </summary>
		/// <param name="vault">The vault to resolve the object type in.</param>
		/// <param name="instance">The instance.  Must have a property or field with the name of <see cref="ObjectTypeReference"/>
		/// that is of type <see cref="MFIdentifier"/>.</param>
		/// <returns>A <see cref="MFIdentifier"/> that represents the correct property definition, or <see langword="null"/> if not found.</returns>
		public virtual MFIdentifier Resolve(Vault vault, object instance)
			=> this.Resolve(vault, instance?.GetType(), instance);
	}

	/// <summary>
	/// Defines that the property or field represents a property definition that should resolve to the default property
	/// of the given object type reference.
	/// </summary>
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
			if (null == objType)
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

	/// <summary>
	/// Defines that the property or field represents a property definition that should resolve to the owner property
	/// of the given object type reference.
	/// </summary>
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
}
