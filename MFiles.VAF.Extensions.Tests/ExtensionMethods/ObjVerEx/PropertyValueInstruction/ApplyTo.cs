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
			new ObjVerExExtensionMethods.ObjectCopyOptions.PropertyValueInstruction().ApplyTo(null);
		}

		[TestMethod]
		public void AddProperty()
		{
			// Set up the instruction to add/update.
			var instruction = new ObjVerExExtensionMethods.ObjectCopyOptions.PropertyValueInstruction()
			{
				InstructionType = ObjVerExExtensionMethods.ObjectCopyOptions.PropertyValueInstructionType.AddValueToProperty,
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
	}
}
