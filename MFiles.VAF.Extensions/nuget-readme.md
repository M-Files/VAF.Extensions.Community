# M-Files Vault Application Framework Extensions library

*Please note that this library is provided "as-is" and with no warranty, explicit or otherwise. You should ensure that the functionality meets your requirements, and thoroughly test them, prior to using in any production scenarios.*

The following helper library is a community-driven set of functionality that extends the base [M-Files Vault Application Framework](https://developer.m-files.com/Frameworks/Vault-Application-Framework/). This library is [open-source](https://github.com/M-Files/VAF.Extensions.Community) and not directly supported by M-Files. Contributions are accepted according to our contribution guide.

## Using the library

 1. Install the latest production release from nuget.
 2. Update your `VaultApplication.cs` file, ensuring that the base class for your vault application is changed to `MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<T>`.  A more complete example of an empty vault application class is shown below.

```
public class VaultApplication
	: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
{

}
```

## Naming formats

Releases follow a naming format based upon the M-Files versioning; releases are named using a combination of the year and month they are released in and an incrementing build number.  Releases may also optionally contain a suffix (starting with a hyphen) denoting that the release is a preview release and should not be used in production environments.

 * `22.6.123` - this full release was made in June 2022.
 * `22.6.140` - this full release was also made in June 2022, but is newer than the one above.
 * `22.7.141-preview` - this release was made in July 2022 from the "preview" branch.  Releases from the preview branch are often close to release quality, but should only be used for testing.
 * `22.7.0.13-test-feature-1` - this release was made in July 2022 from a specific feateure branch.  This release will contain in-development functionality and should only be used when needing to test the specific feature being developed.  Significant breaking changes may still be made when this functionality progresses to preview or release builds.

 Any problems can be logged as [issues against the repository](https://github.com/M-Files/VAF.Extensions.Community/issues), or discussed on the [M-Files Community](https://community.m-files.com).
