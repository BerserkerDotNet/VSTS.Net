# VSTS.Net
REST client for Visual Studio Team Services

## Usage Console app

```csharp
            var query = @"SELECT [System.Id] FROM WorkItems 
                            WHERE [System.WorkItemType] IN ('Bug', 'Task') AND [System.AssignedTo] Ever 'foo@bar.com' AND System.ChangedDate >= '01/01/2018'";

            var client = VstsClient.Get(instanceName: "foo", accessToken: "secure token");
            var items = await client.GetWorkItemsAsync("MyProject", new WorkItemsQuery(query));
```

## Registering with Asp.Net Core DI

```csharp
	services.AddVstsNet(instanceName: "foo", accessToken: "secure token");
```