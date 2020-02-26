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
		protected MFDataType[] HandledDataTypes { get; set; }

		protected PropertyValueSearchConditionTestBase(MFDataType[] handledDataTypes)
		{
			this.HandledDataTypes = handledDataTypes ?? new MFDataType[0];

			// Sanity.
			if (this.HandledDataTypes.Length == 0)
				throw new ArgumentException("Handled data types cannot be empty", nameof(handledDataTypes));
		}
		
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
		protected abstract void AddSearchCondition
			(
			MFSearchBuilder mfSearchBuilder,
			int propertyDef,
			TInputType value,
			MFConditionType conditionType = MFConditionType.MFConditionTypeEqual,
			MFParentChildBehavior parentChildBehavior = MFParentChildBehavior.MFParentChildBehaviorNone,
			DataFunctionCall dataFunctionCall = null
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
		/// <see cref="AddSearchCondition(MFSearchBuilder, int, T, MFConditionType, MFParentChildBehavior, DataFunctionCall)"/>
		/// adds a search condition.
		/// </summary>
		[TestMethod]
		public void AddsSearchCondition()
		{
			// Get the test properties.
			var properties = this.GetTestProperties();

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

		public void AssertSearchConditionIsCorrect
			(
			int propertyDef, 
			MFDataType expectedDataType,
			TInputType input,
			MFConditionType conditionType,
			MFParentChildBehavior parentChildBehavior
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
				parentChildBehavior
			);

			// If there's anything other than one condition then fail.
			if (mfSearchBuilder.Conditions.Count != 1)
				Assert.Inconclusive("Only one search condition should exist");

			// Retrieve the just-added condition.
			var condition = mfSearchBuilder.Conditions[mfSearchBuilder.Conditions.Count];

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
				Assert.Fail(
					$"Adding a search condition for an incorrect type {dataType} should have excepted but it did not.");
			}
			catch (ArgumentException)
			{
				// This is fine - exception is expected.
			}
		}
	}
}