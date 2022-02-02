using System;
using System.Linq;
using MFiles.VAF.Common;
using MFilesAPI;

namespace MFiles.VAF.Extensions
{
	public static partial class ObjVerExExtensionMethods
	{

		#region GetPropertyAs utility method

		/// <summary>
		/// Returns the value of the property specified in <paramref name="propertyDef"/> as an instance of <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type of the property value.</typeparam>
		/// <param name="objVerEx">The object to retrieve data from.</param>
		/// <param name="propertyDef">The property definition Id.</param>
		/// <param name="validDataTypes">The types of property definition expected.  Leave null/empty to skip type checking.</param>
		/// <returns>The value of the property, or default(<typeparamref name="T"/>) if not set.</returns>
		public static T GetPropertyAs<T>
		(
			this ObjVerEx objVerEx, 
			int propertyDef,
			params MFDataType[] validDataTypes
		)
		{
			// Sanity
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if (0 > propertyDef)
				throw new ArgumentOutOfRangeException(nameof(propertyDef), Resources.Exceptions.VaultInteraction.PropertyDefinition_NotResolved);

			// Does this property exist in the collection?
			var propertyValue = objVerEx.Properties.SearchForPropertyEx(propertyDef, true);
			if(null == propertyValue || propertyValue.TypedValue.IsNULL() || propertyValue.TypedValue.IsUninitialized())
				return default(T);

			// Should we validate the type?
			if (null != validDataTypes && validDataTypes.Length > 0)
			{
				// What is the type of this property?
				var dataType = objVerEx.Vault.PropertyDefOperations.GetPropertyDef(propertyDef).DataType;

				// Is it in the collection of valid types.
				if (false == validDataTypes.Contains(dataType))
				{
					throw new ArgumentException
					(
						String.Format
						(
							Resources.Exceptions.ObjVerExExtensionMethods.GetPropertyAs_PropertyNotOfExpectedType,
							propertyDef,
							dataType,
							string.Join(", ", validDataTypes)
						),
						nameof(propertyDef)
					);
				}
			}

			// Attempt to return the data as correctly typed value.
			return (T)propertyValue.TypedValue.Value;
		}

		#endregion

		/// <summary>
		/// Returns the <see cref="DateTime"/> value of the property specified in <paramref name="propertyDef"/>.
		/// </summary>
		/// <param name="objVerEx">The object to retrieve data from.</param>
		/// <param name="propertyDef">The property definition Id.</param>
		/// <returns>The value of the property, or null if not set.</returns>
		public static DateTime? GetPropertyAsDateTime
		(
			this ObjVerEx objVerEx, 
			int propertyDef
		)
		{
			return objVerEx.GetPropertyAs<DateTime?>
			(
				propertyDef,
				MFDataType.MFDatatypeDate,
				MFDataType.MFDatatypeTimestamp
			);
		}

		/// <summary>
		/// Returns the <see cref="bool"/> value of the property specified in <paramref name="propertyDef"/>.
		/// </summary>
		/// <param name="objVerEx">The object version to check.</param>
		/// <param name="propertyDef">The property definition Id.</param>
		/// <returns>The value of the property, or null if not set.</returns>
		public static bool? GetPropertyAsBoolean
		(
			this ObjVerEx objVerEx,
			int propertyDef
		)
		{
			return objVerEx.GetPropertyAs<bool?>
			(
				propertyDef,
				MFDataType.MFDatatypeBoolean
			);
		}

		/// <summary>
		/// Returns the <see cref="int"/> value of the property specified in <paramref name="propertyDef"/>.
		/// </summary>
		/// <param name="objVerEx">The object version to check.</param>
		/// <param name="propertyDef">The property definition Id.</param>
		/// <returns>The value of the property, or null if not set.</returns>
		public static int? GetPropertyAsInteger
		(
			this ObjVerEx objVerEx,
			int propertyDef
		)
		{
			return objVerEx.GetPropertyAs<int?>
			(
				propertyDef,
				MFDataType.MFDatatypeInteger
			);
		}

		/// <summary>
		/// Returns the <see cref="int"/> value of the property specified in <paramref name="propertyDef"/>.
		/// </summary>
		/// <param name="objVerEx">The object version to check.</param>
		/// <param name="propertyDef">The property definition Id.</param>
		/// <returns>The value of the property, or null if not set.</returns>
		public static long? GetPropertyAsLong
		(
			this ObjVerEx objVerEx, 
			int propertyDef
		)
		{
			return objVerEx.GetPropertyAs<long?>
			(
				propertyDef,
				MFDataType.MFDatatypeInteger64
			);
		}

		/// <summary>
		/// Returns the <see cref="double"/> value of the property specified in <paramref name="propertyDef"/>.
		/// </summary>
		/// <param name="objVerEx">The object version to check.</param>
		/// <param name="propertyDef">The property definition Id.</param>
		/// <returns>The value of the property, or null if not set.</returns>
		public static double? GetPropertyAsDouble
		(
			this ObjVerEx objVerEx, 
			int propertyDef
		)
		{
			return objVerEx.GetPropertyAs<double?>
			(
				propertyDef,
				MFDataType.MFDatatypeFloating
			);
		}
	}
}
