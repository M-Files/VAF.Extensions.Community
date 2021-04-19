![GitHub](https://img.shields.io/github/license/M-Files/VAF.Extensions.Community) ![GitHub last commit](https://img.shields.io/github/last-commit/M-Files/VAF.Extensions.Community)

[![Nuget version](https://img.shields.io/nuget/v/MFiles.VAF.Extensions?label=nuget%20version)](https://www.nuget.org/packages/MFiles.VAF.Extensions/) ![Nuget](https://img.shields.io/nuget/dt/MFiles.VAF.Extensions?label=nuget%20downloads)

![GitHub issues](https://img.shields.io/github/issues-raw/M-Files/VAF.Extensions.Community) ![GitHub pull requests](https://img.shields.io/github/issues-pr-raw/M-Files/VAF.Extensions.Community) ![GitHub repo size](https://img.shields.io/github/repo-size/M-Files/VAF.Extensions.Community) 

# M-Files Vault Application Framework Extensions (Community)

_Please note that this library is provided "as-is" and with no warranty, explicit or otherwise. You should ensure that the functionality meets your requirements, and thoroughly test them, prior to using in any production scenarios._

The following helper library is a community-driven set of functionality that extends the base M-Files Vault Application Framework.  This library is open-source and not directly supported by M-Files.  Contributions are accepted according to our [contribution guide](CONTRIBUTING.md).

**NOTE THAT EXAMPLES IN THIS REPOSITORY SHOULD BE CORRECT FOR THE CURRENT BRANCH; IF YOU ARE IN THE MAIN (UNRELEASED) BRANCH THEN THE SAMPLES MAY NOT WORK ON THE CURRENT PUBLIC RELEASE - MAKE SURE THAT YOU SWITCH TO THE 'RELEASE' BRANCH!**

## Nuget package

This library is available [via nuget](https://www.nuget.org/packages/MFiles.VAF.Extensions/).  The simplest way to get started with this library is to add the `MFiles.VAF.Extensions` package to your existing [Vault Application Framework](https://developer.m-files.com/Frameworks/Vault-Application-Framework/) project.  This library requires the use of the Vault Application Framework 2.1 or higher.

## Using the library

The steps required will depend upon which functionality you wish to take into use, but the basic steps include:

1. Open the project to be updated using Visual Studio 2017 or Visual Studio 2019.
2. Right-click on the project in the `Solution Explorer` and select `Manage NuGet packages...`.
3. Ensure that the `Browse` tab is selected and enter `M-Files VAF Extensions` into the search box. 
4. Add a reference to the latest public build of the [VAF Extensions library](https://www.nuget.org/packages/MFiles.VAF.Extensions/).  Note that you should avoid pre-releases (any version that ends in `-something`) in production code.
5. Open your `VaultApplication.cs` file.
6. Change the base class from `MFiles.VAF.ConfigurableVaultApplicationBase<T>` to `MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<T>`.
7. You may also need to add a `using MFiles.VAF.Extensions;` statement to the top of files that wish to use extension methods.  Code examples in this repository should provide you with some guidance.

## Migrating between versions

* Major version increments (e.g. 1.x to 2.x) introduce significant breaking changes including new functionality.  Migrating between major versions may require significant code changes depending on your existing code.
* Minor version increments (e.g. 1.1.x to 1.2.x) are generally additive in nature but may include smaller breaking changes such as the movement of classes between namespaces.  Migrating between minor versions may require small code changes depending on your existing code.
* Build version increments (e.g. 1.1.5 to 1.1.10) are generally bugfixes and should not include any breaking changes.  **Note that build version increments in pre-release builds may contain significant breaking changes; 1.2.3-alpha may be sigificantly different to 1.2.2-alpha.**

### Migrating from 1.1 to 1.2

VAF Extensions 1.2 has removed the `MFiles.VAF.Extensions.MultiServerMode` namespace that was introduced in 1.1.  This change was made to reinforce that the `ConfigurableVaultApplicationBase<T>` class and the `TaskQueueBackgroundOperation` helpers are designed for all M-Files installations, not just those actively using Multi-Server Mode.  It is recommended that vault applications using the VAF Extensions library inherit from the  `MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<T>` class.
