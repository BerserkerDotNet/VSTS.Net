# VSTS.Net
.Net client for Visual Studio Team Services API

[![Build status](https://ci.appveyor.com/api/projects/status/et2718qanpkjf55m?svg=true)](https://ci.appveyor.com/project/BerserkerDotNet/vsts-net)

[![Nuget](https://buildstats.info/nuget/VSTS.Net?v=0.2.1)](https://www.nuget.org/packages/VSTS.Net)

[![Nuget](https://buildstats.info/nuget/VSTS.Net.AspNetCore?v=0.2.1)](https://www.nuget.org/packages/VSTS.Net.AspNetCore)

[Api documentation](https://berserkerdotnet.github.io/VSTS.Net/site/api/index.html)

## Usage

### Console app

```csharp
var query = @"SELECT [System.Id] FROM WorkItems 
        WHERE [System.WorkItemType] IN ('Bug', 'Task') AND [System.AssignedTo] Ever 'foo@bar.com' AND System.ChangedDate >= '01/01/2018'";

var client = VstsClient.Get(instanceName: "foo", accessToken: "secure token");
var items = await client.GetWorkItemsAsync(new WorkItemsQuery(query));
```

### Asp.Net Core
In the `Startup.cs` add `VstsNet` to the services collection

```csharp
services.AddVstsNet(instanceName: "foo", accessToken: "secure token");
```

Now you can consume Vsts client through DI:

```csharp
private readonly IVstsClient client;

public HomeController(IVstsClient client)
{
	this.client = client;
}

public async Task<IActionResult> Index()
{
    var prs = await client.GetPullRequestsAsync("MyProject", "MyRepository", new PullRequestQuery { CreatedAfter = DateTime.Now.AddDays(-5) });
}
```
