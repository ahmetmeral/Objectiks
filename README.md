# Objectiks .NET is a NoDb document store 

Objectiks is an open-source NoDb document store, that can run over a single or multiple **json** files.

### Tasks

- [ ] Sample Project
- [ ] Refs Sample
- [ ] Benchmark
- [ ] File watcher
- [ ] Redis Cache



## Get Started



##### Folder structure

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

###### pages.json schema 

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

###### categories.json schema 

```json 
{
  "TypeOf": "Categories",
  "ParseOf": "Default",
  "Primary": "Id",
  "KeyOf": [ "Name", "Language" ]
}
```


###### tags.json schema 

```json 
{
  "TypeOf": "Tags",
  "ParseOf": "Document",
  "Primary": "Id",
  "KeyOf": [ "Name" ]
}
```

### How to use Objectiks

##### Document Options 

```csharp

var baseDirectory = Path.Combine(
    Directory.GetCurrentDirectory(),
    DocumentDefaults.Root
);

var options = new DocumentOptions();
options.AddDefaultParsers();
options.UseCacheTypeOf<DocumentInMemory>();
options.UseEngineTypeOf<JsonEngine>();
options.UseConnection(new DocumentConnection
{
    BaseDirectory = baseDirectory
});
```               

##### Document Reader

```csharp
var repos = new ObjectiksOf(options);
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

var repos = new ObjectiksOf(options);

//writer
var pages = TestSetup.GeneratePages(10000);
using (var writer = repos.WriterOf<Pages>())
{
    writer.UsePartialStore(1000);
    writer.UseFormatting();
    writer.Add(pages);
    writer.SubmitChanges();
}

//merge
var mergePage = repos
    .TypeOf<Pages>()
    .PrimaryOf(1)
    .First();

mergePage.Title = "Merge";

using (var writer = repos.WriterOf<Pages>())
{
    writer.Add(mergePage);
    writer.SubmitChanges();
}

//delete

using (var writer = repos.WriterOf<Pages>())
{
    writer.Delete(mergePage);
    writer.SubmitChanges();
}

```

##### Document Meta

```csharp
var repos = new ObjectiksOf(options);
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










