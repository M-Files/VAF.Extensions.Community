# ObjVerEx extension methods

## AddFile

Adds a file to an existing M-Files object.

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

*Note: the object must be already checked out and have the "Single File Document" property appropriately set prior to calling this method.*

## ToLookup

The base `ObjVerEx.ToLookup` method returns an M-Files API `Lookup` class instance pointing to that specific version of the object.  This extension method allows you to specify whether you would like the lookup to point to a specific version or always to the latest version:

```
// This is akin to the core objVerEx.ToLookup method.
var versionSpecificLookup = objVerEx.ToLookup(false);

// This lookup will always point to the latest version.
var latestVersionLookup = objVerEx.ToLookup(true);
```