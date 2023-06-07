using MFiles.VAF.Configuration;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public interface IObjTypeMFIdentifier
	{
		Type GetUnresolvedType();
		bool Resolved { get; }
		MFIdentifier Resolve(Vault vault, Type type, bool forceRefresh);
		string Alias { get; } 
		int ID { get; }
	}
	public class ObjTypeMFIdentifier
		: MFIdentifier, IObjTypeMFIdentifier
	{
		public ObjTypeMFIdentifier() {}
		public ObjTypeMFIdentifier(object source) : base(source) {}
		public ObjTypeMFIdentifier(int id) : base(id) { }
		public ObjTypeMFIdentifier(string str) : base(str) { }
		public ObjTypeMFIdentifier(ObjID objID) : base(objID) { }
	}
	public static class IObjTypeMFIdentifierExtensionMethods
	{
		/// <summary>
		/// Returns the <see cref="ObjType"/> for the given <paramref name="identifier"/>.
		/// </summary>
		/// <param name="identifier">The identifier to retrieve.</param>
		/// <param name="vault">The vault to use for resolution, if <paramref name="identifier"/> is not already resolved.</param>
		/// <returns>The ID of the property definition to use for "owner" relationships.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="identifier"/> is null, or <paramref name="vault"/> is null and <paramref name="identifier"/> needs to be resolved.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <paramref name="identifier"/> does not point to an object type, or resolution was unsuccessful.</exception>
		private static ObjType GetObjType(this IObjTypeMFIdentifier identifier, Vault vault)
		{
			// Sanity.
			identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));

			// Throw if the identifier isn't an object type.
			{
				// The unresolved type may be null if no resolution has occurred.
				var unresolvedType = identifier.GetUnresolvedType();
				if (null != unresolvedType && typeof(ObjType) != unresolvedType)
					throw new InvalidOperationException($"The identifier does not refer to an object type.");

				// Note: null type is allowed here because we'll try to resolve if needed below.
			}

			// Attempt to resolve if needed.
			if (false == identifier.Resolved)
			{
				// We need a vault to resolve.
				vault = vault ?? throw new ArgumentNullException(nameof(vault));
				identifier.Resolve(vault, typeof(ObjType), forceRefresh: false);

				// Throw if we can't resolve.
				if (false == identifier.Resolved)
					throw new InvalidOperationException($"Object type with alias {identifier.Alias} could not be found");
			}

			// Bubble any exceptions.
			return vault.ObjectTypeOperations.GetObjectType(identifier.ID);
		}

		/// <summary>
		/// Retrieves the <see cref="ObjTypeClass.OwnerPropertyDef"/> value for the given <paramref name="identifier"/>.
		/// </summary>
		/// <param name="identifier">The identifier to retrieve.</param>
		/// <param name="vault">The vault to use for resolution, if <paramref name="identifier"/> is not already resolved.</param>
		/// <returns>The ID of the property definition to use for "owner" relationships.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="identifier"/> is null, or <paramref name="vault"/> is null and <paramref name="identifier"/> needs to be resolved.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <paramref name="identifier"/> does not point to an object type, or resolution was unsuccessful.</exception>
		public static MFIdentifier GetOwnerPropertyDef(this IObjTypeMFIdentifier identifier, Vault vault)
		{
			// Sanity.
			identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));

			// Try to get the object type.
			try
			{
				return identifier
					.GetObjType(vault)
					.OwnerPropertyDef;
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Could not retrieve the owner property definition", e);
			}
		}

		/// <summary>
		/// Retrieves the <see cref="ObjTypeClass.DefaultPropertyDef"/> value for the given <paramref name="identifier"/>.
		/// </summary>
		/// <param name="identifier">The identifier to retrieve.</param>
		/// <param name="vault">The vault to use for resolution, if <paramref name="identifier"/> is not already resolved.</param>
		/// <returns>The ID of the property definition to use for "default" relationships.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="identifier"/> is null, or <paramref name="vault"/> is null and <paramref name="identifier"/> needs to be resolved.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <paramref name="identifier"/> does not point to an object type, or resolution was unsuccessful.</exception>
		public static MFIdentifier GetDefaultPropertyDef(this IObjTypeMFIdentifier identifier, Vault vault)
		{
			// Sanity.
			identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));

			// Try to get the object type.
			try
			{
				return identifier
					.GetObjType(vault)
					.DefaultPropertyDef;
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Could not retrieve the default property definition", e);
			}
		}

		/// <summary>
		/// Retrieves the <see cref="ObjTypeClass.OwnerType"/> value for the given <paramref name="identifier"/>.
		/// </summary>
		/// <param name="identifier">The identifier to retrieve.</param>
		/// <param name="vault">The vault to use for resolution, if <paramref name="identifier"/> is not already resolved.</param>
		/// <returns>The ID of the owning type, or null if this type does not have an owner.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="identifier"/> is null, or <paramref name="vault"/> is null and <paramref name="identifier"/> needs to be resolved.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <paramref name="identifier"/> does not point to an object type, or resolution was unsuccessful.</exception>
		public static int? GetOwnerType(this IObjTypeMFIdentifier identifier, Vault vault)
		{
			// Sanity.
			identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));

			// Try to get the object type.
			try
			{
				var objType = identifier
					.GetObjType(vault);
				return objType.HasOwnerType
					? (int?)objType.OwnerType : null;
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Could not retrieve the owner type", e);
			}
		}
	}
}
