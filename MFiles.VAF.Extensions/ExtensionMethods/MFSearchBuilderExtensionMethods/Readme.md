# MFSearchBuilder extension methods

## Full text searches

Adds a full-text search condition:

```csharp
mfSearchBuilder.FullText
(
    value: "agenda"
);
```

Note: By default this will search both file contents and metadata.  This functionality can be changed by using the optional `searchFlags` parameter.

## Property searches

These extension methods provide typed `value` arguments for common .NET data types.  Where appropriate (e.g. DateTime searches), objects provide helper methods for searches that would typically have required `DataFunctionCall` instances.

### Boolean property search

Adds a search condition for a property definition of type `MFDatatypeBoolean`:

```csharp
mfSearchBuilder.Property
(
    propertyDef: 1234,
    value: true
);
```

Note: Supports searching by an empty property value:

```csharp
mfSearchBuilder.Property
(
    propertyDef: 1234,
    value: (bool?)null
);
```

### Date or Timestamp property search

Adds a search condition for a property definition of type `MFDatatypeDate` or `MFDatatypeTimestamp`:

```csharp
mfSearchBuilder.Property
(
    propertyDef: 1234,
    value: new DateTime(2020, 02, 02)
);
```

Note: Supports searching by an empty property value:

```csharp
mfSearchBuilder.Property
(
    propertyDef: 1234,
    value: (DateTime?)null
);
```

#### Stripping the time component from a timestamp property

The `Date` method can be used to search for actions that happened on a specific date, for example for all objects created on 1st January 2020:

```csharp
mfSearchBuilder.Date
(
    propertyDef: (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated,
    value: new DateTime(2020, 01, 01)
);
```

#### Searching by the year component of a date or timestamp property

The `Year` method can be used to search for items that occurred during a specific year, regardless of the date or month components:

```csharp
mfSearchBuilder.Year
(
    propertyDef: (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated,
    value: 2020
);
```

Note: Years must be four-digit values (2020, not 20).

#### Searching by the month component of a date or timestamp property

The `Month` method can be used to search for items that occurred during a specific month, regardless of the date or year:

```csharp
mfSearchBuilder.Month
(
    propertyDef: (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated,
    month: 3
);
```

Note: Month numbers must be between 1 (January) and 12 (December).

#### Searching by the year and month components of a date or timestamp property

The `YearAndMonth` method can be used to search for items that occurred during a specific month of a specific year, regardless of the date component:

```csharp
mfSearchBuilder.YearAndMonth
(
    propertyDef: (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated,
    year: 2020,
    month: 3
);
```

Note: Month numbers must be between 1 (January) and 12 (December).  Years must be four-digit values (2020, not 20).

#### Searching by the number of days there are until a specific date or timestamp property

```csharp
mfSearchBuilder.DaysTo
(
    propertyDef: (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated,
    value: 30
);
```

#### Searching by the number of days that have passed since a specific date or timestamp property

```csharp
mfSearchBuilder.DaysFrom
(
    propertyDef: (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated,
    value: 30
);
```

### Numeric property searches

Adds search conditions for a property definition of type `MFDatatypeInteger`,  `MFDatatypeInteger64`, `MFDatatypeFloating`, `MFDatatypeLookup`, or `MFDatatypeMultiSelectLookup`:

```csharp
mfSearchBuilder.Property
(
    propertyDef: 1234,
    value: 1000
);
```

Note: values can be provided as integers, longs or doubles.

Note: When searching for properties of type `MFDatatypeLookup` or `MFDatatypeMultiSelectLookup`, the integer argument is expected to be the internal ID of the object that this property value points to.

### Text property searches

Adds a search condition for a property definition of type `MFDatatypeText` or `MFDatatypeMultiLineText`:

```csharp
mfSearchBuilder.Property
(
    propertyDef: 1234,
    value: "hello world"
);
```

## Searching by object information/flags

### Object ID

Adds a search condition for the internal object ID:

```csharp
mfSearchBuilder.ObjectId
(
    objectId: 1234
);
```

### Object ID segment

Adds a search condition to restrict the search results to objects whose internal ID resides within a given segment.  For example, a segment index of zero and a segment size of 1,000 will return objects with IDs between 0 and 999 inclusive:

```csharp
mfSearchBuilder.ObjectIdSegment
(
    segmentIndex: 0,
    segmentSize: 1000
);
```

### Checked out search

Searches for objects that are (or are not) checked out:

```csharp
mfSearchBuilder.IsCheckedOut
(
    isCheckedOut: false // Only find objects that are not checked out.
);
```

### External ID search

Adds a search condition to search by an object's external ID (e.g. where an object has been synchronised from an external database via an ODBC connection):

```csharp
mfSearchBuilder.ExternalId
(
    externalId: "CUST0001"
);
```

## Searching by file data

### Has files

Adds a search condition to search only for objects with or without files:

```csharp
mfSearchBuilder.HasFiles
(
    hasFiles: true // Only find objects with files.
);
```

### File size

Adds a search condition to search only for objects with files that match a size constraint:

```csharp
mfSearchBuilder.FileSize
(
    size: 1024 * 1024, // in bytes (1MB)
    conditionType: MFConditionType.MFConditionGreaterThanOrEqual
);
```

### File extension

Adds a search condition to search only for objects that have files with a specific extension:

```csharp
mfSearchBuilder.FileExension
(
    extension: ".pdf"
);
```

Note: Extensions can be provided with or without the dot/period at the front.

## Searching by effective permissions

Searches executed by the M-Files Server user (e.g. those executed within the Vault Application Framework) are executed with administrative privileges.  These helper methods can be used to restrict searches to only return items that specific users have permissions to.

### Visible to a specific user

Adds a search condition to only return objects that are visible to the given user:

```csharp
mfSearchBuilder.VisibleTo
(
    userId: 123
);
```

### Editable by to a specific user

Adds a search condition to only return objects that the given user has edit rights to:

```csharp
mfSearchBuilder.EditableBy
(
    userId: 123
);
```

### Deletable by to a specific user

Adds a search condition to only return objects that the given user has deletion rights to:

```csharp
mfSearchBuilder.DeletableBy
(
    userId: 123
);
```

# Using indirection levels

Often searches need to return objects that should be filtered by properties on objects they refer to, rather than their own properties.  For example, a simple search may want to return all `Contact` objects for `Customer`s located in the `United Kingdom`.  This can be done using indirection levels.  More information on indirection levels is available in the [developer portal](https://developer.m-files.com/APIs/COM-API/Searching/SearchConditions/#using-indirection-levels).

All property-value search condition extension methods additionally allow the provision of a `PropertyDefOrObjectTypes` instance representing the indirection levels, and helper methods are available for building these.

### By object type

The example below is based upon the example in the [developer portal](https://developer.m-files.com/APIs/COM-API/Searching/SearchConditions/#referencing-by-object-type).
{:.note }

```csharp
// Create the indirection levels.
var indirectionLevels = new PropertyDefsOrObjectTypes()
    .AddObjectTypeIndirectionLevel(136); // The `Customer` object type Id

// Execute the search.
var searchBuilder = new MFSearchBuilder(env.Vault);
searchBuilder.Deleted(false);
searchBuilder.ObjType(149); // The 'Contact' object type Id.
searchBuilder.Property
(
    1090, // The Id of the 'Country' property definition.
    3, // The Id of the 'United Kingdom' value list item.
    indirectionLevels: indirectionLevels
);
```

### By property definition

The example below is based upon the example in the [developer portal](https://developer.m-files.com/APIs/COM-API/Searching/SearchConditions/#referencing-by-property-definition).
{:.note }

```csharp
// Create the indirection levels.
var indirectionLevels = new PropertyDefsOrObjectTypes()
    .AddPropertyDefIndirectionLevel(1174); // The `Signer` property definition

// Execute the search.
var searchBuilder = new MFSearchBuilder(env.Vault);
searchBuilder.Deleted(false);
searchBuilder.ObjType(0); // Only retrieve documents
searchBuilder.Property
(
    1136, // The Id of the 'Department' property definition.
    1, // The Id of the 'Sales' value list item.
    indirectionLevels: indirectionLevels
);
```
