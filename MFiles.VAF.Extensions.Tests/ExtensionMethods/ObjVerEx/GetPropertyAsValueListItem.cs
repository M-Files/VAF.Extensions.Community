using MFiles.VAF.Configuration;

using MFilesAPI;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;

namespace MFiles.VAF.Extensions.Tests.ExtensionMethods.ObjVerEx
{
    [TestClass]
    public class GetPropertyAsValueListItem
        : TestBaseWithVaultMock
    {

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsIfNullObjVerEx()
        {
            ((Common.ObjVerEx) null).GetPropertyAsValueListItem((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefWorkflow);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowsIfPropDefIdNegative()
        {
            ((Common.ObjVerEx) null).GetPropertyAsValueListItem((int) -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsIfPropDoesNotExist()
        {
            // max int should not point to an existing property definition
            ((Common.ObjVerEx) null).GetPropertyAsValueListItem(int.MaxValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsIfPropIsNotBasedOnValueList()
        {
            // max int should not point to an existing property definition
            ((Common.ObjVerEx) null).GetPropertyAsValueListItem((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefDeleted);
        }

        // TODO Think about further test methods + Question how lookups to object types were handled

    }
}
