using MFiles.VAF.Common;
using MFilesAPI;
using System;
using System.IO;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// Internal interace used for copying objects.
	/// Primarily used for unit testing to avoid needing a vault reference.
	/// </summary>
	internal interface IObjectCopyCreator
	{
		ObjectVersionAndProperties CreateObject
		(
			Vault vault,
			int objectType,
			PropertyValues propertyValues,
			SourceObjectFiles sourceObjectFiles,
			bool singleFileDocument,
			bool checkIn,
			AccessControlList accessControlList
		);

		void AddFile
		(
			ObjVerEx addTo,
			string title,
			string extension,
			System.IO.Stream content
		);

		void AddFile
		(
			ObjVerEx addTo,
			string title,
			string extension,
			string localFilePath
		);

		void SetSingleFileDocument
		(
			ObjVerEx objVerEx,
			bool value
		);

		void CheckIn
		(
			ObjVerEx objVerEx,
			string comments,
			int userId
		);
	}

	/// <summary>
	/// The default implementation of <see cref="IObjectCopyCreator"/>
	/// that uses the M-Files API.
	/// </summary>
	internal class ObjectCopyCreator
		: IObjectCopyCreator
	{
		public void AddFile(ObjVerEx addTo, string title, string extension, Stream content)
		{
			(addTo ?? throw new ArgumentNullException(nameof(addTo))).AddFile(title, extension, content);
		}

		public void AddFile(ObjVerEx addTo, string title, string extension, string localFilePath)
		{

			(addTo ?? throw new ArgumentNullException(nameof(addTo)))
				.Vault
				.ObjectFileOperations
				.AddFile
				(
					addTo.ObjVer,
					title,
					extension,
					localFilePath
				);
		}

		public void CheckIn(ObjVerEx objVerEx, string comments, int userId)
		{
			(objVerEx ?? throw new ArgumentNullException(nameof(objVerEx)))
				.CheckIn
				(
					comments,
					userId
				);
		}

		public ObjectVersionAndProperties CreateObject(Vault vault, int objectType, PropertyValues propertyValues, SourceObjectFiles sourceObjectFiles, bool singleFileDocument, bool checkIn, AccessControlList accessControlList)
		{
			return (vault ?? throw new ArgumentNullException(nameof(vault)))
				.ObjectOperations
				.CreateNewObjectEx
				(
					objectType,
					propertyValues,
					SourceFiles: sourceObjectFiles,
					SFD: singleFileDocument,
					CheckIn: checkIn,
					AccessControlList: accessControlList
				);
		}

		public void SetSingleFileDocument(ObjVerEx objVerEx, bool value)
		{
			(objVerEx ?? throw new ArgumentNullException(nameof(objVerEx)))
				.SaveProperty
				(
					(int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject,
					MFDataType.MFDatatypeBoolean,
					value
				);
		}
	}
}
