# VaultObjectFileOperations extension methods

## AddFile

Adds a file to an existing M-Files object.

```csharp
// Assume that "sourceStream" contains the new file contents:
using (var sourceStream = ...)
{
    env.Vault.ObjectFileOperations.AddFile
    (
        env.ObjVerEx,
        "My new file",
        ".pdf,
        sourceStream
    );
}
```

*Note: the object must be already checked out and have the "Single File Document" property appropriately set prior to calling this method.*