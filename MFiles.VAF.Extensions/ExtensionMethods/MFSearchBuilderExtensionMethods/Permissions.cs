using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MFiles.VAF.Common;
using MFilesAPI;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Extension methods for the <see cref="MFSearchBuilder"/> class.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static partial class MFSearchBuilderExtensionMethods
	{

		#region User has permission to (generic)

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for whether the specified user has the specified permission on an object.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="userId">The ID of the user in question.</param>
		/// <param name="permission">The permission that the user should have on the object.</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder UserHasPermissionTo
		(
			this MFSearchBuilder searchBuilder,
			int userId,
			MFPermissionsExpressionType permission,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual
		)
		{
			// Sanity.
			if (null == searchBuilder)
				throw new ArgumentNullException(nameof(searchBuilder));

			// Create the search condition.
			var searchCondition = new SearchCondition
			{
				ConditionType = conditionType
			};

			// Set up the permission expression.
			searchCondition.Expression.SetPermissionExpression
			(
				permission
			);

			// Set the condition value to the lookup.
			var lookup = new MFilesAPI.Lookup()
			{
				ObjectType = (int)MFBuiltInValueList.MFBuiltInValueListUsers,
				Item = userId
			};
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, lookup);

			// Add the search condition to the collection.
			searchBuilder.Conditions.Add(-1, searchCondition);

			// Return the search builder for chaining.
			return searchBuilder;
		}

		#endregion

		#region Visible to

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for whether the specified user can see an object.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="userId">The ID of the user that should be able to see the object.</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder VisibleTo
		(
			this MFSearchBuilder searchBuilder,
			int userId,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual
		)
		{
			// Use UserHasPermissionTo method.
			return searchBuilder.UserHasPermissionTo
			(
				userId,
				MFPermissionsExpressionType.MFVisibleTo,
				conditionType
			);
		}

		#endregion

		#region Editable by

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for whether the specified user can edit an object.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="userId">The ID of the user that should be able to edit the object.</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder EditableBy
		(
			this MFSearchBuilder searchBuilder,
			int userId,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual
		)
		{
			// Use UserHasPermissionTo method.
			return searchBuilder.UserHasPermissionTo
			(
				userId,
				MFPermissionsExpressionType.MFEditableBy,
				conditionType
			);
		}

		#endregion

		#region Deletable by

		/// <summary>
		/// Adds a <see cref="SearchCondition"/> to the collection for whether the specified user can delete an object.
		/// </summary>
		/// <param name="searchBuilder">The <see cref="MFSearchBuilder"/> to add the condition to.</param>
		/// <param name="userId">The ID of the user that should be able to delete the object.</param>
		/// <param name="conditionType">What type of search to execute (defaults to <see cref="MFConditionType.MFConditionTypeEqual"/>).</param>
		/// <returns>The <paramref name="searchBuilder"/> provided, for chaining.</returns>
		public static MFSearchBuilder DeletableBy
		(
			this MFSearchBuilder searchBuilder,
			int userId,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual
		)
		{
			// Use UserHasPermissionTo method.
			return searchBuilder.UserHasPermissionTo
			(
				userId,
				MFPermissionsExpressionType.MFDeletableBy,
				conditionType
			);
		}

		#endregion
	}
}
