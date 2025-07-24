using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Enables searching for value list items, similar to
	/// how <see cref="MFiles.VAF.Common.MFSearchBuilder"/> does for objects.
	/// </summary>
	public class MFValueListItemSearchBuilder
	{
		/// <summary>
		/// The vault to use for searching.
		/// </summary>
		public Vault Vault { get; }

		/// <summary>
		/// The search conditions.
		/// </summary>
		public SearchConditions SearchConditions { get; }
			= new SearchConditions();

		/// <summary>
		/// The value list ID to search.
		/// </summary>
		public int ValueListId { get; }

		/// <summary>
		/// Instantiates the search builder.
		/// </summary>
		/// <param name="vault">The vault reference to use for searching.</param>
		/// <param name="valueListId">The value list to search.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="vault"/> is null.</exception>
		public MFValueListItemSearchBuilder(Vault vault, int valueListId)
		{
			this.Vault = vault 
				?? throw new ArgumentNullException(nameof(vault));
			this.ValueListId = valueListId;
		}

		/// <summary>
		/// Adds a "name" search condition.
		/// </summary>
		/// <param name="name">The name of the item to search for.</param>
		/// <param name="conditionType">The type of comparison to do.</param>
		/// <returns>The current <see cref="MFValueListItemSearchBuilder"/>.</returns>
		public MFValueListItemSearchBuilder Name(string name, MFConditionType conditionType = MFConditionType.MFConditionTypeEqual)
		{
			// Create the condition.
			var condition = new SearchCondition();

			// Set the expression.
			condition.Expression.SetValueListItemExpression(MFValueListItemPropertyDef.MFValueListItemPropertyDefName,
				MFParentChildBehavior.MFParentChildBehaviorNone);

			// Set the condition type.
			condition.ConditionType = conditionType;

			// Set the value.
			condition.TypedValue.SetValue(MFDataType.MFDatatypeText, name);

			// Add the condition.
			this.SearchConditions.Add(0, condition);

			// Return itself for chaining.
			return this;
		}

		/// <summary>
		/// Adds a "deleted" search condition.
		/// </summary>
		/// <param name="deletedItems">If <see langword="true"/> then includes only deleted items, if <see langword="false"/> then includes only non-deleted items.</param>
		/// <param name="conditionType">The type of comparison to do.</param>
		/// <returns>The current <see cref="MFValueListItemSearchBuilder"/>.</returns>
		public MFValueListItemSearchBuilder Deleted(bool deletedItems, MFConditionType conditionType = MFConditionType.MFConditionTypeEqual)
		{
			// Create the condition.
			var condition = new SearchCondition();

			// Set the expression.
			condition.Expression.SetValueListItemExpression(MFValueListItemPropertyDef.MFValueListItemPropertyDefDeleted,
				MFParentChildBehavior.MFParentChildBehaviorNone);

			// Set the condition type.
			condition.ConditionType = conditionType;

			// Set the value.
			condition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, deletedItems);

			// Add the condition.
			this.SearchConditions.Add(0, condition);

			// Return itself for chaining.
			return this;
		}

		/// <summary>
		/// Adds an "owner" search condition.
		/// </summary>
		/// <param name="ownerId">The id of the owner value list item.</param>
		/// <param name="conditionType">The type of comparison to do.</param>
		/// <returns>The current <see cref="MFValueListItemSearchBuilder"/>.</returns>
		public MFValueListItemSearchBuilder Owner(int ownerId, MFConditionType conditionType = MFConditionType.MFConditionTypeEqual)
		{
			// Create the condition.
			var condition = new SearchCondition();

			// Set the expression.
			condition.Expression.SetValueListItemExpression(MFValueListItemPropertyDef.MFValueListItemPropertyDefOwner,
				MFParentChildBehavior.MFParentChildBehaviorNone);

			// Set the condition type.
			condition.ConditionType = conditionType;

			// Set the value.
			condition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, ownerId);

			// Add the condition.
			this.SearchConditions.Add(0, condition);

			// Return itself for chaining.
			return this;
		}

		/// <summary>
		/// Adds a condition that filters to only items that match a given external ID,
		/// </summary>
		/// <param name="externalId">The external ID.</param>
		/// <param name="conditionType">The type of comparison to do.</param>
		/// <returns>The current <see cref="MFValueListItemSearchBuilder"/>.</returns>
		public MFValueListItemSearchBuilder ExternalId(string externalId, MFConditionType conditionType = MFConditionType.MFConditionTypeEqual)
		{
			// Create the condition.
			var condition = new SearchCondition();

			// Set the expression.
			condition.Expression.SetValueListItemExpression(MFValueListItemPropertyDef.MFValueListItemPropertyDefExtID,
				MFParentChildBehavior.MFParentChildBehaviorNone);

			// Set the condition type.
			condition.ConditionType = conditionType;

			// Set the value.
			condition.TypedValue.SetValue(MFDataType.MFDatatypeText, externalId);

			// Add the condition.
			this.SearchConditions.Add(0, condition);

			// Return itself for chaining.
			return this;
		}

		/// <summary>
		/// Executes the search.
		/// </summary>
		/// <param name="updateFromServer">If this is set to "true", the search operation is always carried out on the server-side.</param>
		/// <param name="refreshType">Specifies how, if at all, external value lists are updated for this operation.</param>
		/// <param name="replaceCurrentUserWithCallersIdentity"></param>
		/// <param name="propertyDefId">Specifies the property definition the filter of which should be applied to the list (-1 for none).</param>
		/// <param name="maximumResults">The maximum number of results to return. By default, no more than 5,000 results are allowed.</param>
		/// <returns>Any matching value list items.</returns>
		/// <remarks>Uses <see cref="VaultValueListItemOperationsClass.SearchForValueListItemsEx2"/> to search.</remarks>
		public ValueListItemSearchResults Find
		(
			bool updateFromServer = false,
			MFExternalDBRefreshType refreshType = MFExternalDBRefreshType.MFExternalDBRefreshTypeNone,
			bool replaceCurrentUserWithCallersIdentity = false,
			int propertyDefId = -1,
			int maximumResults = 5000
		)
		{
			// Delegate.
			return this.Vault.ValueListItemOperations.SearchForValueListItemsEx2
			(
				this.ValueListId,
				this.SearchConditions,
				updateFromServer,
				refreshType,
				replaceCurrentUserWithCallersIdentity,
				propertyDefId,
				maximumResults
			);
		}

	}
}
