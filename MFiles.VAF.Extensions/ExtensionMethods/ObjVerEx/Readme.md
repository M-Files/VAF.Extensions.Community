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