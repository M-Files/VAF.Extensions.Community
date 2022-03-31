using MFiles.VAF.AdminConfigurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public static class FileUploadExtensionMethods
	{
		/// <summary>
		/// Returns <paramref name="fileUpload"/>'s <see cref="FileUpload.Content"/> as a byte array.
		/// </summary>
		/// <param name="fileUpload">The file upload whose contents to retrieve.</param>
		/// <param name="stringEncoding">The expected file content encoding for <see cref="FileUpload.ContentType.PlainText"/>, or <see cref="Encoding.UTF8"/> if none is provided.</param>
		/// <returns>The file contents as a byte array.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="fileUpload"/> is <see langword="null"/>.</exception>
		public static byte[] GetFileContentAsBytes
		(
			this FileUpload fileUpload,
			Encoding stringEncoding = null
		)
		{
			// Sanity.
			if (null == fileUpload)
				throw new ArgumentNullException(nameof(fileUpload));
			if (string.IsNullOrWhiteSpace(fileUpload.Content))
				return new byte[0];
			stringEncoding = stringEncoding ?? Encoding.UTF8;

			// Use the convert.FromBase64String implementation.
			switch(fileUpload.Type)
			{
				case FileUpload.ContentType.Base64Encoded:
					return Convert.FromBase64String(fileUpload.Content);
				case FileUpload.ContentType.PlainText:
					return stringEncoding.GetBytes(fileUpload.Content);
				default:
					throw new NotImplementedException($"File content type {fileUpload.Type} not handled by {nameof(GetFileContentAsBytes)}.");

			}
		}

		/// <summary>
		/// Writes the contents of <paramref name="fileUpload"/> to <paramref name="output"/>.
		/// </summary>
		/// <param name="fileUpload">The file upload to save.</param>
		/// <param name="output">The location to save the file data.</param>
		/// <param name="stringEncoding">The expected file content encoding for <see cref="FileUpload.ContentType.PlainText"/>, or <see cref="Encoding.UTF8"/> if none is provided.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="fileUpload"/> or <paramref name="output"/> are null.</exception>
		public static void SaveFileContents
		(
			this FileUpload fileUpload,
			FileInfo output,
			Encoding stringEncoding = null
		)
		{
			// Sanity.
			if (null == fileUpload)
				throw new ArgumentNullException(nameof(fileUpload));
			if (null == output)
				throw new ArgumentNullException(nameof(output));

			// Remove any file that we need to overwrite.
			if (output.Exists)
			{
				output.Delete();
				output.Refresh();
			}

			// Write the data.
			using (var writer = output.OpenWrite())
			{
				var data = fileUpload.GetFileContentAsBytes(stringEncoding);
				writer.Write(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Creates a file name based on <paramref name="folderName"/> and <paramref name="fileName"/>,
		/// and writes the content of <paramref name="fileUpload"/> there.
		/// </summary>
		/// <param name="fileUpload">The file upload to save.</param>
		/// <param name="folderName">The folder to save the file to.</param>
		/// <param name="fileName">The file name to use, or <see cref="FileUpload.FileName"/>, if the argument is not provided, empty, or null.</param>
		/// <param name="stringEncoding">The expected file content encoding for <see cref="FileUpload.ContentType.PlainText"/>, or <see cref="Encoding.UTF8"/> if none is provided.</param>
		/// <exception cref="ArgumentException">If both <paramref name="fileName"/> and <see cref="FileUpload.FileName"/> are whitespace or empty.</exception>
		public static void SaveFileContents
		(
			this FileUpload fileUpload,
			string folderName,
			string fileName = null,
			Encoding stringEncoding = null
		)
		{
			// Sanity.
			if (string.IsNullOrEmpty(fileName))
				fileName = fileUpload.FileName;
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentException("The file upload does not contain a file name, so one must be provided.");

			// Use the other overload.
			fileUpload.SaveFileContents
			(
				new FileInfo(Path.Combine(folderName, fileName)), 
				stringEncoding
			);
		}
	}
}
