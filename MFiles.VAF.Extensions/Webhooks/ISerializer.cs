using MFiles.VAF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Webhooks
{

    public interface ISerializer
	{
		/// <summary>
		/// If <see langword="true"/> then this serializer can serialize <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type to serialize.</param>
		/// <returns><see langword="true"/> if serialization is supported, <see langword="false"/> otherwise.</returns>	
		bool CanSerialize(Type type);

		/// <summary>
		/// If <see langword="true"/> then this serializer can deserialize <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type to deserialize.</param>
		/// <returns><see langword="true"/> if deserialization is supported, <see langword="false"/> otherwise.</returns>	
		bool CanDeserialize(Type type);

		/// <summary>
		/// Serializes <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The item to serialize.</param>
		/// <returns>The serialized representation.</returns>
        byte[] Serialize(object input);

		/// <summary>
		/// Serializes <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The item to serialize.</param>
		/// <returns>The serialized representation.</returns>
		byte[] Serialize<T>(T input);

		/// <summary>
		/// Deserializes <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The data to serialize.</param>
		/// <param name="t">The type to deserialize to.</param>
		/// <returns>The instance.</returns>
		object Deserialize(byte[] input, Type t);

		/// <summary>
		/// Deserializes <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The data to serialize.</param>
		/// <typeparam name="T">The type to deserialize to.</typeparam>
		/// <returns>The instance.</returns>
		T Deserialize<T>(byte[] input);
    }
}
