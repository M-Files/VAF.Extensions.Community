using MFiles.VAF.Configuration;

using MFilesAPI;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
    [TestClass]
    public class GetPropertyAsValueListItems
        : TestBaseWithVaultMock
    {

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsIfNullObjVerEx()
        {
            ((Common.ObjVerEx) null).GetPropertyAsValueListItems((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefWorkflow);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowsIfPropDefIdNegative()
        {
            ((Common.ObjVerEx) null).GetPropertyAsValueListItems((int) -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsIfPropDoesNotExist()
        {
            // max int should not point to an existing property definition
            ((Common.ObjVerEx) null).GetPropertyAsValueListItems(int.MaxValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsIfPropIsNotBasedOnValueList()
        {
            ((Common.ObjVerEx) null).GetPropertyAsValueListItems((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefDeleted);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsIfPropIsNoMultiSelectLookup()
        {
            ((Common.ObjVerEx) null).GetPropertyAsValueListItems((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefWorkflow);
        }

        // TODO Think about further test methods + Question how lookups to object types were handled

    }
}
