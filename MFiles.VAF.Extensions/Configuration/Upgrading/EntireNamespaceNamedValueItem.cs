using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions.Configuration.Upgrading
{
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
}
