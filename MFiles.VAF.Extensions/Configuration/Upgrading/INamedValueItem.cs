using MFilesAPI;

namespace MFiles.VAF.Extensions.Configuration.Upgrading
{
	/// <summary>
	/// Defines an item in Named Value Storage that can be read and written to.
	/// </summary>
	public interface INamedValueItem
		: ISourceNamedValueItem, ITargetNamedValueItem
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
	}

	/// <summary>
	/// Defines an item in Named Value Storage that can be read.
	/// </summary>
	public interface ISourceNamedValueItem
	{
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
		/// Whether the item is considered valid (e.g. it may be invalid if the <see cref="Namespace"/> is null).
		/// </summary>
		/// <returns><see langword="true"/> if the source is considered valid and can be used.</returns>
		bool IsValid();
	}

	/// <summary>
	/// Defines an item in Named Value Storage that can be written to.
	/// </summary>
	public interface ITargetNamedValueItem
	{
		/// <summary>
		/// Writes the item(s) to Named Value Storage.
		/// </summary>
		/// <param name="manager">The manager to use to write data.</param>
		/// <param name="vault">The vault to write the data to.</param>
		/// <param name="namedValues">The item(s) to write.</param>
		void SetNamedValues(INamedValueStorageManager manager, Vault vault, NamedValues namedValues);

		/// <summary>
		/// Whether the item is considered valid (e.g. it may be invalid if the <see cref="Namespace"/> is null).
		/// </summary>
		/// <returns><see langword="true"/> if the source is considered valid and can be used.</returns>
		bool IsValid();
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

	/// <summary>
	/// Represents all items within a namespace.
	/// </summary>
	public class EntireNamespaceNamedValueItem
		: NamedValueItemBase
	{
		/// <summary>
		/// Creates a reference to all items in a given namespace.
		/// </summary>
		/// <param name="namedValueType">The type of item(s) to be read/written.</param>
		/// <param name="namespace">The location of the item(s).</param>
		public EntireNamespaceNamedValueItem(MFNamedValueType namedValueType, string @namespace)
			: base(namedValueType, @namespace)
		{
		}

		/// <inheritdoc />
		public override NamedValues GetNamedValues(INamedValueStorageManager manager, Vault vault)
			=> manager.GetNamedValues(vault, this.NamedValueType, this.Namespace);

		/// <inheritdoc />
		public override bool IsValid()
			=> false == string.IsNullOrWhiteSpace(this.Namespace);

		/// <inheritdoc />
		public override void SetNamedValues(INamedValueStorageManager manager, Vault vault, NamedValues namedValues)
			=> manager.SetNamedValues(vault, this.NamedValueType, this.Namespace, namedValues);
	}

	/// <summary>
	/// Represents a single named item within a namespace.
	/// </summary>
	public class SingleNamedValueItem
		: NamedValueItemBase, ISourceNamedValueItem, ITargetNamedValueItem
	{
		/// <summary>
		/// Creates a reference to a single named item (<paramref name="name"/>) within a given namespace.
		/// </summary>
		/// <param name="namedValueType">The type of item(s) to be read/written.</param>
		/// <param name="namespace">The location of the item(s).</param>
		/// <param name="name">The name/key of the single item.</param>
		public SingleNamedValueItem(MFNamedValueType namedValueType, string @namespace, string name)
			: base(namedValueType, @namespace)
		{
			this.Name = name;
		}

		/// <summary>
		/// The name (key) of the item within the namespace.
		/// </summary>
		public string Name { get; set; }

		/// <inheritdoc />
		public override NamedValues GetNamedValues(INamedValueStorageManager manager, Vault vault)
		{
			// Get all the items in this namespace.
			var allValuesInNamespace = manager.GetNamedValues(vault, this.NamedValueType, this.Namespace);

			// If we don't have a value then return null, so that it gets skipped.
			if (false == allValuesInNamespace.Contains(this.Name))
				return null;

			// Create a NamedValues instance just for the one item to move.
			var singleItem = new NamedValues();
			singleItem[this.Name] = allValuesInNamespace[this.Name];
			return singleItem;
		}

		/// <inheritdoc />
		public override bool IsValid()
			=> false == string.IsNullOrWhiteSpace(this.Namespace)
			&& false == string.IsNullOrWhiteSpace(this.Name);


		/// <inheritdoc />
		public override void SetNamedValues(INamedValueStorageManager manager, Vault vault, NamedValues namedValues)
			=> manager.SetNamedValues(vault, this.NamedValueType, this.Namespace, namedValues);
	}
}
