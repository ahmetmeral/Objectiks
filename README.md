# Objectiks .NET is a NoDb json document store 

Objectiks is an open-source NoDb json document store, that can run over a single or multiple **json** files.

### Tasks

- [ ] File watcher
- [ ] Nuget Package
- [ ] Sample Project
- [ ] Refs Sample
- [ ] Benchmark
- [ ] Redis Cache


## Get Started



##### Folder structure

> Default Directory ``` .\<Current Directory>\Objectiks```

- **Objectiks** [**Root**]
  - **Documents**
    - Pages
      - Contents
      - Pages.json
    - Categories
      - Categories.json
    - Tags
      - Tags.json
  - **Schemes**
    - Pages.json
    - Categories.json
    - Tags.json
  - Objectik.json [**manifest**]


###### manifest.json

```json
{
  "Name": "Objectiks",
  "Version": "0.0.4",
  "Author": "Ahmet Meral",
  "Primary": "Id",
  "KeyOf": [],
  "TypeOf": [ "Pages", "Categories", "Tags" ],
  "BufferSize": 512,
  "Extention": {
    "Documents": "*.json"
  },
  "Cache": {
    "Expire": 1000
  },
  "Documents": {
    "PropertyOverride": true,
    "StoragePartial": false,
    "StoragePartialLimit": 2
  },
  "Vars": {
    "AnyVariables": true
  }
}
```

###### pages.json schema (sample)

```json 
{
  "TypeOf": "Pages",
  "ParseOf": "Document",
  "Primary": "Id",
  "KeyOf": [ "Name", "Language", "CategoryRef" ],
  "Refs": [
    {
      "ParseOf": "M:M",
      "TypeOf": "Tags",
      "KeyOf": {
        "Source": [ "TagRefs" ],
        "Target": [ "Id" ],
        "Any": false
      },
      "MapOf": {
        "Source": "Tags"
      }
    },
    {
      "ParseOf": "1:1",
      "TypeOf": "Categories",
      "KeyOf": {
        "Source": [ "CategoryRef" ],
        "Target": [ "Id" ]
      },
      "MapOf": {
        "Source": "Category"
      }
    },
    {
      "ParseOf": "1:M",
      "TypeOf": "Categories",
      "MapOf": {
        "Source": "Categories"
      }
    },
    {
      "ParseOf": "1:1F",
      "TypeOf": "Copy",
      "MapOf": {
        "Source": "File",
        "Target": "Contents"
      }
    }
  ],
  "Cache": {
    "Expire": 100000
  }
}

```

###### categories.json schema  (sample)

```json 
{
  "TypeOf": "Categories",
  "ParseOf": "Default",
  "Primary": "Id",
  "KeyOf": [ "Name", "Language" ]
}
```


###### tags.json schema  (sample)

```json 
{
  "TypeOf": "Tags",
  "ParseOf": "Document",
  "Primary": "Id",
  "KeyOf": [ "Name" ]
}
```

### How to use Objectiks


##### Document Reader

```csharp
//default directory .\<Current Directory>\Objectiks
var repos = new ObjectiksOf();
```

```csharp
var page = repos
    .TypeOf<Pages>()
    .PrimaryOf(1)
    .First();
```

```csharp
var listDynamic = repos
    .TypeOf("Pages")
    .OrderBy("Title")
    .Desc()
    .ToList();

var categories = repos
    .TypeOf("Categories")
    .KeyOf("foods")
    .KeyOf("travel")
    .KeyOf("music")
    .Any()
    .ToList();

```
##### Document Writer

```csharp

var repos = new ObjectiksOf();

//writer
var pages = TestSetup.GeneratePages(10000);
using (var writer = repos.WriterOf<Pages>())
{
    //creates a new file for every thousand records
    //manifest.json can also be used for all types through two parameters.
    //StoragePartial=true and StoragePartialLimit=1000
    writer.UsePartialStore(1000);
    writer.UseFormatting();
    writer.Add(pages);
    writer.SubmitChanges();
}

//merge
var merge = repos
    .TypeOf<Pages>()
    .PrimaryOf(1)
    .First();

merge.Title = "Merge";

using (var writer = repos.WriterOf<Pages>())
{
    writer.Add(merge);
    writer.SubmitChanges();
}


//delete
var deleted= repos.First<Pages>(2);
using (var writer = repos.WriterOf<Pages>())
{
    writer.Delete(deleted);
    writer.SubmitChanges();
}

```

##### Document Meta

```csharp
var repos = new ObjectiksOf();
var meta = repos.GetTypeMeta("pages");

var typeOf = meta.TypeOf;
var keyOf = meta.KeyOf;
var parseOf = meta.ParseOf;
var totalCount = meta.TotalRecords;
var sequence = meta.Sequence;
var directory = meta.Directory;
var keyCount = meta.Keys.Count;
var partition = meta.Partitions;

```










