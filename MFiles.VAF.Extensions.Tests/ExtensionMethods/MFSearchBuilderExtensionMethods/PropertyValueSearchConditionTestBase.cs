using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.MFSearchBuilderExtensionMethods
{
	public abstract class PropertyValueSearchConditionTestBase
		: MFSearchBuilderExtensionMethodTestBase
	{
#pragma warning disable CA1819 // Properties should not return arrays
		protected MFDataType[] HandledDataTypes { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

		protected PropertyValueSearchConditionTestBase(MFDataType[] handledDataTypes)
		{
			this.HandledDataTypes = handledDataTypes ?? new MFDataType[0];

			// Sanity.
			if (this.HandledDataTypes.Length == 0)
				throw new ArgumentException("Handled data types cannot be empty", nameof(handledDataTypes));
		}
		
		// ReSharper disable InconsistentNaming
		public const int TestTextPropertyId = 1200;
		public const int TestMultiLineTextPropertyId = 1201;
		public const int TestLookupPropertyId = 1202;
		public const int TestMultiSelectLookupPropertyId = 1203;
		public const int TestDatePropertyId = 1204;
		public const int TestTimePropertyId = 1205;
		public const int TestTimestampPropertyId = 1206;
		public const int TestIntegerPropertyId = 1207;
		public const int TestInteger64PropertyId = 1208;
		public const int TestFloatPropertyId = 1209;
		public const int TestBooleanPropertyId = 1210;
		// ReSharper restore InconsistentNaming

		protected virtual IEnumerable<PropertyDef> GetTestProperties()
		{
#pragma warning disable IDE0028 // Simplify collection initialization
			var  properties = new List<PropertyDef>();
#pragma warning restore IDE0028 // Simplify collection initialization

			properties.Add(new PropertyDef()
			{
				ID = PropertyValueSearchConditionTestBase.TestTextPropertyId,
				Name = "test text property",
				DataType = MFDataType.MFDatatypeText
			});

			properties.Add(new PropertyDef()
			{
				ID = PropertyValueSearchConditionTestBase.TestMultiLineTextPropertyId,
				Name = "test text (multi-line) property",
				DataType = MFDataType.MFDatatypeMultiLineText
			});

			properties.Add(new PropertyDef()
			{
				ID = PropertyValueSearchConditionTestBase.TestLookupPropertyId,
				Name = "test lookup property",
				DataType = MFDataType.MFDatatypeLookup
			});

			properties.Add(new PropertyDef()
			{
				ID = PropertyValueSearchConditionTestBase.TestMultiSelectLookupPropertyId,
				Name = "test lookup (multi-select) property",
				DataType = MFDataType.MFDatatypeMultiSelectLookup
			});

			properties.Add(new PropertyDef()
			{
				ID = PropertyValueSearchConditionTestBase.TestDatePropertyId,
				Name = "test date property",
				DataType = MFDataType.MFDatatypeDate
			});

			properties.Add(new PropertyDef()
			{
				ID = PropertyValueSearchConditionTestBase.TestTimePropertyId,
				Name = "test time property",
				DataType = MFDataType.MFDatatypeTime
			});

			properties.Add(new PropertyDef()
			{
				ID = PropertyValueSearchConditionTestBase.TestTimestampPropertyId,
				Name = "test timestamp property",
				DataType = MFDataType.MFDatatypeTimestamp
			});

			properties.Add(new PropertyDef()
			{
				ID = PropertyValueSearchConditionTestBase.TestIntegerPropertyId,
				Name = "test integer property",
				DataType = MFDataType.MFDatatypeInteger
			});

			properties.Add(new PropertyDef()
			{
				ID = PropertyValueSearchConditionTestBase.TestInteger64PropertyId,
				Name = "test long property",
				DataType = MFDataType.MFDatatypeInteger64
			});

			properties.Add(new PropertyDef()
			{
				ID = PropertyValueSearchConditionTestBase.TestFloatPropertyId,
				Name = "test float property",
				DataType = MFDataType.MFDatatypeFloating
			});

			properties.Add(new PropertyDef()
			{
				ID = PropertyValueSearchConditionTestBase.TestBooleanPropertyId,
				Name = "test boolean property",
				DataType = MFDataType.MFDatatypeBoolean
			});

			return properties;
		}

		protected override Mock<Vault> GetVaultMock()
		{
			// Get the standard vault mock.
			var vaultMock = base.GetVaultMock();

			// Set up the property def operations mock.
			var propertyDefOperationsMock = new Mock<VaultPropertyDefOperations>();

			// Set up the GetPropertyDef method.
			propertyDefOperationsMock
				.Setup(m => m.GetPropertyDef(It.IsAny<int>()))
				.Returns((int propertyDef) =>
				{
					// Make sure we have something.
					var property = this
						.GetTestProperties()
						.FirstOrDefault(p => p.ID == propertyDef);
					if (null == property)
					{
						throw new InvalidOperationException($"Unknown property definition {propertyDef}.");
					}

					return property;
				});

			// Make the vault return the property def operations mock as needed.
			vaultMock
				.SetupGet(m => m.PropertyDefOperations)
				.Returns(propertyDefOperationsMock.Object);

			// Return the vault mock.
			return vaultMock;
		}
	}

	/// <summary>
	/// A base class used for testing <see cref="MFSearchBuilderExtensionMethods"/>
	/// extension methods that refer to property value search conditions.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public abstract class PropertyValueSearchConditionTestBase<TInputType>
		: PropertyValueSearchConditionTestBase
	{

		/// <inheritdoc />
		protected PropertyValueSearchConditionTestBase(MFDataType[] handledDataTypes)
			: base(handledDataTypes)
		{
		}

		/// <summary>
		/// Adds a search condition to the search builder of the type specified.
		/// </summary>
		/// <param name="mfSearchBuilder">The search builder to add to.</param>
		/// <param name="propertyDef">The Id of the property definition to add.</param>
		/// <param name="value">The value to add.</param>
		/// <param name="conditionType">The condition type for the property search condition.</param>
		/// <param name="parentChildBehavior">How to handle parent/child matches.</param>
		/// <param name="dataFunctionCall">The data function call, if any.</param>
		/// <param name="indirectionLevels">The indirection levels (from the search object) to access the property to match.</param>
		protected abstract void AddSearchCondition
			(
			MFSearchBuilder mfSearchBuilder,
			int propertyDef,
			TInputType value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			DataFunctionCall dataFunctionCall = null,
			PropertyDefOrObjectTypes indirectionLevels = null
			);

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullSearchBuilderThrows()
		{
			// Add the search condition.
			this.AddSearchCondition
			(
				null,
				123,
				default
			);
		}

		/// <summary>
		/// Tests that calling 
		/// <see cref="AddSearchCondition(MFSearchBuilder, int, TInputType, MFConditionType, MFParentChildBehavior, DataFunctionCall, PropertyDefOrObjectTypes)"/>
		/// adds a search condition.
		/// </summary>
		[TestMethod]
		public void AddsSearchCondition()
		{
			// Get the test properties.
			var properties = this
				.GetTestProperties()
				.ToList();

			// Check each handled data type works as expected.
			foreach (var dataType in this.HandledDataTypes)
			{
				// Find a property of this data type.
				var property = properties
					.FirstOrDefault(propertyDef => propertyDef.DataType == dataType);
				if (null == property)
					throw new InvalidOperationException($"Property could not be found with data type {dataType}");

				// Create the search builder.
				var mfSearchBuilder = this.GetSearchBuilder();

				// Ensure it has no items in the collection.
				Assert.AreEqual(0, mfSearchBuilder.Conditions.Count);

				// Add the search condition.
				this.AddSearchCondition
				(
					mfSearchBuilder,
					property.ID,
					default
				);

				// Ensure that there is one item in the collection.
				Assert.AreEqual(1, mfSearchBuilder.Conditions.Count);
			}
		}

		/// <summary>
		/// Ensures that the search condition added matches the expected data.
		/// </summary>
		/// <param name="condition">The already-populated search condition.</param>
		/// <param name="propertyDef">The property definition Id expected.</param>
		/// <param name="expectedDataType">The expected data type.</param>
		/// <param name="input">The expected value.</param>
		/// <param name="conditionType">The condition (equal, etc.) expected.</param>
		/// <param name="parentChildBehavior">The expected parent/child behaviour.</param>
		/// <param name="indirectionLevels">The expected indirection levels.</param>
		public void AssertSearchConditionIsCorrect
			(
			SearchCondition condition,
			int propertyDef, 
			MFDataType expectedDataType,
			TInputType input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels
			)
		{
			// Ensure the condition is not null.
			Assert.IsNotNull(condition);

			// Ensure the condition type is correct.
			Assert.AreEqual(conditionType, condition.ConditionType);

			// Ensure the expression type is correct.
			Assert.AreEqual(MFExpressionType.MFExpressionTypePropertyValue, condition.Expression.Type);

			// Ensure the property value data is correct.
			Assert.AreEqual(propertyDef, condition.Expression.DataPropertyValuePropertyDef);
			Assert.AreEqual
			(
				parentChildBehavior,
				condition.Expression.DataPropertyValueParentChildBehavior
			);
			Assert.AreEqual
			(
				MFDataFunction.MFDataFunctionNoOp,
				condition.Expression.DataPropertyValueDataFunction
			);

			// Ensure that the typed value is correct.
			Assert.AreEqual(expectedDataType, condition.TypedValue.DataType);
			if (input == null)
			{
				Assert.IsTrue(condition.TypedValue.IsNULL());
			}
			else
			{
				Assert.AreEqual(input, (TInputType)condition.TypedValue.Value);
			}

			// Ensure that the indirection levels are the same.
			this.AssertIndirectionLevelsAreSame(indirectionLevels, condition.Expression.IndirectionLevels);
		}

		/// <summary>
		/// Ensures that the search condition added matches the expected data.
		/// </summary>
		/// <param name="propertyDef">The property definition Id expected.</param>
		/// <param name="expectedDataType">The expected data type.</param>
		/// <param name="input">The expected value.</param>
		/// <param name="conditionType">The condition (equal, etc.) expected.</param>
		/// <param name="parentChildBehavior">The expected parent/child behaviour.</param>
		/// <param name="indirectionLevels">The expected indirection levels.</param>
		public void AssertSearchConditionIsCorrect
			(
			int propertyDef, 
			MFDataType expectedDataType,
			TInputType input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior,
			PropertyDefOrObjectTypes indirectionLevels
			)
		{
			// Create the search builder.
			var mfSearchBuilder = this.GetSearchBuilder();

			// Add the search condition for the property.
			this.AddSearchCondition
			(
				mfSearchBuilder,
				propertyDef,
				input,
				conditionType,
				parentChildBehavior,
				indirectionLevels: indirectionLevels
			);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

			// Validate it.
			this.AssertSearchConditionIsCorrect
			(
				condition,
				propertyDef,
				expectedDataType,
				input,
				conditionType,
				parentChildBehavior,
				indirectionLevels
			);
		}

		/// <summary>
		/// Asserts that the supplied indirection levels match.
		/// </summary>
		/// <param name="expected">The expected indirection levels.  If null will be converted to an empty collection.</param>
		/// <param name="actual">The actual indirection levels on the search condition.</param>
		public void AssertIndirectionLevelsAreSame
		(
			PropertyDefOrObjectTypes expected,
			PropertyDefOrObjectTypes actual
		)
		{
			// If the expected is null then it should be empty.
			expected = expected ?? new PropertyDefOrObjectTypes();
			Assert.IsNotNull(actual);

			// Ensure that the indirection levels are the same.
			Assert.AreEqual(expected.Count, actual.Count);
			for (var i = 1; i <= expected.Count; i++)
			{
				var expectedLevel = expected[i];
				var actualLevel = actual[i];

				// Should never be null, but check they're the same.
				if (expectedLevel == null)
				{
					Assert.IsNull(actualLevel);
					continue;
				}

				// Make sure they are the same.
				Assert.IsNotNull(actualLevel);
				Assert.AreEqual(expectedLevel.ID, actualLevel.ID);
				Assert.AreEqual(expectedLevel.PropertyDef, actualLevel.PropertyDef);
			}
		}

		[TestMethod]
		[DataRow(PropertyValueSearchConditionTestBase.TestTextPropertyId, MFDataType.MFDatatypeText)]
		[DataRow(PropertyValueSearchConditionTestBase.TestMultiLineTextPropertyId, MFDataType.MFDatatypeMultiLineText)]
		[DataRow(PropertyValueSearchConditionTestBase.TestLookupPropertyId, MFDataType.MFDatatypeLookup)]
		[DataRow(PropertyValueSearchConditionTestBase.TestMultiSelectLookupPropertyId, MFDataType.MFDatatypeMultiSelectLookup)]
		[DataRow(PropertyValueSearchConditionTestBase.TestDatePropertyId, MFDataType.MFDatatypeDate)]
		[DataRow(PropertyValueSearchConditionTestBase.TestTimePropertyId, MFDataType.MFDatatypeTime)]
		[DataRow(PropertyValueSearchConditionTestBase.TestTimestampPropertyId, MFDataType.MFDatatypeTimestamp)]
		[DataRow(PropertyValueSearchConditionTestBase.TestIntegerPropertyId, MFDataType.MFDatatypeInteger)]
		[DataRow(PropertyValueSearchConditionTestBase.TestInteger64PropertyId, MFDataType.MFDatatypeInteger64)]
		[DataRow(PropertyValueSearchConditionTestBase.TestFloatPropertyId, MFDataType.MFDatatypeFloating)]
		[DataRow(PropertyValueSearchConditionTestBase.TestBooleanPropertyId, MFDataType.MFDatatypeBoolean)]
		public void PropertyDefinitionsWithIncorrectDataTypeThrow(
			int propertyDef,
			MFDataType dataType
		)
		{
			// If this should be handled then die.
			if (this.HandledDataTypes.Contains(dataType))
				return;


			try
			{
				// Create the search builder.
				var mfSearchBuilder = this.GetSearchBuilder();

				// Add the search condition.
				this.AddSearchCondition(mfSearchBuilder, propertyDef, default);

				// If we got this far then the above did not throw.
				Assert.Fail
				(
					$"Adding a search condition for an incorrect type {dataType} should have excepted but it did not."
				);
			}
			catch (ArgumentException)
			{
				// This is fine - exception is expected.
			}
		}
	}
}