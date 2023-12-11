using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MFiles.VAF.Common;
using MFilesAPI;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
    /// <summary>
    /// Tests <see cref="ObjVerExExtensionMethods.GetPropertyAsDateTime(Common.ObjVerEx, int)"/>.
    /// </summary>
    [TestClass]
    [ValidDataTypes(MFDataType.MFDatatypeDate, MFDataType.MFDatatypeTimestamp)]
    public class GetPropertyAsDateTime
        : GetPropertyAsSimpleTypeBase<DateTime?>
    {
        /// <inheritdoc />
        public GetPropertyAsDateTime()
            : base((objVerEx, propertyDefId) => objVerEx.GetPropertyAsDateTime(propertyDefId))
        {
		}

		[TestMethod]
		public void DateTimeKind_SetToUTC()
		{
			var properties = new PropertyValues();
			var objVer = new ObjVer();
			objVer.SetIDs(0, 1, 1);
			var objectVersionAndPropertiesMock = new Mock<ObjectVersionAndProperties>();
			objectVersionAndPropertiesMock.Setup(m => m.Properties).Returns(properties);
			objectVersionAndPropertiesMock.Setup(m => m.ObjVer).Returns(objVer);

			var propertyDefOperationsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefOperationsMock
				.Setup(m => m.GetPropertyDef(123))
				.Returns
				(
					new PropertyDef()
					{
						DataType = MFDataType.MFDatatypeTimestamp,
						ID = 123
					}
				);
			propertyDefOperationsMock.SetupAllProperties();

			var vaultMock = base.GetVaultMock();
			vaultMock
				.Setup(m => m.PropertyDefOperations)
				.Returns(propertyDefOperationsMock.Object);

			var timestamp = new Timestamp();
			timestamp.SetValue(new DateTime(2020, 01, 01));
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			objVerEx.Properties.SetProperty
			(
				123,
				MFDataType.MFDatatypeTimestamp,
				timestamp
			);
			var output = this.CallGetPropertyMethod(objVerEx, 123);
			Assert.IsTrue(output.HasValue);
			Assert.AreEqual(DateTimeKind.Utc, output.Value.Kind);
		}

		[TestMethod]
		public void Timestamp_IsNull_ReturnsNull()
		{
			var properties = new PropertyValues();
			var objVer = new ObjVer();
			objVer.SetIDs(0, 1, 1);
			var objectVersionAndPropertiesMock = new Mock<ObjectVersionAndProperties>();
			objectVersionAndPropertiesMock.Setup(m => m.Properties).Returns(properties);
			objectVersionAndPropertiesMock.Setup(m => m.ObjVer).Returns(objVer);

			var propertyDefOperationsMock = new Mock<VaultPropertyDefOperations>();
			propertyDefOperationsMock
				.Setup(m => m.GetPropertyDef(123))
				.Returns
				(
					new PropertyDef()
					{
						DataType = MFDataType.MFDatatypeTimestamp,
						ID = 123
					}
				);
			propertyDefOperationsMock.SetupAllProperties();

			var vaultMock = base.GetVaultMock();
			vaultMock
				.Setup(m => m.PropertyDefOperations)
				.Returns(propertyDefOperationsMock.Object);

			var nullTypedValue = new TypedValue();
			nullTypedValue.SetValueToNULL(MFDataType.MFDatatypeTimestamp);
			var objVerEx = new Common.ObjVerEx(vaultMock.Object, objectVersionAndPropertiesMock.Object);
			objVerEx.Properties.SetProperty
			(
				123,
				nullTypedValue
			);
			var output = this.CallGetPropertyMethod(objVerEx, 123);
			Assert.IsFalse(output.HasValue);
		}
	}

	/// <summary>
	/// Tests <see cref="ObjVerExExtensionMethods.GetPropertyAsBoolean(Common.ObjVerEx, int)"/>.
	/// </summary>
	[TestClass]
    [ValidDataTypes(MFDataType.MFDatatypeBoolean)]
    public class GetPropertyAsBoolean
        : GetPropertyAsSimpleTypeBase<bool?>
    {
        /// <inheritdoc />
        public GetPropertyAsBoolean()
            : base((objVerEx, propertyDefId) => objVerEx.GetPropertyAsBoolean(propertyDefId))
        {
        }
    }

    /// <summary>
    /// Tests <see cref="ObjVerExExtensionMethods.GetPropertyAsInteger(Common.ObjVerEx, int)"/>.
    /// </summary>
    [TestClass]
    [ValidDataTypes(MFDataType.MFDatatypeInteger)]
    public class GetPropertyAsInteger
        : GetPropertyAsSimpleTypeBase<int?>
    {
        /// <inheritdoc />
        public GetPropertyAsInteger()
            : base((objVerEx, propertyDefId) => objVerEx.GetPropertyAsInteger(propertyDefId))
        {
        }
    }

    /// <summary>
    /// Tests <see cref="ObjVerExExtensionMethods.GetPropertyAsLong(Common.ObjVerEx, int)"/>.
    /// </summary>
    [TestClass]
    [ValidDataTypes(MFDataType.MFDatatypeInteger64)]
    public class GetPropertyAsLong
        : GetPropertyAsSimpleTypeBase<long?>
    {
        /// <inheritdoc />
        public GetPropertyAsLong()
            : base((objVerEx, propertyDefId) => objVerEx.GetPropertyAsLong(propertyDefId))
        {
        }
    }
    
    /// <summary>
    /// Tests <see cref="ObjVerExExtensionMethods.GetPropertyAsDouble(Common.ObjVerEx, int)"/>.
    /// </summary>
    [TestClass]
    [ValidDataTypes(MFDataType.MFDatatypeFloating)]
    public class GetPropertyAsDouble
        : GetPropertyAsSimpleTypeBase<double?>
    {
        /// <inheritdoc />
        public GetPropertyAsDouble()
            : base((objVerEx, propertyDefId) => objVerEx.GetPropertyAsDouble(propertyDefId))
        {
        }
    }

    /// <summary>
    /// Common tests for methods that wrap <see cref="ObjVerExExtensionMethods.GetPropertyAs{T}(Common.ObjVerEx, int, MFilesAPI.MFDataType[])"/>.
    /// </summary>
    /// <typeparam name="T">The type passed to <see cref="ObjVerExExtensionMethods.GetPropertyAs{T}(Common.ObjVerEx, int, MFilesAPI.MFDataType[])"/>.</typeparam>
    public abstract class GetPropertyAsSimpleTypeBase<T>
        : TestBaseWithVaultMock
    {
        /// <summary>
        /// The method to be tested.
        /// </summary>
        public Func<Common.ObjVerEx, int, T> CallGetPropertyMethod { get; private set; }

        public virtual List<MFDataType> GetValidDataTypes()
        {
            return GetValidDataTypes(this.GetType());
        }

        public virtual List<MFDataType> GetInvalidDataTypes()
        {
            return GetInvalidDataTypes(this.GetType());
        }

        /// <summary>
        /// Gets the data types that are valid for this method.
        /// </summary>
        public static List<MFDataType> GetValidDataTypes(Type t)
        {
            // Get the valid data types attribute.
            var validDataTypes = t?.GetCustomAttribute<ValidDataTypesAttribute>(true);
            if (null == validDataTypes)
                return new List<MFDataType>();

            // Return the types specified.
            return validDataTypes.GetValidDataTypes();
        }

        /// <summary>
        /// Gets the data types that are valid for this method.
        /// </summary>
        public static List<MFDataType> GetInvalidDataTypes(Type t)
        {
            // Get the valid data types attribute.
            var validDataTypes = t?.GetCustomAttribute<ValidDataTypesAttribute>(true);
            if (null == validDataTypes)
                return new List<MFDataType>();

            // Return the types specified.
            return validDataTypes.GetInvalidDataTypes();
        }

        protected GetPropertyAsSimpleTypeBase
        (
            Func<Common.ObjVerEx, int, T> callGetPropertyMethod
        )
        {
            // Sanity.
            this.CallGetPropertyMethod =
                callGetPropertyMethod ?? throw new ArgumentNullException(nameof(callGetPropertyMethod));
        }

        /// <summary>
        /// Ensures that the method throws an exception if a null ObjVerEx reference is passed.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionForNullObjVerEx()
        {
            this.CallGetPropertyMethod(null, 123);
        }
        
        /// <summary>
        /// Ensures that the method throws an exception if a negative property definition Id is passed
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowsExceptionForNegativePropertyDef()
        {
            this.CallGetPropertyMethod(new Common.ObjVerEx(), -1);
        }
        
        /// <summary>
        /// Ensures that the method does not throw an exception if a zero property definition Id is passed.
        /// </summary>
        [TestMethod]
        public void DoesNotExceptForZeroPropertyDef()
        {
            this.CallGetPropertyMethod(new Common.ObjVerEx(), 0);
        }

        [TestMethod]
        public void ReturnsDefaultIfPropertyDoesNotExistInCollection()
        {
            // Create objVerEx instance.
            var objVerEx = new Common.ObjVerEx
            (
                this.GetVaultMock().Object,
                Mock.Of<ObjectVersion>(ov => ov.ObjVer == new ObjVer()),
                new PropertyValues()
            );

            // Call method.
            var output = this.CallGetPropertyMethod(objVerEx, 12345);

            // Ensure we got a default output.
            Assert.AreEqual( default, output );
        }

        [TestMethod]
        public void ReturnsDefaultIfPropertyIsNull()
        {
            // Set up the properties.
            var propertyValues = new PropertyValues();

            // Add the property value as null.
            {
                var propertyValue = new PropertyValue
                {
                    PropertyDef = 12345
                };
                propertyValue.TypedValue.SetValueToNULL(this.GetValidDataTypes()[0]);
                propertyValues.Add(-1, propertyValue);
            }

            // Create objVerEx instance.
            var objVerEx = new Common.ObjVerEx
            (
                this.GetVaultMock().Object,
                Mock.Of<ObjectVersion>(ov => ov.ObjVer == new ObjVer()),
                propertyValues
            );

            // Call method.
            var output = this.CallGetPropertyMethod(objVerEx, 12345);

            // Ensure we got a default output.
            Assert.AreEqual(default, output);
        }

        [TestMethod]
        public void ReturnsDefaultIfPropertyIsUninitialized()
        {
            // Set up the properties.
            var propertyValues = new PropertyValues();

            // Add the property value as uninitialised.
            {
                var propertyValue = new PropertyValue
                {
                    PropertyDef = 12345
                };
                propertyValue.TypedValue.SetValueToNULL(MFDataType.MFDatatypeUninitialized);
                propertyValues.Add(-1, propertyValue);
            }

            // Create objVerEx instance.
            var objVerEx = new Common.ObjVerEx
            (
                this.GetVaultMock().Object,
                Mock.Of<ObjectVersion>(ov => ov.ObjVer == new ObjVer()),
                propertyValues
            );

            // Call method.
            var output = this.CallGetPropertyMethod(objVerEx, 12345);

            // Ensure we got a default output.
            Assert.AreEqual(default, output);
        }

        [TestMethod]
        public void ValidPropertyDataTypeDoesNotThrowException()
        {
            // Run this for each invalid data type.
            foreach (var dataType in this.GetValidDataTypes())
            {
                // Set up the property definitions mock.
                var propertyDefinitionsMock = new Mock<VaultPropertyDefOperations>();
                propertyDefinitionsMock
                    .Setup(pd => pd.GetPropertyDef(It.IsAny<int>()))
                    .Returns((int propertyDefId) => new PropertyDef()
                    {
                        DataType = dataType,
                        ID = propertyDefId
                    });

                // Set up the vault mock.
                var vaultMock = this.GetVaultMock();
                vaultMock.Setup(v => v.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);

                // Set up the properties.
                var propertyValues = new PropertyValues();

                // Add the property value.
                try
                {
                    var propertyValue = new PropertyValue
                    {
                        PropertyDef = 12345
                    };
                    propertyValue.TypedValue.SetValue(dataType, this.GetSampleValueForDataType(dataType));
                    propertyValues.Add(-1, propertyValue);
                }
                catch(Exception e)
                {
                    throw new InvalidOperationException
                    (
                        $"Exception whilst creating sample value for data type {dataType}.",
                        e
                    );
                }

                // Create objVerEx instance.
                var objVerEx = new Common.ObjVerEx
                (
                    vaultMock.Object,
                    Mock.Of<ObjectVersion>(ov => ov.ObjVer == new ObjVer()),
                    propertyValues
                );

                // Call method.
                try
                {
                    this.CallGetPropertyMethod(objVerEx, 12345);
                }
                catch(Exception e)
                {
                    Assert.Fail($"Unexpected exception ({e.GetType().FullName}) thrown when testing data type {dataType}: {e.Message}");
                }
            }
        }

        [TestMethod]
        public void ThrowsExceptionForInvalidPropertyDataType()
        {
            // Run this for each invalid data type.
            foreach (var dataType in this.GetInvalidDataTypes())
            {
                // Set up the property definitions mock.
                var propertyDefinitionsMock = new Mock<VaultPropertyDefOperations>();
                propertyDefinitionsMock
                    .Setup(pd => pd.GetPropertyDef(It.IsAny<int>()))
                    .Returns((int propertyDefId) => new PropertyDef()
                    {
                        DataType = dataType,
                        ID = propertyDefId
                    });

                // Set up the vault mock.
                var vaultMock = this.GetVaultMock();
                vaultMock.Setup(v => v.PropertyDefOperations).Returns(propertyDefinitionsMock.Object);

                // Set up the properties.
                var propertyValues = new PropertyValues();

                // Add the property value.
                try
                {
                    var propertyValue = new PropertyValue
                    {
                        PropertyDef = 12345
                    };
                    propertyValue.TypedValue.SetValue(dataType, this.GetSampleValueForDataType(dataType));
                    propertyValues.Add(-1, propertyValue);
                }
                catch(Exception e)
                {
                    throw new InvalidOperationException
                    (
                        $"Exception whilst creating sample value for data type {dataType}.",
                        e
                    );
                }

                // Create objVerEx instance.
                var objVerEx = new Common.ObjVerEx
                (
                    vaultMock.Object,
                    Mock.Of<ObjectVersion>(ov => ov.ObjVer == new ObjVer()),
                    propertyValues
                );

                // Call method.
                try
                {
                    this.CallGetPropertyMethod(objVerEx, 12345);
                    Assert.Fail($"Exception should have been thrown when testing data type {dataType}.");
                }
                catch (ArgumentException e)
                {
                    Assert.AreEqual("propertyDef", e.ParamName);
                }
                catch (Exception e)
                {
                    Assert.Fail($"Unexpected exception type ({e.GetType().Name}) thrown when testing data type {dataType}: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Gets a sample (valid) value for the given data type.
        /// </summary>
        /// <param name="dataType">The data type to get a value for.</param>
        /// <returns>A valid value.</returns>
        public object GetSampleValueForDataType(MFDataType dataType)
        {
            switch (dataType)
            {
                case MFDataType.MFDatatypeBoolean:
                    return true;
                    case MFDataType.MFDatatypeDate:
                        return DateTime.Now.Date;
                case MFDataType.MFDatatypeTimestamp:
                case MFDataType.MFDatatypeTime:
                        return DateTime.Now;
                case MFDataType.MFDatatypeFloating:
                    return 1.2345;
                case MFDataType.MFDatatypeInteger:
                    return 1234;
                case MFDataType.MFDatatypeInteger64:
                    return 1234567L;
                case MFDataType.MFDatatypeLookup:
                    return new Lookup()
                    {
                        ObjectType = (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument,
                        Item = 123
                    };
                case MFDataType.MFDatatypeMultiLineText:
                    return "hello\r\nworld";
                case MFDataType.MFDatatypeMultiSelectLookup:
                {
                    var lookups = new Lookups
                    {
                        {
                            -1,
                            new Lookup()
                            {
                                ObjectType = ( int )MFBuiltInObjectType.MFBuiltInObjectTypeDocument,
                                Item = 123
                            }
                        },
                        {
                            -1,
                            new Lookup()
                            {
                                ObjectType = ( int )MFBuiltInObjectType.MFBuiltInObjectTypeDocument,
                                Item = 456
                            }
                        }
                    };
                    return lookups;
                }
                case MFDataType.MFDatatypeText:
                    return "hello world";
                default:
                    throw new ArgumentException($"Data type {dataType} not handled", nameof(dataType));
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ValidDataTypesAttribute
        : Attribute
    {
        private List<MFDataType> ValidDataTypes { get; }

        /// <summary>
        /// Gets the specified valid data types.
        /// </summary>
        /// <returns>The valid data types.</returns>
        public List<MFDataType> GetValidDataTypes()
        {
            return this.ValidDataTypes;
        }

        /// <summary>
        /// Gets all data types that are not in the <see cref="GetValidDataTypes"/> list.
        /// </summary>
        /// <returns>All data types, with the valid ones removed.</returns>
        public List<MFDataType> GetInvalidDataTypes()
        {
            // Get all valid data types, excluding the ones specified as valid.
            return Enum
                .GetValues(typeof(MFDataType))
                .Cast<MFDataType>()
                // Filter valid data types.
                .Where(t => false == this.ValidDataTypes.Contains(t))
                // Filter values we can't use anyway.
                .Where(t =>
                    t != MFDataType.MFDatatypeACL
                    && t != MFDataType.MFDatatypeUninitialized
                    && t != MFDataType.MFDatatypeFILETIME)
                .ToList();
        }

        public ValidDataTypesAttribute(params MFDataType[] dataType)
        {
            this.ValidDataTypes = new List<MFDataType>(dataType ?? new MFDataType[0]);
        }
    }
}
