using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	/// <summary>
	/// Helper methods for testing whether properties have correct attributes.
	/// </summary>
	public static class PropertyInfoExtensionMethods
	{
		/// <summary>
		/// Returns the instance of the <typeparamref name="TAttribute"/> attribute on <paramref name="property"/>.
		/// </summary>
		/// <typeparam name="TAttribute">The attribute type to return.</typeparam>
		/// <param name="property">The property to inspect.</param>
		/// <param name="inherit">true to search this member's inheritance chain.</param>
		/// <returns>The attribute instance.</returns>
		public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this PropertyInfo property, bool inherit = true)
			where TAttribute : Attribute
		{
			return property
				.GetCustomAttributes(typeof(TAttribute), inherit)
				.Cast<TAttribute>();
		}

		/// <summary>
		/// Returns the instance of the <paramref name="attributeType"/> attribute on <paramref name="property"/>.
		/// </summary>
		/// <param name="attributeType">The attribute type to return.</typeparam>
		/// <param name="property">The property to inspect.</param>
		/// <param name="inherit">true to search this member's inheritance chain.</param>
		/// <returns>The attribute instance.</returns>
		public static IEnumerable<Attribute> GetCustomAttributes(this PropertyInfo property, Type attributeType, bool inherit = true)
		{
			return property
				.GetCustomAttributes(attributeType, inherit)
				.Cast<Attribute>();
		}
		/// <summary>
		/// Returns the first instance of the <typeparamref name="TAttribute"/> attribute on <paramref name="property"/>.
		/// </summary>
		/// <typeparam name="TAttribute">The attribute type to return.</typeparam>
		/// <param name="property">The property to inspect.</param>
		/// <param name="inherit">true to search this member's inheritance chain.</param>
		/// <returns>The attribute instance.</returns>
		public static TAttribute GetCustomAttribute<TAttribute>(this PropertyInfo property, bool inherit = true)
			where TAttribute : Attribute
		{
			return property
				.GetCustomAttributes<TAttribute>(inherit)
				.FirstOrDefault();
		}

		/// <summary>
		/// Returns the first instance of the <paramref name="attributeType"/> attribute on <paramref name="property"/>.
		/// </summary>
		/// <param name="attributeType">The attribute type to return.</typeparam>
		/// <param name="property">The property to inspect.</param>
		/// <param name="inherit">true to search this member's inheritance chain.</param>
		/// <returns>The attribute instance.</returns>
		public static Attribute GetCustomAttribute(this PropertyInfo property, Type attributeType, bool inherit = true)
		{
			return property
				.GetCustomAttributes(attributeType, inherit)
				.FirstOrDefault() as Attribute;
		}

		/// <summary>
		/// Returns true if <paramref name="property"/> is marked with at least one <typeparamref name="TAttribute"/>.
		/// </summary>
		/// <typeparam name="TAttribute">The attribute type to query.</typeparam>
		/// <param name="property">The property to inspect.</param>
		/// <param name="inherit">true to search this member's inheritance chain.</param>
		/// <returns>true if the attribute exists, false otherwise.</returns>
		public static bool HasCustomAttribute<TAttribute>(this PropertyInfo property, bool inherit = true)
			where TAttribute : Attribute
		{
			return property.GetCustomAttribute<TAttribute>(inherit) != null;
		}

		/// <summary>
		/// Returns true if <paramref name="property"/> is marked with at least one <paramref name="attributeType"/>.
		/// </summary>
		/// <param name="attributeType">The attribute type to query.</typeparam>
		/// <param name="property">The property to inspect.</param>
		/// <param name="inherit">true to search this member's inheritance chain.</param>
		/// <returns>true if the attribute exists, false otherwise.</returns>
		public static bool HasCustomAttribute(this PropertyInfo property, Type attributeType, bool inherit = true)
		{
			return property.GetCustomAttribute(attributeType, inherit) != null;
		}
	}
}
