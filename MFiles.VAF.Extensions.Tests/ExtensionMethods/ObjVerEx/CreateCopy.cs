using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MFiles.VAF.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using MFilesAPI;
using System.IO;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
	[TestClass]
	public class CreateCopy
		: TestBaseWithVaultMock
	{
		static int count = 0;
		protected override Mock<Vault> GetVaultMock()
		{
			var vaultMock = base.GetVaultMock();

			var objectOperationsMock = new Mock<VaultObjectOperations>();
			objectOperationsMock.Setup
			(
				m => m.GetObjectPermissions(It.IsAny<ObjVer>())
			)
			.Returns(new Mock<ObjectVersionPermissions>().Object);
			vaultMock.Setup(m => m.ObjectOperations).Returns(objectOperationsMock.Object);

			var propertyDefOperationsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefOperationsMock.Setup(m => m.GetPropertyDef(It.IsAny<int>()))
				.Returns((int id) =>
				{
					var mock = new Mock<PropertyDef>();
					mock.Setup(m => m.ID).Returns(id);
					return mock.Object;
				});
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefOperationsMock.Object);

			return vaultMock;
		}

		protected virtual Mock<ObjectVersionAndProperties> CreateObjectVersionAndPropertiesMock
		(
			Vault vault,
			int objectType = 0,
			PropertyValues propertyValues = null,
			ObjectFiles objectFiles = null
		)
		{
			// Sanity.
			propertyValues = propertyValues ?? new PropertyValues();

			// Create a new object version.
			var objVer = new ObjVer();
			objVer.SetIDs(objectType, ++count, 1);

			// Create the object version mock.
			var objectVersionMock = new Mock<ObjectVersion>();
			objectVersionMock.Setup(m => m.ObjVer).Returns(objVer);
			objectVersionMock.Setup(m => m.Files).Returns(objectFiles ?? new Mock<ObjectFiles>().Object);
			objectVersionMock.Setup(m => m.FilesCount).Returns(objectFiles?.Count ?? 0);

			// Create the mock for the object version and properties.
			var objectVersionAndPropertiesMock = new Mock<ObjectVersionAndProperties>();
			objectVersionAndPropertiesMock.SetupAllProperties();
			objectVersionAndPropertiesMock.Setup(m => m.ObjVer).Returns(objVer);
			objectVersionAndPropertiesMock.Setup(m => m.Vault).Returns(vault);
			objectVersionAndPropertiesMock.Setup(m => m.VersionData).Returns(objectVersionMock.Object);
			objectVersionAndPropertiesMock.Setup(m => m.Properties).Returns(propertyValues);
			return objectVersionAndPropertiesMock;
		}

		internal virtual Mock<IObjectCopyCreator> GetObjectCopyCreatorMock()
		{
			var objectCopyCreatorMock = new Mock<IObjectCopyCreator>();
			objectCopyCreatorMock
				.Setup
				(
					m => m.CreateObject
					(
						It.IsAny<Vault>(),
						It.IsAny<int>(),
						It.IsAny<PropertyValues>(),
						It.IsAny<SourceObjectFiles>(),
						It.IsAny<bool>(),
						It.IsAny<bool>(),
						It.IsAny<AccessControlList>()
					)
				)
				.Returns(
				(
					Vault vault,
					int objectType,
					PropertyValues propertyValues,
					SourceObjectFiles sourceObjectFiles,
					bool singleFileDocument,
					bool checkIn,
					AccessControlList accessControlList
				) =>
				{
					var objectFiles = new Mock<ObjectFiles>();
					objectFiles.Setup(m => m.Count).Returns(sourceObjectFiles?.Count ?? 0);
					var converted = new List<ObjectFile>();
					if (null != sourceObjectFiles)
					{
						foreach (SourceObjectFile item in sourceObjectFiles)
						{
							var file = new Mock<ObjectFile>();
							file.Setup(m => m.Title).Returns(item.Title);
							file.Setup(m => m.Extension).Returns(item.Extension);
							converted.Add(file.Object);
						}
					}
					objectFiles.Setup(m => m.GetEnumerator()).Returns(() => converted.GetEnumerator());
					objectFiles.Setup(m => m[It.IsAny<int>()]).Returns((int index) => converted[index + 1]);
					return this.CreateObjectVersionAndPropertiesMock(vault, objectType, propertyValues, objectFiles.Object).Object;
				});
			objectCopyCreatorMock
				.Setup(m => m.SetSingleFileDocument(It.IsAny<Common.ObjVerEx>(), It.IsAny<bool>()))
				.Callback((Common.ObjVerEx objVerEx, bool sfd) =>
				{
					objVerEx.SetProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject, MFDataType.MFDatatypeBoolean, sfd);
				});
			return objectCopyCreatorMock;
		}

		protected Common.ObjVerEx CreateSourceObject
		(
			params Tuple<int, MFDataType, object>[] propertyValues
		)
		{
			return this.CreateSourceObject((Mock<Vault>)null, propertyValues);
		}
		protected Common.ObjVerEx CreateSourceObject
		(
			Mock<Vault> vaultMock,
			params Tuple<int, MFDataType, object>[] propertyValues
		)
		{
			return this.CreateSourceObject(vaultMock, null, propertyValues);
		}
		protected Common.ObjVerEx CreateSourceObject
		(
			SourceObjectFiles sourceObjectFiles,
			params Tuple<int, MFDataType, object>[] propertyValues
		)
		{
			return this.CreateSourceObject(null, sourceObjectFiles, propertyValues);
		}
		protected Common.ObjVerEx CreateSourceObject
		(
			Mock<Vault> vaultMock,
			SourceObjectFiles sourceObjectFiles,
			params Tuple<int, MFDataType, object>[] propertyValues
		)
		{
			return this.CreateSourceObjectWithPropertyValues
			(
				vaultMock,
				sourceObjectFiles,
				(propertyValues ?? new Tuple<int, MFDataType, object>[0])
					.Select(t =>
					{
						var pv = new PropertyValue() { PropertyDef = t.Item1 };
						pv.TypedValue.SetValue(t.Item2, t.Item3);
						return pv;
					})
					.ToArray()
			);
		}

		protected Common.ObjVerEx CreateSourceObjectWithPropertyValues
		(
			params PropertyValue[] propertyValues
		)
		{
			return this.CreateSourceObjectWithPropertyValues(null, null, propertyValues);
		}

		protected Common.ObjVerEx CreateSourceObjectWithPropertyValues
		(
			Mock<Vault> vaultMock,
			params PropertyValue[] propertyValues
		)
		{
			return this.CreateSourceObjectWithPropertyValues(vaultMock, null, propertyValues);
		}

		protected Common.ObjVerEx CreateSourceObjectWithPropertyValues
		(
			SourceObjectFiles sourceObjectFiles,
			params PropertyValue[] propertyValues
		)
		{
			return this.CreateSourceObjectWithPropertyValues(null, sourceObjectFiles, propertyValues);
		}
		protected Common.ObjVerEx CreateSourceObjectWithPropertyValues
		(
			Mock<Vault> vaultMock,
			SourceObjectFiles sourceObjectFiles,
			params PropertyValue[] propertyValues
		)
		{
			// Create our mock objects.
			vaultMock = vaultMock ?? this.GetVaultMock();

			// Create the property values collection.
			var pvs = new PropertyValues();
			foreach (var pv in propertyValues ?? new PropertyValue[0])
			{
				pvs.Add(-1, pv);
			}

			// Set up the object files.
			var objectFiles = new Mock<ObjectFiles>();
			objectFiles.Setup(m => m.Count).Returns(sourceObjectFiles?.Count ?? 0);
			var converted = new List<ObjectFile>();
			if (null != sourceObjectFiles)
			{
				foreach (SourceObjectFile item in sourceObjectFiles)
				{
					var file = new Mock<ObjectFile>();
					file.Setup(m => m.Title).Returns(item.Title);
					file.Setup(m => m.Extension).Returns(item.Extension);
					converted.Add(file.Object);
				}
			}
			objectFiles.Setup(m => m.GetEnumerator()).Returns(() => converted.GetEnumerator());
			objectFiles.Setup(m => m[It.IsAny<int>()]).Returns((int index) => converted[index + 1]);

			// Create the source object itself.
			return new VAF.Common.ObjVerEx
			(
				vaultMock.Object,
				this.CreateObjectVersionAndPropertiesMock
				(
					vaultMock.Object,
					propertyValues: pvs,
					objectFiles: objectFiles.Object
				).Object
			);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullObjVerExThrows()
		{
			((Common.ObjVerEx)null).CreateCopy(objectCopyOptions: new ObjectCopyOptions());
		}

		[TestMethod]
		public void CopyRemovesDeletedProperties()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();

			// Set up the vault.
			var propertyDefOperationsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefOperationsMock.Setup(m => m.GetPropertyDef(It.IsAny<int>()))
				.Throws(new InvalidOperationException("This property was deleted"));
			var vaultMock = this.GetVaultMock();
			vaultMock.Setup(m => m.PropertyDefOperations).Returns(propertyDefOperationsMock.Object);

			// Create our source object.
			var sourceObject = this.CreateSourceObject
			(
				vaultMock,
				new Tuple<int, MFDataType, object>(123, MFDataType.MFDatatypeMultiLineText, "hello world")
			);

			// Execute.
			var copy = sourceObject.CreateCopy(objectCopyCreator: objectCopyCreatorMock.Object);

			// Property 123 should have been removed.
			// Note: property 22 is added by the code, so we will always end up with one!
			Assert.IsFalse(copy.Properties.Cast<PropertyValue>().Any(pv => pv.PropertyDef == 123));
		}

		[TestMethod]
		public void CopyIsNotNull()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();

			// Create our source object.
			var sourceObject = this.CreateSourceObject();

			// Execute.
			var copy = sourceObject.CreateCopy(objectCopyCreator: objectCopyCreatorMock.Object);

			// Target should not be null.
			Assert.IsNotNull(copy);
			Assert.IsNotNull(copy.Properties);
		}

		[TestMethod]
		public void SourcePropertiesCopied()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();

			// Create our source object.
			var sourceObject = this.CreateSourceObject
			(
				new Tuple<int, MFDataType, object>
				(
					(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass,
					MFDataType.MFDatatypeLookup,
					123
				),
				new Tuple<int, MFDataType, object>
				(
					(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle,
					MFDataType.MFDatatypeText,
					"hello world"
				)
			);

			// Execute.
			var copy = sourceObject.CreateCopy(new ObjectCopyOptions()
			{
				SetSingleFileDocumentIfAppropriate = false
			}, objectCopyCreator: objectCopyCreatorMock.Object);

			// Properties should be correct.
			Assert.AreEqual(2, copy.Properties.Count);
			Assert.AreEqual((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass, copy.Properties[1].PropertyDef);
			Assert.AreEqual(123, copy.Properties[1].TypedValue.GetLookupID());
			Assert.AreEqual((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle, copy.Properties[2].PropertyDef);
			Assert.AreEqual("hello world", copy.Properties[2].TypedValue.DisplayValue);
		}

		[TestMethod]
		public void SFDRemovedFromSource()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();

			// Create our source object.
			var sourceObject = this.CreateSourceObject
			(
				new Tuple<int, MFDataType, object>
				(
					(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject,
					MFDataType.MFDatatypeBoolean,
					true
				)
			);

			// Execute.
			var copy = sourceObject.CreateCopy
			(
				new ObjectCopyOptions()
				{
					SetSingleFileDocumentIfAppropriate = false
				},
				objectCopyCreator: objectCopyCreatorMock.Object
			);

			// Properties should be correct.
			Assert.AreEqual(0, copy.Properties.Count);
		}

		[TestMethod]
		public void SFDAddedToTargetIfAppropriate_False()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();

			// Create our source object.
			var sourceObject = this.CreateSourceObject
			(
				new Tuple<int, MFDataType, object>
				(
					(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject,
					MFDataType.MFDatatypeBoolean,
					true
				)
			);

			// Execute.
			var copy = sourceObject.CreateCopy
			(
				new ObjectCopyOptions()
				{
					SetSingleFileDocumentIfAppropriate = true
				},
				objectCopyCreator: objectCopyCreatorMock.Object
			);

			// Properties should be correct.
			Assert.AreEqual(1, copy.Properties.Count);
			Assert.AreEqual(false, copy.GetProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject).Value.Value);
		}

		[TestMethod]
		public void SFDAddedToTargetIfAppropriate_True()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();

			// Create the source files.
			var sourceObjectFiles = new SourceObjectFiles();
			{
				sourceObjectFiles.Add(1, new SourceObjectFile()
				{
					Title = "test file",
					Extension = ".docx"
				});
			}

			// Create our source object.
			var sourceObject = this.CreateSourceObject
			(
				sourceObjectFiles,
				new Tuple<int, MFDataType, object>
				(
					(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject,
					MFDataType.MFDatatypeBoolean,
					true
				)
			);

			// Execute.
			var copy = sourceObject.CreateCopy
			(
				new ObjectCopyOptions()
				{
					SetSingleFileDocumentIfAppropriate = true
				},
				objectCopyCreator: objectCopyCreatorMock.Object
			);

			// Properties should be correct.
			Assert.AreEqual(1, copy.Properties.Count);
			Assert.AreEqual(true, copy.GetProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject).Value.Value);
		}

		// When RemoveSystemProperties = true (default), the system properties should be removed.
		[TestMethod]
		public void SourcePropertiesCopied_SystemPropertiesRemoved()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();

			// Create our source object.
			var sourceObject = this.CreateSourceObject
			(
				new Tuple<int, MFDataType, object>
				(
					(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified,
					MFDataType.MFDatatypeTimestamp,
					new Timestamp()
				)
			);

			// Execute.
			var copy = sourceObject.CreateCopy
			(
				new ObjectCopyOptions()
				{
					RemoveSystemProperties = true,
					SetSingleFileDocumentIfAppropriate = false
				},
				objectCopyCreator: objectCopyCreatorMock.Object
			);

			// Properties should be correct.
			Assert.AreEqual(0, copy.Properties.Count);
		}

		// When RemoveSystemProperties = false, the system properties should not be removed.
		[TestMethod]
		public void SourcePropertiesCopied_SystemPropertiesRetained()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();

			// Create our source object.
			var sourceObject = this.CreateSourceObject
			(
				new Tuple<int, MFDataType, object>
				(
					(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified,
					MFDataType.MFDatatypeTimestamp,
					new Timestamp()
				)
			);

			// Execute.
			var copy = sourceObject.CreateCopy
			(
				new ObjectCopyOptions()
				{
					RemoveSystemProperties = false,
					SetSingleFileDocumentIfAppropriate = false
				},
				objectCopyCreator: objectCopyCreatorMock.Object
			);

			// Properties should be correct.
			Assert.AreEqual(1, copy.Properties.Count);
		}

		[TestMethod]
		public void CorrectTargetType()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();

			// Create our source object.
			var sourceObject = this.CreateSourceObject();

			// Execute.
			var copy = sourceObject.CreateCopy
			(
				objectCopyOptions: new ObjectCopyOptions()
				{
					TargetObjectType = 123 // Not a document.
				},
				objectCopyCreator: objectCopyCreatorMock.Object
			);

			// Source was a document, but we overrode that in the creation.
			Assert.AreEqual(123, copy.ObjVer.Type);
		}

		[TestMethod]
		public void SingleFileDocumentSetToFalseForNonDocumentObjects()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();

			// Create our source object.
			var sourceObject = this.CreateSourceObject();

			// Execute.
			var copy = sourceObject.CreateCopy
			(
				objectCopyOptions: new ObjectCopyOptions()
				{
					TargetObjectType = 123 // Not a document.
				},
				objectCopyCreator: objectCopyCreatorMock.Object
			);

			// SFD = false.
			Assert.AreEqual(1, copy.Properties.Count);
			Assert.AreEqual((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject, copy.Properties[1].PropertyDef);
			Assert.AreEqual(false, copy.Properties[1].TypedValue.Value);
		}

		[TestMethod]
		public void PropertyValueInstructionsAreApplied()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();

			// Create our source object.
			var sourceObject = this.CreateSourceObject();

			// Create the property value instructions.
			var properties = new List<ObjectCopyOptions.PropertyValueInstruction>
			{
				new ObjectCopyOptions.PropertyValueInstruction
				(
					ObjectCopyOptions.PropertyValueInstructionType.ReplaceOrAddPropertyValue,
					(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass,
					MFDataType.MFDatatypeLookup,
					1234
				)
			};

			// Execute.
			var copy = sourceObject.CreateCopy
			(
				objectCopyOptions: new ObjectCopyOptions()
				{
					Properties = properties,
					SetSingleFileDocumentIfAppropriate = false
				},
				objectCopyCreator: objectCopyCreatorMock.Object
			);

			// SFD = false.
			Assert.AreEqual(1, copy.Properties.Count);
			Assert.AreEqual((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass, copy.Properties[1].PropertyDef);
			Assert.AreEqual(1234, copy.Properties[1].TypedValue.GetLookupID());
		}

		[TestMethod]
		public void CheckIn_IsCalledWhenTrue()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();
			var checkedIn = false;
			objectCopyCreatorMock
				.Setup(m => m.CheckIn(It.IsAny<Common.ObjVerEx>(), It.IsAny<string>(), It.IsAny<int>()))
				.Callback((Common.ObjVerEx objVerEx, string comments, int userId) =>
				{
					checkedIn = true;
				})
				.Verifiable();

			// Create our source object.
			var sourceObject = this.CreateSourceObject();

			// Execute.
			var copy = sourceObject.CreateCopy
			(
				objectCopyOptions: new ObjectCopyOptions()
				{
					CheckInObject = true
				},
				objectCopyCreator: objectCopyCreatorMock.Object
			);

			// Assert that checkin was called.
			objectCopyCreatorMock.Verify();
			Assert.AreEqual(true, checkedIn);
		}

		[TestMethod]
		public void CheckIn_CommentsAndUserIdCorrect()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();
			objectCopyCreatorMock
				.Setup(m => m.CheckIn(It.IsAny<Common.ObjVerEx>(), It.IsAny<string>(), It.IsAny<int>()))
				.Callback((Common.ObjVerEx objVerEx, string comments, int userId) =>
				{
					Assert.AreEqual("hello world", comments);
					Assert.AreEqual(1234, userId);
				})
				.Verifiable();

			// Create our source object.
			var sourceObject = this.CreateSourceObject();

			// Execute.
			var copy = sourceObject.CreateCopy
			(
				objectCopyOptions: new ObjectCopyOptions()
				{
					CheckInObject = true,
					CheckInComments = "hello world",
					CreatedByUserId = 1234
				},
				objectCopyCreator: objectCopyCreatorMock.Object
			);

			// Assert that checkin was called.
			objectCopyCreatorMock.Verify();
		}

		[TestMethod]
		public void CheckIn_IsNotCalledWhenFalse()
		{
			// Create our mock objects.
			var objectCopyCreatorMock = this.GetObjectCopyCreatorMock();
			var checkedIn = false;
			objectCopyCreatorMock
				.Setup(m => m.CheckIn(It.IsAny<Common.ObjVerEx>(), It.IsAny<string>(), It.IsAny<int>()))
				.Callback((Common.ObjVerEx objVerEx, string comments, int userId) =>
				{
					checkedIn = true;
				});

			// Create our source object.
			var sourceObject = this.CreateSourceObject();

			// Execute.
			var copy = sourceObject.CreateCopy
			(
				objectCopyOptions: new ObjectCopyOptions()
				{
					CheckInObject = false
				},
				objectCopyCreator: objectCopyCreatorMock.Object
			);

			// Assert that checkin was not called.
			Assert.AreEqual(false, checkedIn);
		}

	}
}
