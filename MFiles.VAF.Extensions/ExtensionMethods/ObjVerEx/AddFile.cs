using System;
using System.IO;
using MFiles.VAF.Common;
using MFilesAPI;

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
	}
}
