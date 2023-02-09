# Configuration upgrading

When using the `ConfigurableVaultApplicationBase<T>` class, changes to the structure of the configuration type (T) provided can cause issues with your application starting.  For example: if a class previously defines a property as a string but is changed to be an integer, the deserialization of any held configuration will fail and the application will not start.

This library supports the ability for you to programmatically control the upgrade process so that the application can convert any old configuration across to the new structures and continue loading.

## Adding versioning to your configuration class

Consider that we wish to make a change to the configuration structure but wish to be able to migrate the old configuration across.  Consider these configuration classes:

```csharp
public class OldConfiguration
{
    [DataMember]
    public string OldCultureString { get; set; }
}

public class NewConfiguration
{
    [DataMember]
    public string CultureString { get; set; }
}
```

We must do three things:

1. Change the configuration classes to use the correct base class.
2. Add the `[ConfigurationVersion]` attribute to all configuration classes.
3. Create methods that upgrade from the old to new configuration instances, and mark them with the `[ConfigurationUpgradeMethod]` attribute.

```csharp
// Note that we can leave the attribute off the "old" configuration class and it will be considered version `0.0`.
public class OldConfiguration
    : MFiles.VAF.Extensions.Configuration.VersionedConfigurationBase // We must inherit from VersionedConfigurationBase or ConfigurationBase.
{
    [DataMember]
    public string OldCultureString { get; set; }

    // We define the upgrade method on the older configuration instance, returning the new one.
    // This takes an instance of version 0 and returns an instance of version 1.
    [ConfigurationUpgradeMethod]
    public static NewConfiguration Upgrade(OldConfiguration version0)
    {
        return new NewConfiguration()
        {
            CultureString = version0.OldCultureString
        };
    }
}

[ConfigurationVersion("1.0", PreviousVersionType = typeof(OldConfiguration))]
public class NewConfiguration
    : MFiles.VAF.Extensions.Configuration.VersionedConfigurationBase // We must inherit from VersionedConfigurationBase or ConfigurationBase.
{
    [DataMember]
    public string CultureString { get; set; }
}
```

NOTE: You must use the VAF Extensions' `ConfigurableVaultApplicationBase<T>` base class for your vault application for the above attributes to be observed.

This will result in the following steps being taken by the library:

1. The library will locate all configuration classes that inherit from `VersionedConfigurationBase` (or a derived class) and find their respective version number, typically from the `[ConfigurationVersion]` attribute on the class.
2. The library will load the JSON configuration from the current location in Named Value Storage.
3. The library will attempt to find a version number in the JSON, fail, and assume that the configuration is version 0.
4. The library will deserialize the configuration to an instance of the class with version 0 (in this case: `OldConfiguration`).
5. The library will locate a configuration upgrade method that can convert `OldConfiguration` to the latest version.
6. The library will call the configuration upgrade method, returning an instance of `NewConfiguration`.
7. The current location in Named Value Storage will be updated with a serialized representation of the converted instance.
8. The VAF will continue to load the configuration and start the application.

## Handling configuration upgrade paths with multiple steps

Where configurations evolve over time, an upgrade may not simply a process of migrating from one version to another; instead the migration process may involve potentially various different initial version (e.g. some installations still have version 0 configuration in use, others version 1, and the new application requires version 2).

```csharp
// Note that we can leave the attribute off the first/initial configuration class and it will be considered version `0.0`.
public class Version0Configuration
    : MFiles.VAF.Extensions.Configuration.VersionedConfigurationBase // We must inherit from VersionedConfigurationBase or ConfigurationBase.
{
    [DataMember]
    public string OldCultureString { get; set; }

    // We define the upgrade method on the older configuration instance, returning the new one.
    // This takes an instance of version 0 and returns an instance of version 1.
    [ConfigurationUpgradeMethod]
    public static Version1Configuration Upgrade(Version0Configuration version0)
    {
        return new Version1Configuration()
        {
            CultureString = version0.OldCultureString
        };
    }
}

[ConfigurationVersion("1.0", PreviousVersionType = typeof(Version0Configuration))]
public class Version1Configuration
    : MFiles.VAF.Extensions.Configuration.VersionedConfigurationBase // We must inherit from VersionedConfigurationBase or ConfigurationBase.
{
    [DataMember]
    public string CultureString { get; set; }
}

[ConfigurationVersion("2.0", PreviousVersionType = typeof(Version1Configuration))]
public class Configuration
    : MFiles.VAF.Extensions.Configuration.ConfigurationBase // We must inherit from VersionedConfigurationBase or ConfigurationBase.
{
    [DataMember]
    public string CultureString { get; set; }

    [ConfigurationUpgradeMethod]
    public static Configuration Upgrade(Version1Configuration version1)
    {
        return new Configuration()
        {
            CultureString = version1.OldCultureString
        }
    }
}
```

This will result in the following steps being taken by the library:

1. The library will locate all configuration classes that inherit from `VersionedConfigurationBase` (or a derived class) and find their respective version number, typically from the `[ConfigurationVersion]` attribute on the class.
2. The library will load the JSON configuration from the current location in Named Value Storage.
3. The library will attempt to find a version number in the JSON.
    1. If the version number is 0, or not found, it will deserialize the JSON to an instance of `Version0Configuration` and use `Version0Configuration.Upgrade` to upgrade it to version 1.
    2. If the version number is 1 - either in the JSON or after the above step has run - it will deserialize the JSON to an instance of `Version1Configuration` and use `Version1Configuration.Upgrade` to upgrade it to version 2.
7. The current location in Named Value Storage will be updated with a serialized representation of the final version of the configuration.
8. The VAF will continue to load the configuration and start the application.

## Controlling where the configuration is loaded from

Older versions of the Vault Application Framework stored applications' configurations in different places to the latest release.  As a result, when upgrading older applications, it is sometimes important that you can specify where the older instances of the configuration may be held.  This can be done using the `[ConfigurationVersion]` attribute:

```csharp
[ConfigurationVersion
(
    "0.0", // Explicitly set the version to 0, as there'll be no version number in NVS.
    UsesCustomNVSLocation = true, // This version was stored somewhere other than the default.
    Namespace = "MyApplication", // This is the NVS namespace where it was held.
    Key = "config", // This is the key of the item.
    NamedValueType = MFilesAPI.MFNamedValueType.MFConfigurationValue // This is the type of named value item that was used.
)]
public class OldConfiguration
    : MFiles.VAF.Extensions.Configuration.VersionedConfigurationBase // We must inherit from VersionedConfigurationBase or ConfigurationBase.
{
    [DataMember]
    public string OldCultureString { get; set; }

    // We define the upgrade method on the older configuration instance, returning the new one.
    // This takes an instance of version 0 and returns an instance of version 1.
    [ConfigurationUpgradeMethod]
    public static NewConfiguration Upgrade(OldConfiguration version0)
    {
        return new NewConfiguration()
        {
            CultureString = version0.OldCultureString
        };
    }
}

[ConfigurationVersion("1.0", PreviousVersionType = typeof(OldConfiguration))]
public class NewConfiguration
    : MFiles.VAF.Extensions.Configuration.VersionedConfigurationBase // We must inherit from VersionedConfigurationBase or ConfigurationBase.
{
    [DataMember]
    public string CultureString { get; set; }
}
```

NOTE: In the above example, `OldConfiguration` will be read from the custom NVS location but `NewConfiguration` will be written to the standard location.  This style configuration will effectively move the configuration; the old version is read from the custom location, deserialized, converted, then serialized and saved to the standard location.

## Disabling configuration upgrading

To completely disable configuration upgrading, return null from the `GetConfigurationUpgradeManager` method on your VaultApplication class:

```csharp
public class VaultApplication : MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
{
    /// <inheritdoc />
    /// <remarks>Configuration upgrading is disabled.</remarks>
    public override IConfigurationUpgradeManager GetConfigurationUpgradeManager()
	{
		return null;
	}
}
```