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