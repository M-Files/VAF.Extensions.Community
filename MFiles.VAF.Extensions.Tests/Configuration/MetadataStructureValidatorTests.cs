using MFiles.VAF.Configuration;
using MFiles.VAF.Extensions.Configuration;
using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests.Configuration
{
	[TestClass]
	public class MetadataStructureValidatorTests
		: TestBaseWithVaultMock
	{

		protected override Mock<Vault> GetVaultMock()
		{
			// Create the object type mock.
			var objectTypeOperationsMock = new Mock<VaultObjectTypeOperations>();
			objectTypeOperationsMock.Setup(m => m.GetObjectTypeIDByAlias(It.IsAny<string>()))
				.Returns((string alias) =>
				{
					switch (alias?.ToLower()?.Trim())
					{
						case "hello_world":
							return 1;
						default:
							return -1;
					}
				});
			objectTypeOperationsMock.Setup(m => m.GetObjectType(It.IsAny<int>()))
				.Returns((int id) =>
				{
					switch (id)
					{
						case 1:
							{
								var objTypeMock = new Mock<ObjType>();
								objTypeMock.SetupAllProperties();
								objTypeMock.Setup(m => m.ID).Returns(id);
								objTypeMock.Setup(m => m.RealObjectType).Returns(true);
								objTypeMock.Setup(m => m.DefaultPropertyDef).Returns(123);
								objTypeMock.Setup(m => m.OwnerPropertyDef).Returns(321);
								return objTypeMock.Object;
							}
						default:
							throw new InvalidOperationException($"Object type with ID {id} was not mocked");
					}
				});

			// Create the property def mock.
			var propertyDefOperationsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefOperationsMock.Setup(m => m.GetPropertyDef(It.IsAny<int>()))
				.Returns((int id) =>
				{
					switch (id)
					{
						case 123:
						case 321:
							{
								var propertyDefMock = new Mock<PropertyDef>();
								propertyDefMock.SetupAllProperties();
								propertyDefMock.Setup(m => m.ID).Returns(id);
								return propertyDefMock.Object;
							}
						default:
							throw new InvalidOperationException($"Property def with ID {id} was not mocked");
					}
				});

			// Return an updated vault mock.
			var vaultMock = base.GetVaultMock();
			vaultMock.SetupGet(m => m.ObjectTypeOperations).Returns(() => objectTypeOperationsMock.Object);
			vaultMock.SetupGet(m => m.PropertyDefOperations).Returns(() => propertyDefOperationsMock.Object);
			return vaultMock;
		}

		public virtual IMetadataStructureValidator GetMetadataStructureValidator()
		{
			return new MFiles.VAF.Extensions.Configuration.MetadataStructureValidator();
		}

		[DataContract]
		class Configuration
		{
			[DataMember]
			[MFObjType]
			public MFIdentifier ObjectType { get; set; }

			[DefaultPropertyDef(nameof(ObjectType))]
			public MFIdentifier DefaultPropertyDef { get; set; }

			[OwnerPropertyDef(nameof(ObjectType))]
			public MFIdentifier OwnerPropertyDef { get; set; }

		}

		[TestMethod]
		public void HappyPath()
		{
			// The config should have a single valid object type defined.
			// The default/owner properties will be driven from this.
			var config = new Configuration()
			{
				ObjectType = "hello_world"
			};
			Assert.IsNull(config.DefaultPropertyDef);
			Assert.IsNull(config.OwnerPropertyDef);

			// Set up the required mocks and other constructs.
			var vaultMock = this.GetVaultMock();
			var validator = this.GetMetadataStructureValidator();
			var validationResult = new ValidationResultForValidation();

			// Check that the overall validation passed.
			Assert.IsTrue
			(
				validator.ValidateItem
				(
					vaultMock.Object,
					"MyConfigId",
					config,
					validationResult
				)
			);

			// Check that we got our properties populated.
			Assert.AreEqual(123, config.DefaultPropertyDef?.ID);
			Assert.AreEqual(321, config.OwnerPropertyDef?.ID);
		}

		[TestMethod]
		public void InvalidObjectTypeAlias()
		{
			// The config should have a single valid object type defined.
			// The default/owner properties will be driven from this.
			var config = new Configuration()
			{
				ObjectType = "invalidObjectTypeAlias"
			};
			Assert.IsNull(config.DefaultPropertyDef);
			Assert.IsNull(config.OwnerPropertyDef);

			// Set up the required mocks and other constructs.
			var vaultMock = this.GetVaultMock();
			var validator = this.GetMetadataStructureValidator();
			var validationResult = new ValidationResultForValidation();

			// Check that the overall validation failed due to the object type.
			Assert.IsFalse
			(
				validator.ValidateItem
				(
					vaultMock.Object,
					"MyConfigId",
					config,
					validationResult
				)
			);

			// Check that our properties are empty.
			Assert.IsNull(config.DefaultPropertyDef?.ID);
			Assert.IsNull(config.OwnerPropertyDef?.ID);
		}
	}
}
