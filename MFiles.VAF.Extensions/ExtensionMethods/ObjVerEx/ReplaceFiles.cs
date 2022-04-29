using System;
using System.IO;
using MFiles.VAF.Common;
using MFilesAPI;
using MFilesAPI.Extensions;

namespace MFiles.VAF.Extensions
{
	public static partial class ObjVerExExtensionMethods
	{

		/// <summary>
		/// Method will at first delete the current files from the object and the adds the sourceFiles as new files.
		/// If allowChangingSingleFilePropertyValue is true, then the Single File property is set based on the sourceFiles file count.
		/// If allowChangingSingleFilePropertyValue is false, then the Single File property is kept the same and an error is thrown if file count differs.
		/// If allowChangingSingleFilePropertyValue is false, then the ReplaceFiles method from VAF is used.
		/// </summary>
		/// <param name="obj">Object, which files should be replaced</param>
		/// <param name="sourceFiles">SourceObjectFiles collection which should be added to the object</param>
		/// <param name="allowChangingSingleFilePropertyValue">Determines can the SingleFile value be changed or not</param>
		public static void ReplaceFiles(this ObjVerEx obj, SourceObjectFiles sourceFiles, bool allowChangingSingleFilePropertyValue)
		{
			if (!obj.Info.ObjectCheckedOut)
				throw new Exception("Object needs to be checked out that the files could be replaced");

			// In case we don't want to change the single file value, then use the VAF's ReplaceFiles method
			if (!allowChangingSingleFilePropertyValue)
			{
				// Use the original VAF extension
				obj.ReplaceFiles(sourceFiles);
			}
			else
			{
				// First set the SingleFile property to false that we can remove every file from the object
				PropertyValue singleFilePropertyValue = new PropertyValue();
				singleFilePropertyValue.PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject;
				singleFilePropertyValue.Value.Value = false;
				obj.Vault.ObjectPropertyOperations.SetProperty(obj.ObjVer, singleFilePropertyValue);

				// Remove every file from the object
				foreach (ObjectFile objectFile in obj.Info.Files)
				{
					obj.Vault.ObjectFileOperations.RemoveFile(obj.ObjVer, objectFile.FileVer);
				}

				// Add new files
				foreach (SourceObjectFile sourceFile in sourceFiles)
				{
					obj.Vault.ObjectFileOperations.AddFile(obj.ObjVer, sourceFile.Title, sourceFile.Extension, sourceFile.SourceFilePath);
				}

				// Modify the SingleFile property value according to the source file count and object type
				bool isSingleFileObject = IsSingleFileObject(sourceFiles, obj.Type);
				singleFilePropertyValue.Value.Value = isSingleFileObject;
				obj.Vault.ObjectPropertyOperations.SetProperty(obj.ObjVer, singleFilePropertyValue);

				// Update object that it contains new files
				obj.Refresh();

				// Update the extension in case the object is single file and the extension differs.
				if (isSingleFileObject && obj.Info.Files[1].Extension != sourceFiles[1].Extension)
					obj.Vault.ObjectFileOperations.RenameFile(obj.ObjVer, obj.Info.Files[1].FileVer, sourceFiles[1].Title, sourceFiles[1].Extension);
			}
		}

		/// <summary>
		/// Checks what should the single file property value be based on the file count and object type
		/// In case the object type is other than Document, then the object is always multi file object.
		/// In case the object type is Document, then the single file property is set based on the sourceObjectFiles file count.
		/// </summary>
		/// <param name="sourceObjectFiles"></param>
		/// <param name="objectTypeID"></param>
		/// <returns></returns>
		private static bool IsSingleFileObject(SourceObjectFiles sourceObjectFiles, int objectTypeID)
		{
			bool isSingleFileObject = false;

			// First set the singleFile value based on the file count.
			// If the file count is 1 then the singleFile value is true.
			// In other cases the value is false meaning the object should be multifile object.
			isSingleFileObject = ((sourceObjectFiles != null && sourceObjectFiles.Count == 1) ? true : false);

			// If the object type is other than Document, then the object is always multifile.
			if (objectTypeID != (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument)
			{
				isSingleFileObject = false;
			}
			return isSingleFileObject;
		}
	}
}
