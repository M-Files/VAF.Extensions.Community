# ObjVerEx extension methods

## IsTemplate

Returns true if the `ObjVerEx` instance contains the built-in `MFBuiltInPropertyDefIsTemplate` property with a value of true:

```csharp
if(objVerEx.IsTemplate())
{
    Console.WriteLine($"The object is a template.");
}
else
{
    Console.WriteLine($"The object is not a template.");
}
```

## ToLookup

The base `ObjVerEx.ToLookup` method returns an M-Files API `Lookup` class instance pointing to that specific version of the object.  This extension method allows you to specify whether you would like the lookup to point to a specific version or always to the latest version:

```
// This is akin to the core objVerEx.ToLookup method.
var versionSpecificLookup = objVerEx.ToLookup(false);

// This lookup will always point to the latest version.
var latestVersionLookup = objVerEx.ToLookup(true);
```