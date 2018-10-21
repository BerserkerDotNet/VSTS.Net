# VSTS.Net
.Net client for Azure DevOps Services (Visual Studio Team Services) API

[![Build status](https://ci.appveyor.com/api/projects/status/et2718qanpkjf55m?svg=true)](https://ci.appveyor.com/project/BerserkerDotNet/vsts-net)

[![Nuget](https://buildstats.info/nuget/VSTS.Net?v=0.2.2)](https://www.nuget.org/packages/VSTS.Net)

[![Nuget](https://buildstats.info/nuget/VSTS.Net.AspNetCore?v=0.2.2)](https://www.nuget.org/packages/VSTS.Net.AspNetCore)

[Api documentation](https://berserkerdotnet.github.io/VSTS.Net/site/api/index.html)

## Usage

### Console app

```csharp
var query = @"SELECT [System.Id] FROM WorkItems 
        WHERE [System.WorkItemType] IN ('Bug', 'Task') AND [System.AssignedTo] Ever 'foo@bar.com' AND System.ChangedDate >= '01/01/2018'";

var urlBuilderFactory = new OnlineUrlBuilderFactory("foo");
var client = VstsClient.Get(urlBuilderFactory, accessToken: "secure token");
var items = await client.GetWorkItemsAsync(new WorkItemsQuery(query));
```
For OnPrem (TFS) versions use `OnPremUrlBuilderFactory` instead of `OnlineUrlBuilderFactory`

### Asp.Net Core
In the `Startup.cs` add `VstsNet` to the services collection

```csharp
services.AddAzureDevOpsServices(instanceName: "foo", accessToken: "secure token");
```

or if you have OnPrem (TFS) version:

```csharp
services.AddAzureDevOpsServices(new Uri("https://foo.mydomain.com"), accessToken: "secure token");
```

To use Azure DevOps Services style url:
```csharp
services.AddAzureDevOpsServices(new Uri("https://dev.azure.com/{organization}"), accessToken: "secure token");
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
