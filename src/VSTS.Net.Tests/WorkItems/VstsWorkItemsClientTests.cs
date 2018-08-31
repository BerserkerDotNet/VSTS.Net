using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Tests.Types;
using VSTS.Net.Types;

namespace VSTS.Net.Tests.WorkItems
{
    [TestFixture]
    public class VstsWorkItemsClientTests : BaseHttpClientTests
    {
        #region GetWorkItems tests
        [Test]
        public async Task GetWorkItemsShouldComposeCorrectUrlForFlatQueries()
        {
            var query = WorkItemsQuery.Get("Dummy query");
            var queryResult = new FlatWorkItemsQueryResult
            {
                Columns = new[] { new ColumnReference("Id", "System.Id"), new ColumnReference("Title", "System.Title") },
                WorkItems = new[] { new WorkItemReference(2), new WorkItemReference(34), new WorkItemReference(56) }
            };
            var fields = queryResult.Columns.Select(c => c.ReferenceName);
            var ids = queryResult.WorkItems.Select(w => w.Id);
            var workItems = ids.Select(i => new WorkItem { Id = i });

            HttpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken))
                .ReturnsAsync(queryResult)
                .Verifiable();

            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.Is<string>(u => VerifyWorkItemsUrl(u, fields, ids)), CancellationToken))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workItems })
                .Verifiable();

            var result = await Client.GetWorkItemsAsync(query, false, CancellationToken);

            result.Should().HaveCount(workItems.Count());
            result.Should().BeEquivalentTo(workItems);

            HttpClientMock.Verify();
        }

        [Test]
        public async Task GetWorkItemsShouldComposeCorrectUrlForTreeQueries()
        {
            var query = WorkItemsQuery.Get("Dummy query", isHierarchical: true);
            var queryResult = new HierarchicalWorkItemsQueryResult
            {
                Columns = new[] { new ColumnReference("Id", "System.Id"), new ColumnReference("Title", "System.Title") },
                WorkItemRelations = new[]
                {
                    new WorkItemLink(new WorkItemReference(2), new WorkItemReference(34), "foo"),
                    new WorkItemLink(new WorkItemReference(2), new WorkItemReference(2), "foo"),
                    new WorkItemLink(new WorkItemReference(2), new WorkItemReference(56), "foo")
                }
            };
            var fields = queryResult.Columns.Select(c => c.ReferenceName);
            var ids = queryResult.WorkItemRelations.Select(w => w.Source.Id)
                .Union(queryResult.WorkItemRelations.Select(w => w.Target.Id))
                .Distinct()
                .ToArray();
            var workItems = ids.Select(i => new WorkItem { Id = i });

            HttpClientMock.Setup(c => c.ExecutePost<HierarchicalWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => q.IsHierarchical), CancellationToken))
                .ReturnsAsync(queryResult)
                .Verifiable();

            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.Is<string>(u => VerifyWorkItemsUrl(u, fields, ids)), CancellationToken))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workItems })
                .Verifiable();

            var result = await Client.GetWorkItemsAsync(query, false, CancellationToken);

            result.Should().HaveCount(workItems.Count());
            result.Should().BeEquivalentTo(workItems);

            HttpClientMock.Verify();
        }

        [Test]
        public void GetWorkItemsShouldThrowExceptionIfNullQuery()
        {
            var query = WorkItemsQuery.Get("Dummy query");

            HttpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken.None))
                .ReturnsAsync((FlatWorkItemsQueryResult)null);

            Client.Awaiting(c => c.GetWorkItemsAsync(query))
                .Should().Throw<NotSupportedException>();
        }

        [Test]
        public async Task GetWorkItemsShouldReturnEmptyListOfWorkItemsIfResponseIsNull()
        {
            var query = WorkItemsQuery.Get("Dummy query");

            HttpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken.None))
                .ReturnsAsync(new FlatWorkItemsQueryResult { Columns = Enumerable.Empty<ColumnReference>(), WorkItems = Enumerable.Empty<WorkItemReference>() });
            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.IsAny<string>(), CancellationToken.None))
                .ReturnsAsync((CollectionResponse<WorkItem>)null);

            var result = await Client.GetWorkItemsAsync(query);

            result.Should().HaveCount(0);
        }

        [Test]
        public void GetWorkItemsShouldNotCatchExceptions()
        {
            var query = WorkItemsQuery.Get("Dummy query");

            // Test 1
            HttpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken.None))
                .Throws<Exception>();

            Client.Awaiting(c => c.GetWorkItemsAsync(query))
                .Should().Throw<Exception>();

            // Test 2
            HttpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken.None))
                .ReturnsAsync(new FlatWorkItemsQueryResult
                {
                    Columns = new[] { new ColumnReference("Title", "System.Title") },
                    WorkItems = new[] { new WorkItemReference(1) }
                });
            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.IsAny<string>(), CancellationToken.None))
                .Throws(new Exception());

            Client.Awaiting(c => c.GetWorkItemsAsync(query))
                .Should().Throw<Exception>();
        }

        [Test]
        public async Task GetWorkItemsShouldReturnEmptyIfNoItemsFound()
        {
            var query = WorkItemsQuery.Get("Dummy query");
            var queryResult = new FlatWorkItemsQueryResult
            {
                Columns = new ColumnReference[0],
                WorkItems = new WorkItemReference[0]
            };

            HttpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken))
                .ReturnsAsync(queryResult)
                .Verifiable();

            var result = await Client.GetWorkItemsAsync(query, false, CancellationToken);

            result.Should().BeEmpty();

            HttpClientMock.Verify();
            HttpClientMock.Verify(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.IsAny<string>(), CancellationToken), Times.Never());
        }

        [Test]
        public async Task GetWorkItemsShouldIgnoreFiledsParameterIfExpand()
        {
            var query = WorkItemsQuery.Get("Dummy query");
            var queryResult = new FlatWorkItemsQueryResult
            {
                Columns = new[] { new ColumnReference("Id", "System.Id"), new ColumnReference("Title", "System.Title") },
                WorkItems = new[] { new WorkItemReference(2), new WorkItemReference(34), new WorkItemReference(56) }
            };
            var fields = queryResult.Columns.Select(c => c.ReferenceName);
            var ids = queryResult.WorkItems.Select(w => w.Id);
            var workItems = ids.Select(i => new WorkItem { Id = i });

            HttpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken))
                .ReturnsAsync(queryResult)
                .Verifiable();

            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.Is<string>(u => VerifyWorkItemsUrl(u, ids)), CancellationToken))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workItems })
                .Verifiable();

            var result = await Client.GetWorkItemsAsync(query, expand: true, cancellationToken: CancellationToken);

            result.Should().HaveCount(workItems.Count());
            result.Should().BeEquivalentTo(workItems);

            HttpClientMock.Verify();
        }
        #endregion

        #region GetWorkItemUpdates tests

        [Test]
        public async Task GetWorkItemUpdatesVerifyUrl()
        {
            const int workItemId = 78778;
            var updates = new[]
            {
                new WorkItemUpdate { WorkItemId = workItemId, Id = 1, Revision = 1 },
                new WorkItemUpdate { WorkItemId = workItemId, Id = 2, Revision = 2 }
            };

            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItemUpdate>>(It.Is<string>(u => VerifyUpdatesUrl(u, workItemId)), CancellationToken))
                .ReturnsAsync(new CollectionResponse<WorkItemUpdate> { Value = updates })
                .Verifiable();

            var result = await Client.GetWorkItemUpdatesAsync(workItemId, CancellationToken);

            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(updates);

            HttpClientMock.Verify();
        }

        [Test]
        public void GetWorkItemUpdatesShouldNotCatchExceptions()
        {
            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItemUpdate>>(It.IsAny<string>(), CancellationToken.None))
                .Throws<Exception>();

            Client.Awaiting(c => c.GetWorkItemUpdatesAsync(234))
                .Should().Throw<Exception>();
        }

        [Test]
        public async Task GetWorkItemUpdatesShouldReturnEmptyListIfResponseNull()
        {
            const int workItemId = 78778;

            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItemUpdate>>(It.IsAny<string>(), CancellationToken.None))
                .ReturnsAsync((CollectionResponse<WorkItemUpdate>)null);

            var result = await Client.GetWorkItemUpdatesAsync(workItemId);

            result.Should().HaveCount(0);
        }

        #endregion

        private bool VerifyWorkItemsUrl(string url, IEnumerable<string> fields, IEnumerable<int> ids)
        {
            var fieldsString = string.Join(',', fields);
            var idsString = string.Join(',', ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&fields={fieldsString}&api-version={Constants.CurrentWorkItemsApiVersion}";

            return string.Equals(url, expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyWorkItemsUrl(string url, IEnumerable<int> ids)
        {
            var idsString = string.Join(',', ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&$expand=All&api-version={Constants.CurrentWorkItemsApiVersion}";

            return string.Equals(url, expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUpdatesUrl(string url, int workitemId)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems/{workitemId}/updates?api-version={Constants.CurrentWorkItemsApiVersion}";
            return string.Equals(url, expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
