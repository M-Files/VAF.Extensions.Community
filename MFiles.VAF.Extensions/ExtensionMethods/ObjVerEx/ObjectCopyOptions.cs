using MFiles.VAF.Common;
using MFilesAPI;
using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions
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
		/// Whether to copy the files from the source to the new object.
		/// </summary>
		public bool CopySourceFiles { get; set; } = true;

		/// <summary>
		/// Whether to copy the properties from the source to the new object.
		/// If false then <see cref="Properties"/> must contain all properties needed
		/// to create the new object.
		/// </summary>
		public bool CopySourceProperties { get; set; } = true;

		/// <summary>
		/// If true then any ACL on the source will be copied to the new object.
		/// </summary>
		public bool CopySourceACL { get; set; } = true;

		/// <summary>
		/// If true then will remove system properties (see: <see cref="MFPropertyValuesBuilder.RemoveSystemProperties"/>)
		/// from the new object before creating.
		/// </summary>
		public bool RemoveSystemProperties { get; set; } = true;

		/// <summary>
		/// Instructions used to alter the source object's properties (e.g. to override a value).
		/// </summary>
		public List<PropertyValueInstruction> Properties { get; set; }
			= new List<PropertyValueInstruction>();

		/// <summary>
		/// Any additional files to add to the new object.
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
			public PropertyValueInstruction()
			{
			}
			public PropertyValueInstruction
			(
				PropertyValueInstructionType instructionType,
				int propertyDefId,
				MFDataType dataType,
				object value
			)
			{
				this.InstructionType = instructionType;
				var pv = new PropertyValue()
				{
					PropertyDef = propertyDefId
				};
				pv.TypedValue.SetValue(dataType, value);
				this.PropertyValue = pv;
			}
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
					throw new InvalidOperationException(Resources.Exceptions.ObjVerExExtensionMethods.CreateCopy_ObjectCopyOptions_PropertyValueNull);

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
									throw new InvalidOperationException
									(
										String.Format
										(
											Resources.Exceptions.ObjVerExExtensionMethods.CreateCopy_ObjectCopyOptions_InvalidDataType,
											existingValue.TypedValue.DataType,
											this.PropertyValue.TypedValue.DataType
										)
									);

								switch (existingValue.TypedValue.DataType)
								{
									case MFDataType.MFDatatypeMultiSelectLookup:

										// Add each provided value to the end of the lookup.
										foreach (Lookup lookup in this.PropertyValue.TypedValue.GetValueAsLookups())
										{
											// We can't deal with unmanaged references (yet?).
											if (lookup.IsUnmanagedReference())
												throw new NotImplementedException(Resources.Exceptions.ObjVerExExtensionMethods.CreateCopy_ObjectCopyOptions_UnmanagedReferences);

											existingValue.AddLookup(lookup.Item, lookup.Version);
										}
										break;

									default:
										throw new NotImplementedException
										(
											string.Format
											(
												Resources.Exceptions.ObjVerExExtensionMethods.CreateCopy_ObjectCopyOptions_UnsupportedDataType,
												PropertyValueInstructionType.AddValueToProperty,
												existingValue.TypedValue.DataType
											)
										);
								}
							}
							break;
						}
					case PropertyValueInstructionType.ReplaceOrAddPropertyValue:
						{
							// Add or replace the property value.
							var index = propertyValues.IndexOf(this.PropertyValue.PropertyDef);
							if (index == -1)
								propertyValues.Add(-1, this.PropertyValue); // Not there; add.
							else
								propertyValues[index] = this.PropertyValue; // Overwrite
							break;
						}
					case PropertyValueInstructionType.RemovePropertyValue:
						{
							// Remove the property value.
							var index = propertyValues.IndexOf(this.PropertyValue.PropertyDef);
							if (index > -1)
								propertyValues.Remove(index);
							break;
						}
					default:
						throw new NotImplementedException
						(
							string.Format
							(
								Resources.Exceptions.ObjVerExExtensionMethods.CreateCopy_ObjectCopyOptions_UnhandledInstructionType, 
								this.InstructionType
							)
						);
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
			RemovePropertyValue = 1,

			/// <summary>
			/// Replace the property value on the target object with
			/// the value contained in the instruction (adding the value if it did not exist, or
			/// overriding any pre-existing value).
			/// </summary>
			ReplaceOrAddPropertyValue = 2,

			/// <summary>
			/// Add the source property value to the instruction value.  If the property value
			/// already exists then the provided value will be added to the end of the lookup.
			/// </summary>
			/// <remarks>Can currently only be used with <see cref="MFDataType.MFDatatypeMultiSelectLookup"/> properties.</remarks>
			AddValueToProperty = 3
		}
	}
}
