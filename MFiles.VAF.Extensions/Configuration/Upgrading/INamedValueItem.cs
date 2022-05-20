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

	public interface IEntireNamespaceNamedValueItem
		: INamedValueItem
	{

	}

	/// <summary>
	/// Represents all items within a namespace.
	/// </summary>
	public class EntireNamespaceNamedValueItem
		: NamedValueItemBase, IEntireNamespaceNamedValueItem
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
		{
			manager = manager ?? throw new ArgumentNullException(nameof(manager));
			return manager.GetNamedValues(vault, this.NamedValueType, this.Namespace);
		}

		/// <inheritdoc />
		public override bool IsValid()
			=> false == string.IsNullOrWhiteSpace(this.Namespace);

		/// <inheritdoc />
		public override void SetNamedValues(INamedValueStorageManager manager, Vault vault, NamedValues namedValues)
		{
			manager = manager ?? throw new ArgumentNullException(nameof(manager));
			manager.SetNamedValues(vault, this.NamedValueType, this.Namespace, namedValues);
		}
	}

	public interface ISingleNamedValueItem
		: INamedValueItem
	{

		/// <summary>
		/// The name (key) of the item within the namespace.
		/// </summary>
		string Name { get; set; }
	}
	public static class ISingleNamedValueItemExtensionMethods
	{
		public static bool TryRead(this ISingleNamedValueItem namedValueItem, Vault vault, INamedValueStorageManager namedValueStorageManager, IJsonConvert jsonConvert, out string data)
		{
			data = null;

			// Sanity.
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (null == namedValueStorageManager)
				throw new ArgumentNullException(nameof(namedValueStorageManager));
			if (null == jsonConvert)
				throw new ArgumentNullException(nameof(jsonConvert));
			if (null == namedValueItem)
				return false;

			// Attempt to retrieve the data from NVS.
			var namedValues = namedValueStorageManager.GetNamedValues(vault, namedValueItem.NamedValueType, namedValueItem.Namespace);
			if (null == namedValues || false == namedValues.Names.Cast<string>().Contains(namedValueItem.Name))
				return false;

			// Read the data.
			data = namedValues[namedValueItem.Name]?.ToString();
			return false == string.IsNullOrWhiteSpace(data);
		}

		public static bool TryRead<TOutputType>
		(
			this ISingleNamedValueItem namedValueItem, 
			Vault vault, 
			INamedValueStorageManager namedValueStorageManager, 
			IJsonConvert jsonConvert, 
			out string data,
			out Version version,
			out TOutputType output
		)
			where TOutputType : VersionedConfigurationBase
		{

			data = null;
			version = null;
			output = null;

			// Use the other overload.
			if (false == namedValueItem.TryRead(vault, namedValueStorageManager, jsonConvert, out data))
				return false;

			// Deserialize the object
			output = jsonConvert.Deserialize<TOutputType>(data);

			// If we can, grab the version.
			version = output?.Version ?? new Version("0.0");

			// Did we get it?
			return output != default;
		}

		public static bool TryRead
		(
			this ISingleNamedValueItem namedValueItem,
			Vault vault,
			INamedValueStorageManager namedValueStorageManager,
			IJsonConvert jsonConvert,
			out string data,
			out Version version
		) => namedValueItem.TryRead<VersionedConfigurationBase>(vault, namedValueStorageManager, jsonConvert, out data, out version, out _);
	}

	/// <summary>
	/// Represents a single named item within a namespace.
	/// </summary>
	public class SingleNamedValueItem
		: NamedValueItemBase, ISingleNamedValueItem
	{
		/// <summary>
		/// Returns a <see cref="SingleNamedValueItem"/> instance pointing at the location used by the latest VAF release.
		/// </summary>
		/// <param name="vaultApplication"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static SingleNamedValueItem ForLatestVAFVersion(VaultApplicationBase vaultApplication) =>
			new SingleNamedValueItem
				(
					MFNamedValueType.MFSystemAdminConfiguration,
					vaultApplication?.GetType()?.FullName ?? throw new ArgumentNullException(nameof(vaultApplication)),
					"configuration"
				);

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

		/// <inheritdoc />
		public string Name { get; set; }

		/// <inheritdoc />
		public override NamedValues GetNamedValues(INamedValueStorageManager manager, Vault vault)
		{
			// Sanity.
			if (null == manager)
				throw new ArgumentNullException(nameof(manager));
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (false == this.IsValid())
				throw new InvalidOperationException("This object is not in a valid state");

			// Get all the items in this namespace.
			var allValuesInNamespace = manager.GetNamedValues(vault, this.NamedValueType, this.Namespace);
			if (null == allValuesInNamespace)
				return null;

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
		{
			// Sanity.
			if (null == manager)
				throw new ArgumentNullException(nameof(manager));
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));
			if (false == this.IsValid())
				throw new InvalidOperationException("This object is not in a valid state");

			if ((namedValues?.Names?.Count ?? 0) == 0)
				return;
			if (namedValues.Names.Count != 1)
				throw new ArgumentException("Cannot move multiple named values to a single location.", nameof(namedValues));

			// Copy to a new instance for the target key.
			var toSet = new NamedValues();
			toSet[this.Name] = namedValues[namedValues.Names.Cast<string>().First()];

			manager.SetNamedValues(vault, this.NamedValueType, this.Namespace, toSet);
		}
	}
}
