using System;
using System.IO;
using MFiles.VAF.Common;
using MFilesAPI;
using MFilesAPI.Extensions;

namespace MFiles.VAF.Extensions
{
	public static partial class VaultObjectFileOperationsExtensionMethods
	{
		/// <summary>
		/// Adds a new file to the specified object.
		/// </summary>
		/// <param name="objectFileOperations">The instance of <see cref="VaultObjectFileOperations"/> to use.</param>
		/// <param name="objVerEx">The object version to add the file to.  Must already be checked out.</param>
		/// <param name="title">The title of the file (without an extension).</param>
		/// <param name="extension">The file extension.  Can be supplied with or without preceeding ".".</param>
		/// <param name="fileContents">The contents of the file.</param>
		public static void AddFile
		(
			this VaultObjectFileOperations objectFileOperations,
			ObjVerEx objVerEx,
			string title,
			string extension,
			Stream fileContents
		)
		{
			// Sanity.
			if (null == objectFileOperations)
				throw new ArgumentNullException(nameof(objectFileOperations));
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if (null == objVerEx.Vault)
				throw new ArgumentException("The ObjVerEx does not have a valid vault connection.", nameof(objVerEx));
			if (String.IsNullOrWhiteSpace(title))
				throw new ArgumentException("The file must have a title/name.", nameof(title));
			if (null == fileContents)
				throw new ArgumentNullException(nameof(fileContents));

			// Use the other extension method.
			objectFileOperations.AddFile
			(
				objVerEx.ObjVer,
				objVerEx.Vault,
				title,
				extension,
				fileContents
			);
		}

	}
}
