using MFiles.VAF.Common;
using MFilesAPI;
using MFilesAPI.Extensions.Email;
using System;

namespace MFiles.VAF.Extensions.Email
{
	// ReSharper disable once InconsistentNaming
	public static class IEmailMessageExtensionMethods
	{
		/// <summary>
		/// Adds all files on the given <paramref name="objVerEx"/> to the <paramref name="emailMessage"/>.
		/// </summary>
		/// <param name="emailMessage">The email message to add the files to.</param>
		/// <param name="objVerEx">The object version that has the files.</param>
		/// <param name="fileFormat">The format for the attached files.</param>
		public static void AddAllFiles
		(
			this IEmailMessage emailMessage,
			ObjVerEx objVerEx,
			MFFileFormat fileFormat = MFFileFormat.MFFileFormatNative
		)
		{
			// Sanity.
			if (null == emailMessage)
				throw new ArgumentNullException(nameof(emailMessage));
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if (null == objVerEx.Vault)
				throw new ArgumentException(Resources.Exceptions.ObjVerExExtensionMethods.ObjVerExVaultReferenceNull, nameof(objVerEx));
			if (null == objVerEx.Info)
				throw new ArgumentException(Resources.Exceptions.ObjVerExExtensionMethods.ObjVerExVersionInfoReferenceNull, nameof(objVerEx));

			// Use the base implementation.
			emailMessage.AddAllFiles(objVerEx.Info, objVerEx.Vault, fileFormat);
		}

		/// <summary>
		/// Configures the <paramref name="emailMessage"/> according to the given <paramref name="messageConfiguration"/>.
		/// </summary>
		/// <param name="emailMessage">The email message to configure.</param>
		/// <param name="messageConfiguration">The message configuration to use.</param>
		/// <param name="objectContext">The object to use as context for any placeholder content.</param>
		public static void Configure
		(
			this IEmailMessage emailMessage,
			MessageConfiguration messageConfiguration,
			ObjVerEx objectContext
		)
		{
			// Sanity.
			if (null == emailMessage)
				throw new ArgumentNullException(nameof(emailMessage));
			if (null == messageConfiguration)
				throw new ArgumentNullException(nameof(messageConfiguration));
			if (null == objectContext)
				throw new ArgumentNullException(nameof(objectContext));

			// To/CC/BCC.
			if(null != messageConfiguration.To)
				foreach (var p in messageConfiguration.To)
					emailMessage.AddRecipient(AddressType.To, p);
			if(null != messageConfiguration.CarbonCopy)
				foreach (var p in messageConfiguration.CarbonCopy)
					emailMessage.AddRecipient(AddressType.CarbonCopy, p);
			if(null != messageConfiguration.BlindCarbonCopy)
				foreach (var p in messageConfiguration.BlindCarbonCopy)
					emailMessage.AddRecipient(AddressType.BlindCarbonCopy, p);

			// Subject/email bodies.
			if (null != messageConfiguration.Subject)
				emailMessage.Subject = objectContext.ExpandPlaceholderText(messageConfiguration.Subject);
			if (null != messageConfiguration.TextBody)
				emailMessage.TextBody = objectContext.ExpandPlaceholderText(messageConfiguration.TextBody);
			if (null != messageConfiguration.HtmlBody)
				emailMessage.HtmlBody = objectContext.ExpandPlaceholderText(messageConfiguration.HtmlBody);

			// Override sender details?
			if (false == messageConfiguration.UseDefaultSender)
				emailMessage.SetSender(messageConfiguration.From);

			// Attach files?
			if (messageConfiguration.AttachCurrentObjectFiles)
				emailMessage.AddAllFiles(objectContext, messageConfiguration.AttachedFileFormat);
		}
	}
}
