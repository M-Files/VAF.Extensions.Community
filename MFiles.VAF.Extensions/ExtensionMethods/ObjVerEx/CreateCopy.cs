using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFilesAPI;
using MFilesAPI.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MFiles.VAF.Extensions.ObjectCopyOptions;

namespace MFiles.VAF.Extensions
{

	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// Creates a copy of this object in the vault.
		/// </summary>
		/// <param name="source">The source object to copy.</param>
		/// <param name="objectCopyOptions">Options defining how to copy the object.</param>
		/// <returns>The new object.</returns>
		/// <remarks>No attempt is made to roll back anything in case of an exception.
		/// It is recommended that the <paramref name="source"/> is loaded from a transactional vault reference
		/// so that the transaction is rolled back in case of issue.</remarks>
		public static ObjVerEx CreateCopy
		(
			this ObjVerEx source,
			ObjectCopyOptions objectCopyOptions = null
		)
		{
			return source.CreateCopy
			(
				objectCopyOptions ?? new ObjectCopyOptions(),
				new ObjectCopyCreator()
			);
		}

		/// <summary>
		/// Implementation for <see cref="CreateCopy(ObjVerEx, ObjectCopyOptions" />.
		/// Interaction with the vault is via the <paramref name="objectCopyCreator"/>.
		/// </summary>
		/// <param name="source">The source object to copy.</param>
		/// <param name="objectCopyOptions">Options about how to copy the object.</param>
		/// <param name="objectCopyCreator">The instance that will actually create the copy.</param>
		/// <returns></returns>
		internal static ObjVerEx CreateCopy
		(
			this ObjVerEx source,
			ObjectCopyOptions objectCopyOptions = null,
			IObjectCopyCreator objectCopyCreator = null
		)
		{
			// Sanity.
			if (null == source)
				throw new ArgumentNullException(nameof(source));
			objectCopyOptions = objectCopyOptions ?? new ObjectCopyOptions();
			objectCopyCreator = objectCopyCreator ?? new ObjectCopyCreator();

			// Create properties for the new object.
			// If we should copy the source properties then start there,
			// otherwise use an empty collection.
			var propertyValues = new MFPropertyValuesBuilder
			(
				source.Vault,
				objectCopyOptions.CopySourceProperties ? source.Properties.Clone() : new PropertyValues()
			);

			// Remove system properties if requested.
			if (objectCopyOptions.RemoveSystemProperties)
				propertyValues.RemoveSystemProperties();

			// Create an instruction to set SFD to false (will be applied in next section).
			// Will be set accordingly later.
			{
				var instruction = new PropertyValueInstruction()
				{
					InstructionType = PropertyValueInstructionType.RemovePropertyValue
				};
				instruction.PropertyValue.PropertyDef =
					(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject;
				objectCopyOptions.Properties.Add(instruction);
			}

			// Modify the properties as appropriate by running the instructions.
			if (null != objectCopyOptions.Properties)
			{
				foreach (var instruction in objectCopyOptions.Properties)
				{
					if (null == instruction)
						continue;
					instruction.ApplyTo(propertyValues.Values);
				}
			}

			// Are there any properties that are deleted?
			foreach (PropertyValue property in propertyValues.Values.Clone())
			{
				try
				{
					var loadedProperty = source.Vault.PropertyDefOperations.GetPropertyDef(property.PropertyDef);
				}
				catch
				{
					// Property cannot be loaded; it has been deleted.
					propertyValues.Values.RemoveProperty(property.PropertyDef);
				}
			}

			// Create the object, but do not check it in.
			var newObject = new ObjVerEx(source.Vault, objectCopyCreator.CreateObject
			(
				source.Vault,
				objectCopyOptions.TargetObjectType ?? source.ObjVer.Type, 
				propertyValues.Values,
				sourceObjectFiles: null, // We will add these later.
				singleFileDocument: false, // Always false here, until we know how many files we're copying.
				checkIn: false, // Don't check in until the files have been added.
				accessControlList: objectCopyOptions.CopySourceACL ? source.ACL : null
			));

			// Copy the source files to the new object.
			var fileCount = 0;
			if (objectCopyOptions.CopySourceFiles && null != source?.Info?.Files)
			{
				// Note: using .Cast() here throws out our Moq tests, so let's not use it...
				foreach (ObjectFile file in source.Info.Files)
				{
					fileCount++;
					using (var fileStream = file.OpenRead(source.Vault))
					{
						objectCopyCreator.AddFile(newObject, file.Title, file.Extension, fileStream);
					}
				}
			}

			// Add any additional files.
			if (null != objectCopyOptions.AdditionalFiles)
			{
				foreach (SourceObjectFile sourceFile in objectCopyOptions.AdditionalFiles)
				{
					fileCount++;
					objectCopyCreator.AddFile
					(
						newObject,
						sourceFile.Title,
						sourceFile.Extension,
						sourceFile.SourceFilePath
					);
				}
			}

			// If the option is set to false then we'll get whatever the source was
			// (unless it went from a document to something else, in which case it's always false).
			if (objectCopyOptions.SetSingleFileDocumentIfAppropriate)
			{
				// True if a single file.
				objectCopyCreator.SetSingleFileDocument
				(
					newObject, 
					newObject.Type == (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument 
						&& fileCount == 1
				);
			}

			// Check in?
			if (objectCopyOptions.CheckInObject)
				objectCopyCreator.CheckIn
				(
					newObject, 
					objectCopyOptions.CheckInComments ?? "",
					objectCopyOptions.CreatedByUserId ?? -1
				);

			// Return the shiny new object.
			return newObject;
		}
	}
}
