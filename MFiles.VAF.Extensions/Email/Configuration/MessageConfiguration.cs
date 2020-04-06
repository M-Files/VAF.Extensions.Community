using MFiles.VAF.Configuration;
using MFilesAPI;
using MFilesAPI.Extensions.Email;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.Email
{
	/// <summary>
	/// The message to send.
	/// </summary>
	[DataContract]
	public class MessageConfiguration
	{
		/// <summary>
		/// Who to send the email to.
		/// </summary>
		[DataMember(Order = 0)]
		[JsonConfEditor
		(
			IsRequired = true
		)]
		public virtual List<EmailAddress> To { get; set; } = new List<EmailAddress>();

		/// <summary>
		/// Who to CC the email to.
		/// </summary>
		[DataMember(Order = 1 )]
		[JsonConfEditor
		(
			Label = "CC"
		)]
		public virtual List<EmailAddress> CarbonCopy { get; set; } = new List<EmailAddress>();

		/// <summary>
		/// Who to BCC the email to.
		/// </summary>
		[DataMember(Order = 2 )]
		[JsonConfEditor
		(
			Label = "BCC"
		)]
		public virtual List<EmailAddress> BlindCarbonCopy { get; set; } = new List<EmailAddress>();

		/// <summary>
		/// The email subject.
		/// </summary>
		[DataMember(Order = 3 )]
		[JsonConfEditor
		(
			Label = "Email Subject",
			TypeEditor = "placeholderText",
			IsRequired = true
		)]
		public virtual string Subject { get; set; }
		
		/// <summary>
		/// The plain text version of the message body.
		/// </summary>
		/// <remarks>At least one of <see cref="TextBody"/> or <see cref="HtmlBody"/> should be populated.  If both are populated then a multi-part email will be sent with both representations.</remarks>
		[DataMember(Order = 4 )]
		[JsonConfEditor
		(
			Label = "Email Body (Text)",
			TypeEditor = "placeholderText",
			IsRequired = true
		)]
		public virtual string TextBody { get; set; }
		
		/// <summary>
		/// The HTML version of the message body.
		/// </summary>
		/// <remarks>At least one of <see cref="TextBody"/> or <see cref="HtmlBody"/> should be populated.  If both are populated then a multi-part email will be sent with both representations.</remarks>
		[DataMember(Order = 5 )]
		[JsonConfEditor
		(
			Label = "Email Body (HTML)",
			TypeEditor = "placeholderText"
		)]
		public virtual string HtmlBody { get; set; }

		/// <summary>
		/// Whether to use default sender details.
		/// </summary>
		[DataMember(Order = 6)]
		[JsonConfEditor
		(
			Label = "Use Default Sender",
			DefaultValue = true
		)]
		public virtual bool UseDefaultSender { get; set; } = true;

		/// <summary>
		/// The email address of the sender.
		/// </summary>
		[DataMember(Order = 7 )]
		[JsonConfEditor
		(
			Label = "Sender",
			DefaultValue = (string)null,
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'UseDefaultSender' && .value == false }"
		)]
		public virtual EmailAddress From { get; set; }
		
		/// <summary>
		/// Whether to attach the current object's files to the email.
		/// </summary>
		[DataMember(Order = 8 )]
		[JsonConfEditor
		(
			Label = "Attach Current Object's Files",
			DefaultValue = false
		)]

		public virtual bool AttachCurrentObjectFiles { get;set; } = false;

		/// <summary>
		/// The file format to attach files in.
		/// </summary>
		[DataMember(Order = 9)]
		[JsonConfEditor
		(
			Label = "Attached File Format",
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'AttachCurrentObjectFiles' && .value == true }",
			DefaultValue = MFFileFormat.MFFileFormatNative
		)]
		public virtual MFFileFormat AttachedFileFormat { get; set; } = MFFileFormat.MFFileFormatNative;
	}
}