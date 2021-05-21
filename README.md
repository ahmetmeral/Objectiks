# Objectiks .NET is a NoDb json document store 

Objectiks is an open-source NoDb json document store, that can run over a single or multiple **json** files.



## Get Started

> ```Install-Package Objectiks.NoDb -Version 1.0.0-beta-01```

##### Folder structure

> Default Directory ``` .\<Current Directory>\Objectiks```

- **Objectiks** [**Root**]
  - **Documents**
    - Pages
      - Pages.json
    - Tags
      - Tags.json
  
### How to use Objectiks

##### Models (samples)

```csharp
[TypeOf("Pages"), Serializable, CacheOf(1000)]
public class Pages
{
    [Primary]
    public int Id { get; set; }

    [WorkOf, Requried]
    public int WorkSpaceRef { get; set; }

    [UserOf, Requried]
    public int UserRef { get; set; }

    [KeyOf, Requried]
    public string Tag { get; set; }

    [Requried]
    public string Title { get; set; }

    public string File { get; set; }

    [Ignore]
    public string Contents { get; set; }
}

[TypeOf("Tags"), Serializable,CacheOf(1000)]
public class Tags
{
    [Primary]
    public int Id { get; set; }

    [Requried]
    public string Name { get; set; }
}

```

> **Primary,Requried or Ignore  etc.. attributes are used for writing to a file.**


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










