using System.Runtime.Serialization;
using MFiles.VAF.Configuration;

namespace MFiles.VAF.Extensions.Email
{
	/// <summary>
	/// Configuration settings for authentication to the Smtp server.
	/// </summary>
	[DataContract]
	public class Credentials
		: MFilesAPI.Extensions.Email.Credentials
	{
		/// <summary>
		/// The account name to connect as.
		/// </summary>
		[DataMember(Order = 0 )]
		[JsonConfEditor(Label = "Username")]
		public override string AccountName
		{
			get => base.AccountName;
			set => base.AccountName = value;
		}
		
		/// <summary>
		/// The password for the account.
		/// </summary>
		[DataMember(Order = 1 )]
		[JsonConfEditor(Label = "Password")]
		[Security(IsPassword = true)]
		public override string Password
		{
			get => base.Password;
			set => base.Password = value;
		}
	}
}