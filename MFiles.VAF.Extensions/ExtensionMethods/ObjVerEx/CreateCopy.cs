using MFiles.VAF.Common;
using MFilesAPI;
using MFilesAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MFiles.VAF.Extensions.ObjVerExExtensionMethods.ObjectCopyOptions;

namespace MFiles.VAF.Extensions
{
	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// Options to use when creating a copy of an object via <see cref="ObjVerExExtensionMethods.CreateCopy(ObjVerEx, ObjectCopyOptions)"/>.
		/// </summary>
		public class ObjectCopyOptions
		{
			/// <summary>
			/// The type of the object to create, or null to inherit from the source.
			/// </summary>
			public int? TargetObjectType { get; set; } = null;

			/// <summary>
			/// If true, and the resulting object is a document and has a single file,
			/// the SFD property will be set to true (regardless of the setting on the source).
			/// </summary>
			public bool SetSingleFileDocumentIfAppropriate { get; set; } = true;

			/// <summary>
			/// Any comments to use when the object is checked in.
			/// </summary>
			public string CheckInComments { get; set; }

			/// <summary>
			/// The ID of the user that should be shown as creating this object,
			/// or null for no explicit value provided.
			/// </summary>
			public int? CreatedByUserId { get; set; }

			/// <summary>
			/// Whether the object should be checked in as part of the call.
			/// </summary>
			public bool CheckInObject { get; set; } = true;

			/// <summary>
			/// Whether to copy the files from the source to the target object.
			/// </summary>
			public bool CopySourceFiles { get; set; } = true;

			/// <summary>
			/// Whether to copy the properties from the source to the target object.
			/// If false then <see cref="Properties"/> must contain all properties needed
			/// to create the new object.
			/// </summary>
			public bool CopySourceProperties { get; set; } = true;

			/// <summary>
			/// If true then any ACL on the source will be copied to the target.
			/// </summary>
			public bool CopySourceACL { get; set; } = true;

			/// <summary>
			/// Instructions used to alter the source object's properties (e.g. to override a value).
			/// </summary>
			public List<PropertyValueInstruction> Properties { get; set; }
				= new List<PropertyValueInstruction>();

			/// <summary>
			/// Any additional files to add to the target object.
			/// </summary>
			/// <remarks>File names must be unique within objects.
			/// If two files with the same name appear (either within <see cref="AdditionalFiles"/> itself,
			/// or when combined with the source object's files) then an error will be thrown.</remarks>
			public SourceObjectFiles AdditionalFiles { get; set; } = new SourceObjectFiles();

			/// <summary>
			/// Defines an instruction to override the value of a single property value.
			/// </summary>
			public class PropertyValueInstruction
			{
				/// <summary>
				/// The type of instruction.
				/// </summary>
				public PropertyValueInstructionType InstructionType { get; set; }
					= PropertyValueInstructionType.Unknown;

				/// <summary>
				/// The new value (unused if <see cref="InstructionType"/> equals
				/// <see cref="PropertyValueInstructionType.RemoveProperty"/>.
				/// </summary>
				public PropertyValue PropertyValue { get; set; }
					= new PropertyValue();

				/// <summary>
				/// Applies this instruction to the provided <paramref name="propertyValues"/>.
				/// </summary>
				/// <param name="propertyValues">The properties to apply the instruction to.</param>
				public virtual void ApplyTo(PropertyValues propertyValues)
				{
					// Sanity.
					if (null == propertyValues)
						throw new ArgumentNullException(nameof(propertyValues));
					if (null == this.PropertyValue)
						throw new InvalidOperationException("Property value is null; cannot apply it to the new object");

					// Treat each instruction type differently.
					switch (this.InstructionType)
					{
						case PropertyValueInstructionType.AddValueToProperty:
							{
								// Add the provided value onto the end of whatever is there.
								var index = propertyValues.IndexOf(this.PropertyValue.PropertyDef);
								if (index == -1)
									propertyValues.Add(-1, this.PropertyValue); // Not there; add.
								else
								{
									// Get the existing value and ensure that we can handle the types.
									var existingValue = propertyValues[index];
									if (existingValue.TypedValue.DataType != this.PropertyValue.TypedValue.DataType)
										throw new InvalidOperationException($"Data types are not a match (source: {existingValue.TypedValue.DataType}, instruction: {this.PropertyValue.TypedValue.DataType}");

									switch (existingValue.TypedValue.DataType)
									{
										case MFDataType.MFDatatypeMultiSelectLookup:

											// Add each provided value to the end of the lookup.
											foreach (Lookup lookup in this.PropertyValue.TypedValue.GetValueAsLookups())
											{
												// We can't deal with unmanaged references (yet?).
												if (lookup.IsUnmanagedReference())
													throw new NotImplementedException("Cannot use instruction to populate unmanaged lookup reference.");

												existingValue.AddLookup(lookup.Item, lookup.Version);
											}
											break;

										default:
											throw new NotImplementedException($"Cannot use instruction type {PropertyValueInstructionType.AddValueToProperty} with datatype {existingValue.TypedValue.DataType}.");
									}
								}
								break;
							}
						case PropertyValueInstructionType.ReplaceProperty:
							{
								// Add or replace the property value.
								var index = propertyValues.IndexOf(this.PropertyValue.PropertyDef);
								if (index == -1)
									propertyValues.Add(-1, this.PropertyValue); // Not there; add.
								else
									propertyValues[index] = this.PropertyValue; // Overwrite
								break;
							}
						case PropertyValueInstructionType.RemoveProperty:
							{
								// Remove the property value.
								var index = propertyValues.IndexOf(this.PropertyValue.PropertyDef);
								if (index > -1)
									propertyValues.Remove(index);
								break;
							}
						default:
							throw new NotImplementedException($"The instruction type {this.InstructionType} was not handled.");
					}
				}
			}

			/// <summary>
			/// The type of property value instruction.
			/// </summary>
			public enum PropertyValueInstructionType
			{
				/// <summary>
				/// Instruction is unknown.  Will throw an exception if used.
				/// </summary>
				Unknown = 0,

				/// <summary>
				/// Remove the property from the target object entirely.
				/// </summary>
				RemoveProperty = 1,

				/// <summary>
				/// Replace the property value on the target object with
				/// the value contained in the instruction (adding the value if it did not exist, or
				/// overriding any pre-existing value).
				/// </summary>
				ReplaceProperty = 2,

				/// <summary>
				/// Add the source property value to the instruction value.
				/// </summary>
				AddValueToProperty = 3
			}
		}

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
					InstructionType = PropertyValueInstructionType.ReplaceProperty
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
