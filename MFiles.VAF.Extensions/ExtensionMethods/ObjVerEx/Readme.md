# ObjVerEx extension methods

## AddFile

Adds a file to an existing M-Files object:

```csharp
// Assume that "sourceStream" contains the new file contents:
using (var sourceStream = ...)
{
    env.ObjVerEx.AddFile
    (
        "My new file",
        ".pdf,
        sourceStream
    );
}
```

*Note: The object must be already checked out and have the "Single File Document" property appropriately set prior to calling this method.*

## ReplaceFileContent

Replaces the content of an existing file with content from an existing .NET `System.IO.Stream` instance:

```csharp
// Assume that "sourceStream" contains the new file contents:
using (var sourceStream = ...)
{
    env.ObjVerEx.ReplaceFileContent
    (
        env.ObjVerEx.Info.Files[1],
        sourceStream
    );
}
```

If the object contains exactly one file and you wish to update that file then another overload can be used:

```csharp
// Assume that "sourceStream" contains the new file contents:
using (var sourceStream = ...)
{
    objVerEx.ReplaceFileContent
    (
        sourceStream
    );
}
```

*Note: When working with files in event handlers, [GetFilesForModificationInEventHandler](https://www.m-files.com/api/documentation/latest/index.html#MFilesAPI~VaultObjectFileOperations~GetFilesForModificationInEventHandler.html) will need to be called.  Because of this it may be more practical to use the `[objectFile.ReplaceFileContent](https://github.com/M-Files/COMAPI.Extensions.Community/blob/master/MFilesAPI.Extensions/ExtensionMethods/ObjectFileExtensionMethods.cs#L165)` extension method from the M-Files COM API extension library.*

## ToLookup

The base `ObjVerEx.ToLookup` method returns an M-Files API `Lookup` class instance pointing to that specific version of the object.  This extension method allows you to specify whether you would like the lookup to point to a specific version or always to the latest version:

```
// This is akin to the core objVerEx.ToLookup method.
var versionSpecificLookup = objVerEx.ToLookup(false);

// This lookup will always point to the latest version.
var latestVersionLookup = objVerEx.ToLookup(true);
```

## GetPropertyAs methods

These methods make it easier to retrieve property value values in standard .NET types.

### GetPropertyAsDateTime

Returns the value of a property as a `DateTime?` instance.  Returns null if the property does not exist or is marked as null or not initialized.  Throws an `ArgumentException` exception if the property definition ID provided does not map to an appropriate M-Files property data type.

```csharp
DateTime? value = env.ObjVerEx.GetPropertyAsDateTime(1234);
```

### GetPropertyAsInteger

Returns the value of a property as an `int?` instance.  Returns null if the property does not exist or is marked as null or not initialized.  Throws an `ArgumentException` exception if the property definition ID provided does not map to an appropriate M-Files property data type.

```csharp
int? value = env.ObjVerEx.GetPropertyAsInteger(1234);
```

### GetPropertyAsLong

Returns the value of a property as an `long?` instance.  Returns null if the property does not exist or is marked as null or not initialized.  Throws an `ArgumentException` exception if the property definition ID provided does not map to an appropriate M-Files property data type.

```csharp
long? value = env.ObjVerEx.GetPropertyAsLong(1234);
```

### GetPropertyAsDouble

Returns the value of a property as an `double?` instance.  Returns null if the property does not exist or is marked as null or not initialized.  Throws an `ArgumentException` exception if the property definition ID provided does not map to an appropriate M-Files property data type.

```csharp
double? value = env.ObjVerEx.GetPropertyAsDouble(1234);
```

## ExpandSimpleConcatenation

Performs replacement of content in a similar manner to the built-in [simple concatenation of properties](https://www.m-files.com/user-guide/latest/eng/Automatic_values.html#automatic_values__simple_concatenation_of_properties).

### Internal ID

```csharp
var output = objVerEx.ExpandSimpleConcatenation("The internal ID of object %PROPERTY_0% is %INTERNALID%")
```

### Object type ID

```csharp
var output = objVerEx.ExpandSimpleConcatenation("The object type ID is %OBJECTTYPEID%.")
```

### Object version ID

```csharp
var output = objVerEx.ExpandSimpleConcatenation("The object version ID is %OBJECTVERSIONID%.")
```

### Object GUID

```csharp
var output = objVerEx.ExpandSimpleConcatenation("The object GUID is %OBJECTGUID%.")
```

### External ID

```csharp
var output = objVerEx.ExpandSimpleConcatenation("The external ID of object %PROPERTY_0% is %EXTERNALID%.")
```

### Display ID

This will return the external ID (if set), the original object ID (if set), or the internal ID (if neither set).

```csharp
var output = objVerEx.ExpandSimpleConcatenation("The display ID of object %PROPERTY_0% is %DISPLAYID%")
```

### Combined internal and external ID

```csharp
var output = objVerEx.ExpandSimpleConcatenation("The internal ID of object %PROPERTY_0% is %INTERNALID%, and the external ID is %EXTERNALID%.")
```

### Indirect properties

```csharp
var output = objVerEx.ExpandSimpleConcatenation("The customer's country of object %PROPERTY_0% is %PROPERTY_{MF.PD.Customer}.PROPERTY_{MF.PD.Country}%.")
```

### Vault GUID

```csharp
var output = objVerEx.ExpandSimpleConcatenation("The vault GUID is %VAULTGUID%.")
```

## TryGetPropertyText

Avoid error handling methods while using ``MFIdentifier`` objects representing a property definition
taken directly out of the VAF ``Configuration`` object.

If the configuration contains a ``null`` value or an invalid ID/GUID/Alias for a property definition
in configuration of the vault application it just returns ``false`` and sets the output
string to ``null`` without throwing any exception as known e.g. from the method:
```csharp
public static bool TryParse (string? s, out int? result);
```

### Parameter "prop" as ``MFIdentifier`` object

This method differs from the other extension methods by using ``MFIdentifier``
instead of ``int`` for specifying the property in order to use the configuration parameter
of the vault application as it is to avoid error handling.

If ``int`` would be used as parameter instead then this method would become obsolete and
could be replaced by standard ``ObjVerEx.GetPropertyText(int prop)`` method.


### Example:  
__Full name of a person to be used in various environments__

Some environments perhaps may only contain only first name and last name. Perhaps the
values where copied from a test environment using IDs which do not fit here. This would
cause the following checks for each configuration parameter:

```csharp
// Get the configuration variable using a sample variable
// to be replaced by variable to be used in real life
MFIdentifier prop = Configuration.SubObject.Somewhere.SomeVariable;

// Set the string depending on validity of the configuration variable
string result = null == prop || !prop.IsResolved
    ? null
    : env.ObjVerEx.GetPropertyText(prop);
```

If this check would be forgotten it perhaps won't cause an exception in the development
environment but in the target environment with other configuration.

The method ``TryGetPropertyText`` is meant to avoid this especially if the
vault application should be used in more than one customer system. Then you can use simply
one line and it would be easier __not__ to forget the checks, like this example shows:

```csharp
// Try to get all properties which are configured
_ = env.ObjVerEx.TryGetPropertyText(cfg.NameFields.PropSalutation, out string salutation);
_ = env.ObjVerEx.TryGetPropertyText(cfg.NameFields.PropAcademicTitle, out string academicTitle);
_ = env.ObjVerEx.TryGetPropertyText(cfg.NameFields.PropFirstName, out string firstName);
_ = env.ObjVerEx.TryGetPropertyText(cfg.NameFields.PropMiddleName, out string middleName);
_ = env.ObjVerEx.TryGetPropertyText(cfg.NameFields.PropLastName, out string lastName);

// Compose real name
string displayName = "";
displayName = (displayName + (salutation ?? "")).TrimEnd();
displayName += 0 < displayName.Length ? " " : "";
displayName = (displayName + (academicTitle ?? "")).TrimEnd();
displayName += 0 < displayName.Length ? " " : "";
displayName = (displayName + (firstName ?? "")).TrimEnd();
displayName += 0 < displayName.Length ? " " : "";
displayName = (displayName + (middleName ?? "")).TrimEnd();

// Put last name at the position depending on configuration
displayName = cfg.Options.PutLastNameFirst && !string.IsNullOrEmpty(lastName)
    ? lastName + (0 < displayName.Length ? ", " : "") + displayName
    : displayName + (0 < displayName.Length && !string.IsNullOrEmpty(lastName) ? " " : "") + (lastName ?? "");
```

Implementing this without the helper method would make the implementation much
more complex if someone tries to understand what happens.

### Output parameter as string / Return value as bool

The output parameter would be set as additional parameter like quite common
by other ``TryDoSomething`` methods in the C# universe.

The return value will be ``true`` if it was a ``MFIdentifier`` object
different from ``null`` which could be resolved as a valid property definition.

In cases where the standard ``GetPropertyText`` method would throw an exception because of
an identifier object not set or not resolved, the ``TryGetPropertyText`` would just return
``false`` like common for those methods and set the output parameter to ``null``.

### Possible other extensions

This could be also implemented for all other methods where the identifier may be invalid or
not set like ``GetPropertyAs<T>``. If someone would implement this or similar functions
I would probably use it.
