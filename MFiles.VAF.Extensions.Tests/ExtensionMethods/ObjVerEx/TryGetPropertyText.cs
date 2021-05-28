using System;
using System.Collections.Generic;

using MFiles.VAF.Configuration;

using MFilesAPI;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	/// <summary>
	/// Tests <see cref="ObjVerExExtensionMethods.TryGetPropertyText(Common.ObjVerEx, out string, VAF.Configuration.MFIdentifier, string)"/>
	/// </summary>
	[TestClass]
	public class TryGetPropertyText
		: TestBaseWithVaultMock
	{
		#region Class for props params
		public class PropertyDefTestEnvironment
		{
			public PropertyDefTestEnvironment(MFIdentifier prop, Common.ObjVerEx objverEx)
			{
				Ident = prop;
				IsResolved = prop?.IsResolved ?? false;
				IsAssigned = IsResolved && null != objverEx && objverEx.HasProperty(prop);
				Value = IsResolved ? objverEx.GetPropertyText(prop) : null;
			}

			public MFIdentifier Ident;
			public bool IsResolved;
			public bool IsAssigned;
			public string Value;

			public override string ToString()
			{
				return $"\n- #PropDef: {Ident?.ID.ToString() ?? "null"}" +
					$"\n- Resolved: {IsResolved}\n- Assigned: {IsAssigned}" +
					$"\n- Value:    {(null == Value ? "null" : "\"" + Value + "\"")}";
			}
		}
		#endregion

		#region Initialized MFIdentifier and ObjVerEx objects for properties for testing
		private Common.ObjVerEx objVerEx;
		private const Common.ObjVerEx nullObjVerEx = null;
		private const int idCustomProp = 1111;
		private const string aliasCustomProp = "PD.Test";
		private PropertyDefTestEnvironment envNull;
		private PropertyDefTestEnvironment envNameOrTitle;
		private PropertyDefTestEnvironment envKeywords;
		private PropertyDefTestEnvironment envMessageID;
		private PropertyDefTestEnvironment envCustomProp;
		private PropertyDefTestEnvironment envNotResolved;
		private List<PropertyDefTestEnvironment> ListEnvironments => new List<PropertyDefTestEnvironment>()
		{
			envNull, envNameOrTitle, envKeywords, envMessageID, envCustomProp, envNotResolved
		};
		#endregion

		/// <summary>
		/// Ensures that a null <see cref="Common.ObjVerEx"/> reference throws an <see cref="ArgumentNullException"/>.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TryGetPropertyText_ThrowsIfNullObjVerEx()
			=> _ = nullObjVerEx.TryGetPropertyText(envNameOrTitle.Ident, out _);

		/// <summary>
		/// Ensures that not resolved <see cref="MFIdentifier"/> objects return <see cref="true"/>
		/// </summary>
		[TestMethod]
		public void TryGetPropertyText_ResultForNullIdentifier()
			=> Assert.IsFalse(objVerEx.TryGetPropertyText(envNull.Ident, out _));

		/// <summary>
		/// Ensures that not resolved <see cref="MFIdentifier"/> objects return <see cref="true"/>
		/// </summary>
		[TestMethod]
		public void TryGetPropertyText_ResultForNotResolved()
			=> Assert.IsFalse(objVerEx.TryGetPropertyText(envNotResolved.Ident, out _));

		/// <summary>
		/// Check all return values for all entries
		/// </summary>
		[TestMethod]
		public void TryGetPropertyText_CheckOutputForInitializedEntries()
		{
			foreach (PropertyDefTestEnvironment env in ListEnvironments)
			{
				Assert.AreEqual(env.IsResolved, objVerEx.TryGetPropertyText(env.Ident, out string result));
				Assert.AreEqual(env.Value, result);
			}
		}

		#region Test initialization
		/// <summary>
		/// Initialize <see cref="Common.ObjVerEx"/> object with property Name or Title set to <see cref="valueNameOrTitle"/>,
		/// property MessageID set to <see cref="valueMessageID"/> (null) and property Keywords known but not set.
		/// </summary>
		[TestInitialize]
		public void TryGetPropertyText_Setup()
		{
			// Mock the property definition operations object.
			Mock<VaultPropertyDefOperations> propertyDefinitionsMock = new Mock<VaultPropertyDefOperations>();

			propertyDefinitionsMock.Setup(m => m.GetPropertyDefIDByAlias(It.IsAny<string>()))
				.Returns((string propertyAlias) =>
				{
					return propertyAlias == aliasCustomProp ? idCustomProp : -1;
				})
				.Verifiable();

			propertyDefinitionsMock.Setup(m => m.GetPropertyDef(It.IsAny<int>()))
				.Returns((int propertyDef) =>
				{
					switch (propertyDef)
					{
						case (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle:
						case (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefKeywords:
						case (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefMessageID:
						case idCustomProp:
							return new PropertyDef
							{
								ID = propertyDef,
								DataType = MFDataType.MFDatatypeText,
								Name = $"Property_{propertyDef}",
							};
						default:
							return null;

					}
				})
				.Verifiable();

			// Mock the vault.
			Mock<Vault> vaultMock = GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);

			// Set up the data for the ObjVerEx.
			ObjVer objVer = new ObjVer();
			objVer.SetIDs((int) MFBuiltInObjectType.MFBuiltInObjectTypeDocument, ID: 1, Version: 1);
			Mock<ObjectVersion> objectVersionMock = new Mock<ObjectVersion>();
			objectVersionMock.SetupGet(m => m.ObjVer).Returns(objVer);

			// Setup properties for NameOrTitle and MessageID (NOT: Keywords)
			PropertyValue pv;
			PropertyValues properties = new PropertyValues();
			{
				// NameOrTitle
				pv = new PropertyValue
				{
					PropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle,
				};
				pv.TypedValue.SetValue(MFDataType.MFDatatypeText, "valueNameOrTitle");
				properties.Add(1, pv);

				// MessageID
				pv = new PropertyValue
				{
					PropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefMessageID,
				};
				pv.TypedValue.SetValue(MFDataType.MFDatatypeText, null);
				properties.Add(2, pv);

				// CustomProp
				pv = new PropertyValue
				{
					PropertyDef = idCustomProp,
				};
				pv.TypedValue.SetValue(MFDataType.MFDatatypeText, "valueCustomProp");
				properties.Add(3, pv);
			}

			// Create the ObjVerEx.
			objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionMock.Object, properties);

			// Get the test property params object
			MFIdentifier identCurrent = null;
			envNull = new PropertyDefTestEnvironment(identCurrent, objVerEx);

			identCurrent = new MFIdentifier((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle);
			identCurrent.Resolve(vaultMock.Object, typeof(PropertyDef));
			envNameOrTitle = new PropertyDefTestEnvironment(identCurrent, objVerEx);

			identCurrent = new MFIdentifier((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefKeywords);
			identCurrent.Resolve(vaultMock.Object, typeof(PropertyDef));
			envKeywords = new PropertyDefTestEnvironment(identCurrent, objVerEx);

			identCurrent = new MFIdentifier((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefMessageID);
			identCurrent.Resolve(vaultMock.Object, typeof(PropertyDef));
			envMessageID= new PropertyDefTestEnvironment(identCurrent, objVerEx);

			identCurrent = new MFIdentifier(aliasCustomProp);
			identCurrent.Resolve(vaultMock.Object, typeof(PropertyDef));
			envCustomProp = new PropertyDefTestEnvironment(identCurrent, objVerEx);

			identCurrent = new MFIdentifier("incorrectAlias");
			identCurrent.Resolve(vaultMock.Object, typeof(PropertyDef));
			envNotResolved = new PropertyDefTestEnvironment(identCurrent, objVerEx);
		}
		#endregion
	}
}
