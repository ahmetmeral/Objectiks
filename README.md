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


##### Document Options

```csharp

public class NoDbEngineInMemoryOption : NoDbDocumentOption
{
    public NoDbEngineInMemoryOption() : base()
    {
        Name = "NoDbEngineProvider";

        RegisterTypeOf<Pages>();
        RegisterTypeOf<Tags>();
    }
}


//Initialize json documents
//default directory .\<Current Directory>\Objectiks\Documents
ObjectiksOf.Core.Initialize(new DocumentProvider(), new NoDbEngineInMemoryOption());


```

##### Document Reader

```csharp

var repos = new ObjectiksOf();

var page = repos
    .TypeOf<Pages>()
    .PrimaryOf(1)
    .First();


var pages = repos
    .TypeOf<Pages>()
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
    //StoragePartial=true and StoragePartialLimit=1000
    writer.UsePartialStore(1000);
    writer.UseFormatting();
    writer.AddDocuments(pages);
    writer.SubmitChanges();
}

```







