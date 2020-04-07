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
		/// Adds a new file to the specified object.
		/// </summary>
		/// <param name="objVerEx">The object version to add the file to.  Must already be checked out.</param>
		/// <param name="title">The title of the file (without an extension).</param>
		/// <param name="extension">The file extension.  Can be supplied with or without preceeding ".".</param>
		/// <param name="fileContents">The contents of the file.</param>
		public static void AddFile
		(
			this ObjVerEx objVerEx,
			string title,
			string extension,
			Stream fileContents

		)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if (null == objVerEx.Vault?.ObjectFileOperations)
				throw new ArgumentException("The ObjVerEx does not have a valid vault connection.", nameof(objVerEx));
			if (String.IsNullOrWhiteSpace(title))
				throw new ArgumentException("The file must have a title/name.", nameof(title));
			if (null == fileContents)
				throw new ArgumentNullException(nameof(fileContents));

			// Use the other extension method.
			objVerEx.Vault.ObjectFileOperations.AddFile
			(
				objVerEx,
				title,
				extension,
				fileContents
			);
		}

		/// <summary>
		/// Replaces the contents of an existing <paramref name="objectFile"/> with data from <paramref name="fileContents"/>.
		/// </summary>
		/// <param name="objVerEx">The object version that contains the file.  Must already be checked out.</param>
		/// <param name="objectFile">The file to update.</param>
		/// <param name="fileContents">The contents of the file.</param>
		/// <param name="blockSize">The block size to use for transfers.</param>
		public static void ReplaceFileContent
		(
			this ObjVerEx objVerEx,
			ObjectFile objectFile,
			Stream fileContents,
			int blockSize = FileTransfers.DefaultBlockSize
		)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if (null == objectFile)
				throw new ArgumentNullException(nameof(objectFile));
			if (null == fileContents)
				throw new ArgumentNullException(nameof(fileContents));

			// Use the other extension method.
			objectFile.ReplaceFileContent
			(
				objVerEx.Vault,
				fileContents,
				blockSize
			);
		}

		/// <summary>
		/// Replaces the contents of an existing object with exactly one file, with data from <paramref name="fileContents"/>.
		/// </summary>
		/// <param name="objVerEx">The object version that contains exactly one file.  Must already be checked out.</param>
		/// <param name="fileContents">The contents of the file.</param>
		/// <param name="blockSize">The block size to use for transfers.</param>
		public static void ReplaceFileContent
		(
			this ObjVerEx objVerEx,
			Stream fileContents,
			int blockSize = FileTransfers.DefaultBlockSize
		)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if (null == objVerEx.Info?.Files)
				throw new ArgumentException($"The object version does not contain information about its files.", nameof(objVerEx));
			if (null == fileContents)
				throw new ArgumentNullException(nameof(fileContents));

			// Does it have exactly one file?
			if (1 != objVerEx.Info.FilesCount)
				throw new ArgumentException($"The object version does not contain exactly one file.", nameof(objVerEx));

			// Use the other extension method.
			objVerEx.ReplaceFileContent
			(
				objVerEx.Info.Files[1],
				fileContents,
				blockSize
			);
		}
	}
}
