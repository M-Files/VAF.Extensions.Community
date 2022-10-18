using MFilesAPI;
using System;
using System.Linq;

namespace MFiles.VAF.Extensions.Configuration.Upgrading
{
	/// <summary>
	/// Defines an item in Named Value Storage that can be read and written to.
	/// </summary>
	public interface INamedValueItem
	{
		/// <summary>
		/// The type of entry.
		/// </summary>
		MFNamedValueType NamedValueType { get; set; }

		/// <summary>
		/// The namespace for this item.
		/// </summary>
		string Namespace { get; set; }

		/// <summary>
		/// Whether the item is considered valid (e.g. it may be invalid if the <see cref="Namespace"/> is null).
		/// </summary>
		/// <returns><see langword="true"/> if the source is considered valid and can be used.</returns>
		bool IsValid();
		/// <summary>
		/// Retrieves the item(s) from Named Value Storage.
		/// </summary>
		/// <param name="manager">The manager to use to read data.</param>
		/// <param name="vault">The vault to read the data from.</param>
		/// <returns>The values at the defined location.</returns>
		NamedValues GetNamedValues(INamedValueStorageManager manager, Vault vault);

		/// <summary>
		/// Removes the item(s) from Named Value Storage. 
		/// </summary>
		/// <param name="manager">The manager to use to read data.</param>
		/// <param name="vault">The vault to read the data from.</param>
		/// <param name="namedValueNames">The item(s) within this namespace to remove.</param>
		void RemoveNamedValues(INamedValueStorageManager manager, Vault vault, params string[] namedValueNames);
		
		/// <summary>
		/// Writes the item(s) to Named Value Storage.
		/// </summary>
		/// <param name="manager">The manager to use to write data.</param>
		/// <param name="vault">The vault to write the data to.</param>
		/// <param name="namedValues">The item(s) to write.</param>
		void SetNamedValues(INamedValueStorageManager manager, Vault vault, NamedValues namedValues);
	}

	/// <summary>
	/// A base implementation of <see cref="INamedValueItem"/>.
	/// </summary>
	public abstract class NamedValueItemBase
		: INamedValueItem
	{
		/// <summary>
		/// Creates a named value item with the defined <paramref name="namedValueType"/>
		/// and <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namedValueType">The type of item(s) to be read/written.</param>
		/// <param name="namespace">The location of the item(s).</param>
		protected NamedValueItemBase(MFNamedValueType namedValueType, string @namespace)
		{
			this.NamedValueType = namedValueType;
			this.Namespace = @namespace;
		}

		/// <summary>
		/// The type of item(s) to be read/written.
		/// </summary>
		public MFNamedValueType NamedValueType { get; set; }

		/// <summary>
		/// The namespace location of the item(s).
		/// </summary>
		public string Namespace { get; set; }

		/// <inheritdoc />
		public abstract NamedValues GetNamedValues(INamedValueStorageManager manager, Vault vault);

		/// <inheritdoc />
		public abstract bool IsValid();

		/// <inheritdoc />
		public abstract void SetNamedValues(INamedValueStorageManager manager, Vault vault, NamedValues namedValues);

		/// <inheritdoc />
		public virtual void RemoveNamedValues(INamedValueStorageManager manager, Vault vault, params string[] namedValueNames)
			=> manager.RemoveNamedValues(vault, this.NamedValueType, this.Namespace, namedValueNames);
	}
}
