# Email Extensions

*Note: This is a very early version of a potential addition to the above library.  This content is only valid for the versions being provided here and may not be applicable in the future.*

This builds upon the [COM API Extensions](https://github.com/M-Files/COMAPI.Extensions.Community/tree/Email/MFilesAPI.Extensions/Email) functionality, and the readme there should be read before this one.

## Configuration

The VAF Community Extensions project provides an implementation of `MFilesAPI.Extensions.Email.Configuration.SmtpConfiguration` that has been decorated with the attributes required for use in VAF 2.1 configuration areas.

### Adding to a Vault Application Framework Configuration file

The VAF extensions library contains a class definition that can be simply added to an existing VAF 2.1 configuration class to expose the required SMTP configuration elements:

```csharp
[DataContract]
public class Configuration
{
	[DataMember]
	public MFiles.VAF.Extensions.Email.VAFSmtpConfiguration SmtpConfiguration { get; set; }
		= new MFiles.VAF.Extensions.Email.VAFSmtpConfiguration();
}
```
*Note: After installing your application you must go and configure your SMTP settings.  These cannot be read from M-Files Server Notification details, unfortunately.**

## Sending an email

The VAF extensions library adds an extension method for `ObjVerEx.AddAllFiles`, allowing files to be easily attached from an existing `ObjVerEx` instance:

```csharp
[StateAction("WFS.test.SendEmail")]
public void SendEmailWorkflowHandler(StateEnvironment env)
{
	// Create a message.
	using (var emailMessage = new EmailMessage(this.Configuration.SmtpConfiguration))
	{
		// To.
		emailMessage.AddRecipient(AddressType.To, "craig.hawker@m-files.com");

		// Configure the message metadata.
		emailMessage.Subject = "hello world";
		emailMessage.HtmlBody = $"This is a <b>HTML</b> for document {env.ObjVerEx.Title}.";

		// Add all files from the current object.
		emailMessage.AddAllFiles(env.ObjVerEx, MFFileFormat.MFFileFormatPDF);
		
		// Send the message.
		emailMessage.Send();
	}
}

```