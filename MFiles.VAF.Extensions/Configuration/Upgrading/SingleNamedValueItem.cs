using MFilesAPI;
using System;
using System.Linq;

namespace MFiles.VAF.Extensions.Configuration.Upgrading
{

	public interface ISingleNamedValueItem
		: INamedValueItem
	{

		/// <summary>
		/// The name (key) of the item within the namespace.
		/// </summary>
		string Name { get; set; }
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

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{this.Namespace}.{this.Name} ({this.NamedValueType})";
		}
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
}
