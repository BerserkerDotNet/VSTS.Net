using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VSTS.Net.Types;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.WorkItems
{
    [TestFixture]
    public class VstsWorkItemsClientTests : BaseHttpClientTests
    {
        #region ExecuteQuery tests

        [Test]
        public void ExecuteQueryShouldThrowArgumentNullExceptionIfQueryIsNull()
        {
            _client.Awaiting(async c => await c.ExecuteQueryAsync(null))
                .Should()
                .Throw<ArgumentNullException>("query");
        }

        [Test]
        public void ExecuteQueryShouldThrowArgumentNullExceptionIfConfigurationIsNull()
        {
            var client = new VstsClient(null, _httpClientMock.Object);
            client.Awaiting(async c => await c.ExecuteQueryAsync(new WorkItemsQuery("dummy")))
                .Should()
                .Throw<ArgumentNullException>("configuration");
        }

        [Test]
        public void ExecuteQueryShouldThrowArgumentExceptionIfQueryIsEmpty()
        {
            var query = new WorkItemsQuery(string.Empty);
            _client.Awaiting(async c => await c.ExecuteQueryAsync(query))
                .Should()
                .Throw<ArgumentException>();
        }

        [TestCase(false, typeof(FlatWorkItemsQueryResult))]
        [TestCase(true, typeof(HierarchicalWorkItemsQueryResult))]
        public async Task ExecuteQueryShouldReturnCorrectResultType(bool isHierarchical, Type expectedReturnType)
        {
            var query = new WorkItemsQuery("Dummy query", isHierarchical);
            _httpClientMock.Setup(c => c.ExecutePost<HierarchicalWorkItemsQueryResult>(It.Is<string>(u => VerifyWiqlQueryUrl(u)), It.Is<WorkItemsQuery>(q => q.IsHierarchical), _cancellationToken))
                .ReturnsAsync(new HierarchicalWorkItemsQueryResult());
            _httpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.Is<string>(u => VerifyWiqlQueryUrl(u)), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), _cancellationToken))
                .ReturnsAsync(new FlatWorkItemsQueryResult());

            var result = await _client.ExecuteQueryAsync(query, _cancellationToken);

            result.Should().BeOfType(expectedReturnType);
            _httpClientMock.Verify(c => c.ExecutePost<HierarchicalWorkItemsQueryResult>(It.Is<string>(u => VerifyWiqlQueryUrl(u)), It.Is<WorkItemsQuery>(q => q.IsHierarchical), _cancellationToken),
                Times.Exactly(isHierarchical ? 1 : 0));
            _httpClientMock.Verify(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.Is<string>(u => VerifyWiqlQueryUrl(u)), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), _cancellationToken),
                Times.Exactly(!isHierarchical ? 1 : 0));
        }
        #endregion

        #region GetWorkItems tests
        [Test]
        public async Task GetWorkItemsShouldComposeCorrectUrlForFlatQueries()
        {
            var query = new WorkItemsQuery("Dummy query");
            var queryResult = new FlatWorkItemsQueryResult
            {
                Columns = new[] { new ColumnReference("Id", "System.Id"), new ColumnReference("Title", "System.Title") },
                WorkItems = new[] { new WorkItemReference(2), new WorkItemReference(34), new WorkItemReference(56) }
            };
            var fields = queryResult.Columns.Select(c => c.ReferenceName);
            var ids = queryResult.WorkItems.Select(w => w.Id);
            var workItems = ids.Select(i => new WorkItem { Id = i });

            _httpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), _cancellationToken))
                .ReturnsAsync(queryResult)
                .Verifiable();

            _httpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.Is<string>(u => VerifyWorkItemsUrl(u, fields, ids)), _cancellationToken))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workItems })
                .Verifiable();

            var result = await _client.GetWorkItemsAsync(query, _cancellationToken);

            result.Should().HaveCount(workItems.Count());
            result.Should().BeEquivalentTo(workItems);

            _httpClientMock.Verify();
        }

        [Test]
        public async Task GetWorkItemsShouldComposeCorrectUrlForTreeQueries()
        {
            var query = new WorkItemsQuery("Dummy query", isHierarchical: true);
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

            _httpClientMock.Setup(c => c.ExecutePost<HierarchicalWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => q.IsHierarchical), _cancellationToken))
                .ReturnsAsync(queryResult)
                .Verifiable();

            _httpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.Is<string>(u => VerifyWorkItemsUrl(u, fields, ids)), _cancellationToken))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workItems })
                .Verifiable();

            var result = await _client.GetWorkItemsAsync(query, _cancellationToken);

            result.Should().HaveCount(workItems.Count());
            result.Should().BeEquivalentTo(workItems);

            _httpClientMock.Verify();
        }

        [Test]
        public void GetWorkItemsShouldThrowExceptionIfNullQuery()
        {
            var query = new WorkItemsQuery("Dummy query");

            _httpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken.None))
                .ReturnsAsync((FlatWorkItemsQueryResult)null);

            _client.Awaiting(c => c.GetWorkItemsAsync(query))
                .Should().Throw<NotSupportedException>();
        }

        [Test]
        public async Task GetWorkItemsShouldReturnEmptyListOfWorkItemsIfResponseIsNull()
        {
            var query = new WorkItemsQuery("Dummy query");

            _httpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken.None))
                .ReturnsAsync(new FlatWorkItemsQueryResult { Columns = Enumerable.Empty<ColumnReference>(), WorkItems = Enumerable.Empty<WorkItemReference>() });
            _httpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.IsAny<string>(), CancellationToken.None))
                .ReturnsAsync((CollectionResponse<WorkItem>)null);

            var result = await _client.GetWorkItemsAsync(query);

            result.Should().HaveCount(0);
        }

        [Test]
        public void GetWorkItemsShouldNotCatchExceptions()
        {
            var query = new WorkItemsQuery("Dummy query");

            // Test 1
            _httpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken.None))
                .Throws<Exception>();

            _client.Awaiting(c => c.GetWorkItemsAsync(query))
                .Should().Throw<Exception>();

            // Test 2
            _httpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken.None))
                .ReturnsAsync(new FlatWorkItemsQueryResult
                {
                    Columns = new[] {new ColumnReference("Title", "System.Title") },
                    WorkItems = new[] {new WorkItemReference(1) } });
            _httpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.IsAny<string>(), CancellationToken.None))
                .Throws(new Exception());

            _client.Awaiting(c => c.GetWorkItemsAsync(query))
                .Should().Throw<Exception>();
        }

        [Test]
        public async Task GetWorkItemsShouldReturnEmptyIfNoItemsFound()
        {
            var query = new WorkItemsQuery("Dummy query");
            var queryResult = new FlatWorkItemsQueryResult
            {
                Columns = new ColumnReference[0],
                WorkItems = new WorkItemReference[0]
            };

            _httpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), _cancellationToken))
                .ReturnsAsync(queryResult)
                .Verifiable();

            var result = await _client.GetWorkItemsAsync(query, _cancellationToken);

            result.Should().BeEmpty();

            _httpClientMock.Verify();
            _httpClientMock.Verify(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.IsAny<string>(), _cancellationToken), Times.Never());
        }
        #endregion

        #region GetWorkItemUpdates tests

        [Test]
        public async Task GetWorkItemUpdatesVerifyUrl()
        {
            const int workItemId = 78778;
            var updates = new[]
            {
                new WorkItemUpdate { WorkItemId=workItemId, Id=1, Revision = 1 },
                new WorkItemUpdate { WorkItemId=workItemId, Id=2, Revision = 2 }
            };

            _httpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItemUpdate>>(It.Is<string>(u => VerifyUpdatesUrl(u, workItemId)), _cancellationToken))
                .ReturnsAsync(new CollectionResponse<WorkItemUpdate> { Value = updates })
                .Verifiable();

            var result = await _client.GetWorkItemUpdatesAsync(workItemId, _cancellationToken);

            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(updates);

            _httpClientMock.Verify();
        }

        [Test]
        public void GetWorkItemUpdatesShouldNotCatchExceptions()
        {
            _httpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItemUpdate>>(It.IsAny<string>(), CancellationToken.None))
                .Throws<Exception>();

            _client.Awaiting(c => c.GetWorkItemUpdatesAsync(234))
                .Should().Throw<Exception>();
        }

        [Test]
        public async Task GetWorkItemUpdatesShouldReturnEmptyListIfResponseNull()
        {
            const int workItemId = 78778;

            _httpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItemUpdate>>(It.IsAny<string>(), CancellationToken.None))
                .ReturnsAsync((CollectionResponse<WorkItemUpdate>)null);

            var result = await _client.GetWorkItemUpdatesAsync(workItemId);

            result.Should().HaveCount(0);
        }

        #endregion

        private bool VerifyWiqlQueryUrl(string url)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/wiql?api-version={Constants.CurrentWorkItemsApiVersion}";
            return string.Equals(url, expectedUrl, StringComparison.Ordinal);
        }

        private bool VerifyWorkItemsUrl(string url, IEnumerable<string> fields, IEnumerable<int> ids)
        {
            var fieldsString = string.Join(',', fields);
            var idsString = string.Join(',', ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&fields={fieldsString}&api-version={Constants.CurrentWorkItemsApiVersion}";

            return string.Equals(url, expectedUrl, StringComparison.Ordinal);
        }

        private bool VerifyUpdatesUrl(string url, int workitemId)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems/{workitemId}/updates?api-version={Constants.CurrentWorkItemsApiVersion}";
            return string.Equals(url, expectedUrl, StringComparison.Ordinal);
        }
    }
}
