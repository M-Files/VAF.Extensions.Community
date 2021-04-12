using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	internal static class TypeExtensionMethods
	{
		/// <summary>
		/// Locates the values of properties and fields on <paramref name="input"/>
		/// of type <typeparamref name="TReturnType"/>.
		/// </summary>
		/// <typeparam name="TReturnType">The property/field type.  Will also match items that are assignable from this type.</typeparam>
		/// <param name="input">The type to search.</param>
		/// <param name="instance">The instance of <paramref name="input"/> that property/field values should be loaded from.</param>
		/// <param name="bindingFlags">The binding flags to identify the properties and fields to use.</param>
		/// <param name="includeBackingFields">If <see langword="true"/> then backing fields for properties will be returned.</param>
		public static IQueryable<TReturnType> GetPropertiesAndFieldsOfType<TReturnType>
		   (
				this Type input,
				object instance,
				BindingFlags bindingFlags = BindingFlags.Instance
					| BindingFlags.FlattenHierarchy
					| BindingFlags.Public
					| BindingFlags.NonPublic,
				bool includeBackingFields = false
		   )
			   where TReturnType : class
		{
			IQueryable<TReturnType> items;
			input.GetPropertiesAndFieldsOfType
			(
				instance, 
				out items,
				bindingFlags,
				includeBackingFields
			);
			return items;
		}

		/// <summary>
		/// Locates the values of properties and fields on <paramref name="input"/>
		/// of type <typeparamref name="TReturnType"/>.
		/// </summary>
		/// <typeparam name="TReturnType">The property/field type.  Will also match items that are assignable from this type.</typeparam>
		/// <param name="input">The type to search.</param>
		/// <param name="instance">The instance of <paramref name="input"/> that property/field values should be loaded from.</param>
		/// <param name="items">The value of the properties.</param>
		/// <param name="bindingFlags">The binding flags to identify the properties and fields to use.</param>
		/// <param name="includeBackingFields">If <see langword="true"/> then backing fields for properties will be returned.</param>
		public static void GetPropertiesAndFieldsOfType<TReturnType>
		(
			this Type input,
			object instance,
			out IQueryable<TReturnType> items,
			BindingFlags bindingFlags = BindingFlags.Instance
				| BindingFlags.FlattenHierarchy
				| BindingFlags.Public
				| BindingFlags.NonPublic,
			bool includeBackingFields = false
		)
			where TReturnType : class
		{
			// Use GetPropertiesAndFieldsOfType to get the properties and fields.
			input.GetPropertiesAndFieldsOfType<TReturnType>
			(
				out IQueryable<PropertyInfo> p, 
				out IQueryable<FieldInfo> f,
				bindingFlags,
				includeBackingFields
			);

			// Create our list.
			var list = new List<TReturnType>();

			// Filter properties by the attribute.
			list.AddRange
			(
				p
					.Where(mi => mi.CanRead)
					.Select(mi => mi.GetValue(instance) as TReturnType)
			);

			// Filter fields by the attribute.
			list.AddRange
			(
				f
					.Select(mi => mi.GetValue(instance) as TReturnType)
			);

			// Return our list.
			items = list.AsQueryable();
		}

		/// <summary>
		/// Locates the values of properties and fields on <paramref name="input"/>
		/// of type <typeparamref name="TReturnType"/> that have the attribute <typeparamref name="TAttribute"/>.
		/// </summary>
		/// <typeparam name="TReturnType">The property/field type.  Will also match items that are assignable from this type.</typeparam>
		/// <typeparam name="TAttribute">The attribute that should be on the property/field.</typeparam>
		/// <param name="input">The type to search.</param>
		/// <param name="instance">The instance of <paramref name="input"/> that property/field values should be loaded from.</param>
		/// <param name="bindingFlags">The binding flags to identify the properties and fields to use.</param>
		/// <param name="includeBackingFields">If <see langword="true"/> then backing fields for properties will be returned.</param>
		public static IQueryable<Tuple<TReturnType, TAttribute[]>> GetPropertiesAndFieldsOfTypeWithAttribute<TReturnType, TAttribute>
		(
			this Type input,
			object instance,
			BindingFlags bindingFlags = BindingFlags.Instance
				| BindingFlags.FlattenHierarchy
				| BindingFlags.Public
				| BindingFlags.NonPublic,
			bool includeBackingFields = false
		)
			where TReturnType : class
			where TAttribute : Attribute
		{
			IQueryable<Tuple<TReturnType, TAttribute[]>> items;
			input.GetPropertiesAndFieldsOfTypeWithAttribute
			(
				instance, 
				out items,
				bindingFlags,
				includeBackingFields
			);
			return items;
		}

		/// <summary>
		/// Locates the values of properties and fields on <paramref name="input"/>
		/// of type <typeparamref name="TReturnType"/> that have the attribute <typeparamref name="TAttribute"/>.
		/// </summary>
		/// <typeparam name="TReturnType">The property/field type.  Will also match items that are assignable from this type.</typeparam>
		/// <typeparam name="TAttribute">The attribute that should be on the property/field.</typeparam>
		/// <param name="input">The type to search.</param>
		/// <param name="instance">The instance of <paramref name="input"/> that property/field values should be loaded from.</param>
		/// <param name="items">The value of the properties and the attribute.</param>
		/// <param name="bindingFlags">The binding flags to identify the properties and fields to use.</param>
		/// <param name="includeBackingFields">If <see langword="true"/> then backing fields for properties will be returned.</param>
		public static void GetPropertiesAndFieldsOfTypeWithAttribute<TReturnType, TAttribute>
		(
			this Type input,
			object instance,
			out IQueryable<Tuple<TReturnType, TAttribute[]>> items,
			BindingFlags bindingFlags = BindingFlags.Instance
				| BindingFlags.FlattenHierarchy
				| BindingFlags.Public
				| BindingFlags.NonPublic,
			bool includeBackingFields = false
		)
			where TReturnType : class
			where TAttribute : Attribute
		{
			// Use GetPropertiesAndFieldsOfType to get the properties and fields.
			input.GetPropertiesAndFieldsOfType<TReturnType>
			(
				out IQueryable<PropertyInfo> p,
				out IQueryable<FieldInfo> f,
				bindingFlags, 
				includeBackingFields
			);

			// Create our list.
			var list = new List<Tuple<TReturnType, TAttribute[]>>();

			// Filter properties by the attribute.
			list.AddRange
			(
				p
					.Where(mi => mi.CanRead)
					.Select(mi => new Tuple<TReturnType, TAttribute[]>
					(
						mi.GetValue(instance) as TReturnType,
						mi.GetCustomAttributes<TAttribute>().ToArray() ?? new TAttribute[0]
					))
					.Where(t => t.Item2.Length > 0)
			);

			// Filter fields by the attribute.
			list.AddRange
			(
				f
					.Select(mi => new Tuple<TReturnType, TAttribute[]>
					(
						mi.GetValue(instance) as TReturnType,
						mi.GetCustomAttributes<TAttribute>().ToArray() ?? new TAttribute[0]
					))
					.Where(t => t.Item2.Length > 0)
			);

			// Return our list.
			items = list.AsQueryable();
		}

		/// <summary>
		/// Returns information about <paramref name="properties"/> and <paramref name="fields"/>
		/// on objects of type <paramref name="input"/> that are declared of type <typeparamref name="TReturnType"/>.
		/// </summary>
		/// <typeparam name="TReturnType">The property/field type.  Will also match items that are assignable from this type.</typeparam>
		/// <param name="input">The type to search.</param>
		/// <param name="properties">The properties that match the query parameters.</param>
		/// <param name="fields">The fields that match the query parameters.</param>
		/// <param name="bindingFlags">The binding flags to identify the properties and fields to use.</param>
		/// <param name="includeBackingFields">If <see langword="true"/> then backing fields for properties will be returned.</param>
		public static void GetPropertiesAndFieldsOfType<TReturnType>
		(
			this Type input,
			out IQueryable<PropertyInfo> properties,
			out IQueryable<FieldInfo> fields,
			BindingFlags bindingFlags = BindingFlags.Instance
				| BindingFlags.FlattenHierarchy
				| BindingFlags.Public
				| BindingFlags.NonPublic,
			bool includeBackingFields = false
		)
		{
			// Sanity.
			if (input == null)
				throw new ArgumentNullException(nameof(input));

			// What is the type of property/field we care about?
			var type = typeof(TReturnType);

			// Get the properties;
			properties = input
				.GetProperties(bindingFlags)
				.Where(p => type.IsAssignableFrom(p.PropertyType))
				.AsQueryable();

			// Get the fields;
			fields = input
				.GetFields(bindingFlags)
				.Where(f => type.IsAssignableFrom(f.FieldType))
				.Where(f => includeBackingFields || !f.Name.EndsWith("_BackingField"))
				.AsQueryable();
		}
	}
}
