using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class IsTemplate
		: TestBaseWithVaultMock
	{
		/// <summary>
		/// A null <see cref="Common.ObjVerEx"/> should throw an exception.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullObjVerExThrows()
		{
			((Common.ObjVerEx) null).IsTemplate();
		}
		
		/// <summary>
		/// If the <see cref="MFBuiltInPropertyDef.MFBuiltInPropertyDefIsTemplate"/>
		/// property does not exist in the  <see cref="Common.ObjVerEx.Properties"/> collection then 
		/// <see cref="ObjVerExExtensionMethods.IsTemplate"/> should return false.
		/// </summary>
		[TestMethod]
		public void EmptyPropertiesShouldReturnFalse()
		{
			var objVerEx = new Common.ObjVerEx();
			Assert.AreEqual(false, objVerEx.IsTemplate());
		}
		
		/// <summary>
		/// Check that the <see cref="ObjVerExExtensionMethods.IsTemplate"/> method returns correctly for various inputs.
		/// </summary>
		/// <param name="input">The value to set the <see cref="MFBuiltInPropertyDef.MFBuiltInPropertyDefIsTemplate"/> property value to.</param>
		/// <param name="expectedOutput">The expected output from <see cref="ObjVerExExtensionMethods.IsTemplate"/>.</param>
		[TestMethod]
		[DataRow(false, false)]
		[DataRow(true, true)]
		public void IsTemplatePropertyShouldReturnCorrectly
			(
			bool input,
			bool expectedOutput
			)
		{
			// Create the Is Template property.
			var propertyValue = new PropertyValue
			{
				PropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefIsTemplate
			};
			propertyValue.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, input);

			// Create the ObjVerEx and set the properties.
			var objVerEx = new Common.ObjVerEx();
			objVerEx.Properties.Add(-1, propertyValue);

			// Assert.
			Assert.AreEqual(expectedOutput, objVerEx.IsTemplate());
		}
	}
}
