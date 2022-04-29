using System.Collections.Generic;
using System.Runtime.Serialization;
using MFiles.VAF.Configuration;
using MFilesAPI;
using MFilesAPI.Extensions.Email;

namespace MFiles.VAF.Extensions.Email
{
	[DataContract]
	public class VAFSmtpConfiguration
		: SmtpConfiguration
	{
		/// <inheritdoc />
		[DataMember(Order = 0 )]
		[JsonConfEditor(Label = "Default Sender")]
		public override EmailAddress DefaultSender
		{
			get => base.DefaultSender;
			set => base.DefaultSender = value;
		}

		/// <inheritdoc />
		[DataMember(Order = 1 )]
		[JsonConfEditor
		(
			Label = "Use Local Pickup Folder", 
			HelpText = "If true, system will not attempt to send the email using the SMTP server defined and will instead write emails to a folder on disk for a separate application to then process.  This can improve performance significantly.",
			DefaultValue = false
		)]
		public override bool UseLocalPickupFolder
		{
			get => base.UseLocalPickupFolder;
			set => base.UseLocalPickupFolder = value;
		}
		
		/// <inheritdoc />
		[DataMember(Order = 2 )]
		[JsonConfEditor
		(
			Label = "Pickup Folder", 
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'UseLocalPickupFolder' && .value == true }"
		)]
		public override string LocalPickupFolder
		{
			get => base.LocalPickupFolder;
			set => base.LocalPickupFolder = value;
		}

		/// <inheritdoc />
		[DataMember(Order = 3 )]
		[JsonConfEditor
		(
			Label = "Server Address / Host", 
			Hidden = false,
			HideWhen = ".parent._children{.key == 'UseLocalPickupFolder' && .value == true }"
		)]
		public override string ServerAddress
		{
			get => base.ServerAddress;
			set => base.ServerAddress = value;
		}
		
		/// <inheritdoc />
		[DataMember(Order = 4 )]
		[JsonConfIntegerEditor
		(
			Label = "Server Port",
			HelpText = "Typically 25, 465 or 587", 
			Hidden = false,
			HideWhen = ".parent._children{.key == 'UseLocalPickupFolder' && .value == true }",
			DefaultValue = SmtpConfiguration.DefaultPort
		)]
		public override int Port
		{
			get => base.Port;
			set => base.Port = value;
		}
		
		/// <inheritdoc />
		[DataMember(Order = 5 )]
		[JsonConfEditor
		(
			Label = "Use encrypted connection (SSL/TLS)", 
			Hidden = false,
			HideWhen = ".parent._children{.key == 'UseLocalPickupFolder'&& .value == true }",
			DefaultValue = true
		)]
		public override bool UseEncryptedConnection
		{
			get => base.UseEncryptedConnection;
			set => base.UseEncryptedConnection = value;
		}
		
		/// <inheritdoc />
		[DataMember(Order = 6 )]
		[JsonConfEditor
		(
			Label = "SMTP server requires authentication",
			Hidden = false,
			HideWhen = ".parent._children{.key == 'UseLocalPickupFolder' && .value == true }",
			DefaultValue = true
		)]
		public override bool RequiresAuthentication
		{
			get => base.RequiresAuthentication;
			set => base.RequiresAuthentication = value;
		}
		
		/// <summary>
		/// The authentication credentials to use if <see cref="RequiresAuthentication"/> is true.
		/// </summary>
		[DataMember(Order = 7 )]
		[JsonConfEditor
		(
			Label = "Authentication Credentials", 
			Hidden = true,
			ShowWhen = ".parent._children{.key == 'RequiresAuthentication' && .value == true }"
		)]
		public new Credentials Credentials
		{
			get
			{
				// If we have the wrong type then fix it.
				if (false == base.Credentials is Credentials)
				{
					var credentials = new Credentials()
					{
						AccountName = base.Credentials?.AccountName,
						Password = base.Credentials?.Password
					};
					base.Credentials = credentials;
				}
				return (Credentials) base.Credentials;
			}
			set => base.Credentials = value;
		}

		/// <summary>
		/// The default email headers.
		/// </summary>
		[DataMember(Order = 8)]
		[JsonConfEditor
		(
			Label = "Default email headers",
			HelpText = "If your mail system requires specific headers to be added to all outgoing emails then configure those here."
		)]
		public new List<EmailHeader> DefaultEmailHeader
		{
			get => base.DefaultEmailHeader;
			set => base.DefaultEmailHeader = value;
		}

		/// <summary>
		/// Performs validation of the configuration for visualisation into the admin editor.
		/// </summary>
		/// <param name="vault">The vault to use for validation, if appropriate.</param>
		/// <returns>Any validation findings.</returns>
		public IEnumerable<ValidationFinding> Validate(Vault vault)
		{
			yield break;
		}
	}
}
