using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MFiles.VAF.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx.PropertyValueInstruction
{
	[TestClass]
	public class ApplyTo
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsWithNullPropertyValues()
		{
			new ObjectCopyOptions.PropertyValueInstruction().ApplyTo(null);
		}

		#region AddValueToProperty

		public static IEnumerable<object[]> GetUnsupportedDataTypesAndValues()
		{
			yield return new object[] { MFDataType.MFDatatypeLookup, 1, 2 };
			yield return new object[] { MFDataType.MFDatatypeDate, new DateTime(2000, 1, 1), DateTime.Now };
			yield return new object[] { MFDataType.MFDatatypeInteger, 1, 2 };
			yield return new object[] { MFDataType.MFDatatypeInteger64, 1, 2 };

			// Text properties could be supported, I guess (concatenate).
			yield return new object[] { MFDataType.MFDatatypeText, "hello", "world" };
			yield return new object[] { MFDataType.MFDatatypeMultiLineText, "hello", "world" };
		}

		// Should not work for unsupported data types.
		[TestMethod]
		[DynamicData(nameof(ApplyTo.GetUnsupportedDataTypesAndValues), DynamicDataSourceType.Method)]
		[ExpectedException(typeof(NotImplementedException))]
		public void AddValueToProperty_ThrowsForUnsupportedDataType
		(
			MFDataType dataType,
			object initialValue,
			object newValue
		)
		{
			const int propertyDefId = 12345;

			// Set up the instruction to add/update.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.AddValueToProperty,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = propertyDefId
				}
			};
			instruction.PropertyValue.TypedValue.SetValue
			(
				dataType,
				newValue // Add the new value.
			);

			// Apply it.
			var propertyValues = new PropertyValues();
			{
				var pv = new PropertyValue()
				{
					PropertyDef = propertyDefId
				};
				// Set the starting value the initial value.
				pv.TypedValue.SetValue
				(
					dataType,
					initialValue
				);
				propertyValues.Add(-1, pv);
			}
			instruction.ApplyTo(propertyValues);
		}

		// Does ApplyTo add the property if it is not there at all?
		[TestMethod]
		public void AddValueToProperty_Adds()
		{
			// Set up the instruction to add/update.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.AddValueToProperty,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass
				}
			};
			instruction.PropertyValue.TypedValue.SetValue
			(
				MFDataType.MFDatatypeLookup,
				(int)MFBuiltInDocumentClass.MFBuiltInDocumentClassOtherDocument
			);

			// Apply it.
			var propertyValues = new PropertyValues();
			instruction.ApplyTo(propertyValues);

			// Test the results.
			Assert.AreEqual(1, propertyValues.Count);
			Assert.AreEqual((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass, propertyValues[1].PropertyDef);
			Assert.AreEqual
			(
				(int)MFBuiltInDocumentClass.MFBuiltInDocumentClassOtherDocument,
				propertyValues[1].TypedValue.GetLookupID()
			);
		}

		// Does ApplyTo add a single value (int[]) to a MSLU property?
		[TestMethod]
		public void AddValueToProperty_AddsSingleValueAsArray_Null()
		{
			const int propertyDef = 1024;

			// Set up the instruction to add/update.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.AddValueToProperty,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = propertyDef
				}
			};
			instruction.PropertyValue.TypedValue.SetValue
			(
				MFDataType.MFDatatypeMultiSelectLookup,
				new int[] { 4 }
			);

			// Apply it.
			var propertyValues = new PropertyValues();
			{
				var pv = new PropertyValue()
				{
					PropertyDef = propertyDef
				};
				// Set the starting value to null (should end up with one item).
				pv.TypedValue.SetValueToNULL(MFDataType.MFDatatypeMultiSelectLookup);
				propertyValues.Add(-1, pv);
			}
			instruction.ApplyTo(propertyValues);

			// Test the results.
			Assert.AreEqual(1, propertyValues.Count);
			Assert.AreEqual(propertyDef, propertyValues[1].PropertyDef);
			Assert.AreEqual
			(
				1,
				propertyValues[1].TypedValue.GetValueAsLookups().Count
			);
			Assert.AreEqual
			(
				4, // This is the single ID that was added.
				propertyValues[1].TypedValue.GetValueAsLookups()[1].Item
			);
		}

		// Does ApplyTo add a single value (int) to a MSLU property?
		[TestMethod]
		public void AddValueToProperty_AddsSingleValueInt_Null()
		{
			const int propertyDef = 1024;

			// Set up the instruction to add/update.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.AddValueToProperty,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = propertyDef
				}
			};
			instruction.PropertyValue.TypedValue.SetValue
			(
				MFDataType.MFDatatypeMultiSelectLookup,
				4
			);

			// Apply it.
			var propertyValues = new PropertyValues();
			{
				var pv = new PropertyValue()
				{
					PropertyDef = propertyDef
				};
				// Set the starting value to null (should end up with one item).
				pv.TypedValue.SetValueToNULL(MFDataType.MFDatatypeMultiSelectLookup);
				propertyValues.Add(-1, pv);
			}
			instruction.ApplyTo(propertyValues);

			// Test the results.
			Assert.AreEqual(1, propertyValues.Count);
			Assert.AreEqual(propertyDef, propertyValues[1].PropertyDef);
			Assert.AreEqual
			(
				1,
				propertyValues[1].TypedValue.GetValueAsLookups().Count
			);
			Assert.AreEqual
			(
				4, // This is the single ID that was added.
				propertyValues[1].TypedValue.GetValueAsLookups()[1].Item
			);
		}

		// Does ApplyTo add multiple values to a MSLU property?
		[TestMethod]
		public void AddValueToProperty_AddsMultipleValues_Null()
		{
			const int propertyDef = 1024;

			// Set up the instruction to add/update.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.AddValueToProperty,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = propertyDef
				}
			};
			instruction.PropertyValue.TypedValue.SetValue
			(
				MFDataType.MFDatatypeMultiSelectLookup,
				new int[] { 4, 5, 6 }
			);

			// Apply it.
			var propertyValues = new PropertyValues();
			{
				var pv = new PropertyValue()
				{
					PropertyDef = propertyDef
				};
				// Set the starting value to null (should end up with one item).
				pv.TypedValue.SetValueToNULL(MFDataType.MFDatatypeMultiSelectLookup);
				propertyValues.Add(-1, pv);
			}
			instruction.ApplyTo(propertyValues);

			// Test the results.
			Assert.AreEqual(1, propertyValues.Count);
			Assert.AreEqual(propertyDef, propertyValues[1].PropertyDef);
			Assert.AreEqual
			(
				3,
				propertyValues[1].TypedValue.GetValueAsLookups().Count
			);

			// Check they are as expected.
			Assert.AreEqual
			(
				4,
				propertyValues[1].TypedValue.GetValueAsLookups()[1].Item
			);
			Assert.AreEqual
			(
				5,
				propertyValues[1].TypedValue.GetValueAsLookups()[2].Item
			);
			Assert.AreEqual
			(
				6,
				propertyValues[1].TypedValue.GetValueAsLookups()[3].Item
			);
		}

		// Does ApplyTo add a single value to a MSLU property?
		[TestMethod]
		public void AddValueToProperty_AddsSingleValue_SingleNewValue_NoOverlap()
		{
			const int propertyDef = 1024;

			// Set up the instruction to add/update.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.AddValueToProperty,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = propertyDef
				}
			};
			instruction.PropertyValue.TypedValue.SetValue
			(
				MFDataType.MFDatatypeMultiSelectLookup,
				new int[] { 6 }
			);

			// Apply it.
			var propertyValues = new PropertyValues();
			{
				var pv = new PropertyValue()
				{
					PropertyDef = propertyDef
				};
				// Set the starting value to a single item (should end up with two).
				pv.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, new int[] { 4 } );
				propertyValues.Add(-1, pv);
			}
			instruction.ApplyTo(propertyValues);

			// Test the results.
			Assert.AreEqual(1, propertyValues.Count);
			Assert.AreEqual(propertyDef, propertyValues[1].PropertyDef);
			Assert.AreEqual
			(
				2,
				propertyValues[1].TypedValue.GetValueAsLookups().Count
			);
			Assert.AreEqual
			(
				4, // This is the starting one.
				propertyValues[1].TypedValue.GetValueAsLookups()[1].Item
			);
			Assert.AreEqual
			(
				6, // This is the single ID that was added.
				propertyValues[1].TypedValue.GetValueAsLookups()[2].Item
			);
		}

		// Does ApplyTo add multiple values to a MSLU property?
		[TestMethod]
		public void AddValueToProperty_AddsMultipleValues_MultipleNewValues_NoOverlap()
		{
			const int propertyDef = 1024;

			// Set up the instruction to add/update.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.AddValueToProperty,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = propertyDef
				}
			};
			instruction.PropertyValue.TypedValue.SetValue
			(
				MFDataType.MFDatatypeMultiSelectLookup,
				new int[] { 4, 5, 6 }
			);

			// Apply it.
			var propertyValues = new PropertyValues();
			{
				var pv = new PropertyValue()
				{
					PropertyDef = propertyDef
				};
				// Set the starting value to two items (should end up with five).
				pv.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, new int[] { 123, 456 } );
				propertyValues.Add(-1, pv);
			}
			instruction.ApplyTo(propertyValues);

			// Test the results.
			Assert.AreEqual(1, propertyValues.Count);
			Assert.AreEqual(propertyDef, propertyValues[1].PropertyDef);
			Assert.AreEqual
			(
				5,
				propertyValues[1].TypedValue.GetValueAsLookups().Count
			);

			// Check they are as expected.
			Assert.AreEqual
			(
				123,
				propertyValues[1].TypedValue.GetValueAsLookups()[1].Item
			);
			Assert.AreEqual
			(
				456,
				propertyValues[1].TypedValue.GetValueAsLookups()[2].Item
			);
			Assert.AreEqual
			(
				4,
				propertyValues[1].TypedValue.GetValueAsLookups()[3].Item
			);
			Assert.AreEqual
			(
				5,
				propertyValues[1].TypedValue.GetValueAsLookups()[4].Item
			);
			Assert.AreEqual
			(
				6,
				propertyValues[1].TypedValue.GetValueAsLookups()[5].Item
			);
		}

		// Does ApplyTo add a single value to a MSLU property?
		[TestMethod]
		public void AddValueToProperty_AddsSingleValue_SingleNewValue_AlreadyExists()
		{
			const int propertyDef = 1024;

			// Set up the instruction to add/update.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.AddValueToProperty,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = propertyDef
				}
			};
			instruction.PropertyValue.TypedValue.SetValue
			(
				MFDataType.MFDatatypeMultiSelectLookup,
				new int[] { 4 }
			);

			// Apply it.
			var propertyValues = new PropertyValues();
			{
				var pv = new PropertyValue()
				{
					PropertyDef = propertyDef
				};
				// Set the starting value to a single item (should end up with two).
				pv.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, new int[] { 4 });
				propertyValues.Add(-1, pv);
			}
			instruction.ApplyTo(propertyValues);

			// Test the results.
			Assert.AreEqual(1, propertyValues.Count);
			Assert.AreEqual(propertyDef, propertyValues[1].PropertyDef);
			Assert.AreEqual
			(
				1,
				propertyValues[1].TypedValue.GetValueAsLookups().Count
			);
			Assert.AreEqual
			(
				4, // This is the starting one.
				propertyValues[1].TypedValue.GetValueAsLookups()[1].Item
			);
		}

		#endregion

		#region ReplaceOrAddPropertyValue

		[TestMethod]
		public void ReplacePropertyValue_Adds()
		{
			// Set up the instruction to replace.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.ReplaceOrAddPropertyValue,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass
				}
			};
			instruction.PropertyValue.TypedValue.SetValue
			(
				MFDataType.MFDatatypeLookup,
				(int)MFBuiltInDocumentClass.MFBuiltInDocumentClassOtherDocument
			);

			// Apply it.
			var propertyValues = new PropertyValues();
			instruction.ApplyTo(propertyValues);

			// Test the results.
			Assert.AreEqual(1, propertyValues.Count);
			Assert.AreEqual((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass, propertyValues[1].PropertyDef);
			Assert.AreEqual
			(
				(int)MFBuiltInDocumentClass.MFBuiltInDocumentClassOtherDocument,
				propertyValues[1].TypedValue.GetLookupID()
			);
		}

		[TestMethod]
		public void ReplacePropertyValue_Replaces()
		{
			// Set up the instruction to replace.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.ReplaceOrAddPropertyValue,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass
				}
			};
			instruction.PropertyValue.TypedValue.SetValue
			(
				MFDataType.MFDatatypeLookup,
				(int)MFBuiltInDocumentClass.MFBuiltInDocumentClassUnclassifiedDocument
			);

			// Apply it.
			var propertyValues = new PropertyValues();
			{
				var pv = new PropertyValue()
				{
					PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass
				};
				// Set the starting value to "other document".  Should be replaced.
				pv.TypedValue.SetValue(MFDataType.MFDatatypeLookup, (int)MFBuiltInDocumentClass.MFBuiltInDocumentClassOtherDocument);
				propertyValues.Add(-1, pv);
			}
			instruction.ApplyTo(propertyValues);

			// Test the results.
			Assert.AreEqual(1, propertyValues.Count);
			Assert.AreEqual((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass, propertyValues[1].PropertyDef);
			Assert.AreEqual
			(
				(int)MFBuiltInDocumentClass.MFBuiltInDocumentClassUnclassifiedDocument,
				propertyValues[1].TypedValue.GetLookupID()
			);
		}

		#endregion

		#region RemovePropertyValue

		[TestMethod]
		public void RemoveProperty()
		{
			// Set up the instruction to remove.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.RemovePropertyValue,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass
				}
			};

			// Apply it.
			var propertyValues = new PropertyValues();
			{
				var pv = new PropertyValue()
				{
					PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass
				};
				propertyValues.Add(-1, pv);
			}
			instruction.ApplyTo(propertyValues);

			// Test the results.
			Assert.AreEqual(0, propertyValues.Count);
		}

		[TestMethod]
		public void RemoveProperty_DoesNotThrowIfNotFound()
		{
			// Set up the instruction to remove.
			var instruction = new ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjectCopyOptions.PropertyValueInstructionType.RemovePropertyValue,
				PropertyValue = new PropertyValue()
				{
					PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass
				}
			};

			// Apply it.
			var propertyValues = new PropertyValues();
			instruction.ApplyTo(propertyValues);

			// Test the results.
			Assert.AreEqual(0, propertyValues.Count);
		}

		#endregion

	}
}
