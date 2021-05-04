using MFiles.VAF.Common;
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
			// Sanity.
			if (null == source)
				throw new ArgumentNullException(nameof(source));
			objectCopyOptions = objectCopyOptions ?? new ObjectCopyOptions();

			// Create properties for the new object.
			// If we should copy the source properties then start there,
			// otherwise use an empty collection.
			var propertyValues = new MFPropertyValuesBuilder
			(
				source.Vault,
				objectCopyOptions.CopySourceProperties ? source.Properties.Clone() : new PropertyValues()
			);

			// If it's not a document then SFD = false.
			if ((objectCopyOptions.TargetObjectType ?? source.ObjVer.Type)
				!= (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument)
			{
				// Create an instruction to set SFD to false (will be applied in next section).
				var instruction = new PropertyValueInstruction()
				{
					InstructionType = PropertyValueInstructionType.ReplaceOrAddPropertyValue
				};
				instruction.PropertyValue.PropertyDef =
					(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject;
				instruction.PropertyValue.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
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

			// Create the object, but do not check it in.
			var newObject = new ObjVerEx(source.Vault, source.Vault.ObjectOperations.CreateNewObjectEx
			(
				objectCopyOptions.TargetObjectType ?? source.ObjVer.Type, 
				propertyValues.Values, 
				SourceFiles: null, // We will add these later.
				SFD: false, // Always false here, until we know how many files we're copying.
				CheckIn: false, // Don't check in until the files have been added.
				AccessControlList: objectCopyOptions.CopySourceACL ? source.ACL : null
			));

			// Copy the source files to the new object.
			var fileCount = 0;
			if (objectCopyOptions.CopySourceFiles)
			{
				foreach (var file in source.Info.Files.Cast<ObjectFile>())
				{
					fileCount++;
					using (var fileStream = file.OpenRead(source.Vault))
					{
						newObject.AddFile(file.Title, file.Extension, fileStream);
					}
				}
			}

			// Add any additional files.
			if (null != objectCopyOptions.AdditionalFiles)
			{
				foreach (SourceObjectFile sourceFile in objectCopyOptions.AdditionalFiles)
				{
					newObject.Vault.ObjectFileOperations.AddFile
					(
						newObject.ObjVer,
						sourceFile.Title,
						sourceFile.Extension,
						sourceFile.SourceFilePath
					);
				}
			}

			// Set the single file document property?
			// Can only be done if the object is a document.
			if((objectCopyOptions.TargetObjectType ?? source.ObjVer.Type)
					== (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument)
			{
				// If the option is set to false then we'll get whatever the source was
				// (unless it went from a document to something else, in which case it's always false).
				if (objectCopyOptions.SetSingleFileDocumentIfAppropriate)
				{
					newObject.SaveProperty
					(
						(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject,
						MFDataType.MFDatatypeBoolean,
						fileCount == 1 // True if a single file.
					);
				}
			}

			// Check in?
			if (objectCopyOptions.CheckInObject)
				newObject.CheckIn
				(
					objectCopyOptions.CheckInComments ?? "",
					objectCopyOptions.CreatedByUserId ?? -1
				);

			// Return the shiny new object.
			return newObject;
		}
	}
}
