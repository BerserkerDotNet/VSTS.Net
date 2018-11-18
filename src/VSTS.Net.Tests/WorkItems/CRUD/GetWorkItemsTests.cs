using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using VSTS.Net.Exceptions;
using VSTS.Net.Models.Response;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Tests.Types;
using VSTS.Net.Types;

namespace VSTS.Net.Tests.WorkItems.CRUD
{
    [TestFixture]
    public class GetWorkItemsTests : BaseHttpClientTests
    {
        [Test]
        public void ThrowsIfEmptyListOfIds()
        {
            Client.Awaiting(c => c.GetWorkItemsAsync((int[])null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ReturnsEmptyCollectionIfEmptyListOfIds()
        {
            var result = await Client.GetWorkItemsAsync(new int[0]);

            result.Should().BeEmpty();
        }

        [Test]
        public async Task QueryWorkItems()
        {
            var workitems = new[] { new WorkItem { Id = 1 }, new WorkItem { Id = 2 } };
            SetupGetCollectionOf<WorkItem>()
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems });

            var result = await Client.GetWorkItemsAsync(new[] { 1, 2 }, cancellationToken: CancellationToken);

            result.Should().BeEquivalentTo(workitems);
        }

        [Test]
        public async Task ReturnsEmptyListIfResponseIsNull()
        {
            SetupGetCollectionOf<WorkItem>()
                .ReturnsAsync((CollectionResponse<WorkItem>)null);

            var result = await Client.GetWorkItemsAsync(new[] { 1, 2 });

            result.Should().BeEmpty();
        }

        [Test]
        public async Task RetrieveInBatchesIfTooManyIds()
        {
            var batch1 = Enumerable.Range(1, 10).Select(i => new WorkItem { Id = i }).ToArray();
            var batch2 = Enumerable.Range(11, 5).Select(i => new WorkItem { Id = i }).ToArray();
            Client.Configuration.WorkitemsBatchSize = 10;
            SetupPagedGetCollectionOf<WorkItem>(u => VerifyBatchUrl(u))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = batch1 })
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = batch2 });

            var result = await Client.GetWorkItemsAsync(Enumerable.Range(1, 15).ToArray(), fields: new[] { "Id" }, cancellationToken: CancellationToken);

            result.Should().HaveCount(15);

            HttpClientMock.Verify(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.IsAny<string>(), CancellationToken), Times.Exactly(2));
        }

        [Test]
        public async Task VerifyUrl()
        {
            var ids = new[] { 1, 2 };
            var fields = new[] { "System.Title", "System.Tags" };
            var asOf = DateTime.UtcNow.AddDays(-1);
            var workitems = new[] { new WorkItem { Id = 1 }, new WorkItem { Id = 2 } };
            SetupGetCollectionOf<WorkItem>(u => VerifyUrlWithIds(u, ids))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems })
                .Verifiable();
            SetupGetCollectionOf<WorkItem>(u => VerifyUrlWithAllParameters(u, ids, asOf, fields))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems })
                .Verifiable();
            SetupGetCollectionOf<WorkItem>(u => VerifyUrlWithOnlyAsOf(u, ids, asOf))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems })
                .Verifiable();
            SetupGetCollectionOf<WorkItem>(u => VerifyUrlWithOnlyFields(u, ids, fields))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems })
                .Verifiable();

            await Client.GetWorkItemsAsync(ids, cancellationToken: CancellationToken);
            await Client.GetWorkItemsAsync(ids, asOf, fields, cancellationToken: CancellationToken);
            await Client.GetWorkItemsAsync(ids, fields: fields, cancellationToken: CancellationToken);
            await Client.GetWorkItemsAsync(ids, asOf, cancellationToken: CancellationToken);

            HttpClientMock.Verify();
        }

        [Test]
        public void ThrowsArgumentNullIfIdEmpty()
        {
            Client.Awaiting(c => c.GetWorkItemsAsync(Guid.Empty, false, CancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task AggregatesWorkitemsFromFlatQuery()
        {
            var workitemReferences = new[] { new WorkItemReference(1), new WorkItemReference(2), new WorkItemReference(3) };
            var queryResult = new FlatWorkItemsQueryResult { QueryType = QueryType.Flat, WorkItems = workitemReferences };
            HttpClientMock.Setup(c => c.ExecuteGet<JObject>(It.IsAny<string>(), CancellationToken))
                .ReturnsAsync(JObject.FromObject(queryResult));

            var workitems = workitemReferences.Select(r => new WorkItem { Id = r.Id });
            SetupGetCollectionOf<WorkItem>(url => url.Contains("$expand=All"))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems });

            var result = await Client.GetWorkItemsAsync(Guid.NewGuid(), true, CancellationToken);

            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(workitems);

            HttpClientMock.VerifyAll();
        }

        [Test]
        public async Task AggregatesWorkitemsFromHierarchicalQuery()
        {
            var workitemLinks = new[]
            {
                new WorkItemLink(new WorkItemReference(1), new WorkItemReference(2), "forward"),
                new WorkItemLink(new WorkItemReference(1), new WorkItemReference(3), "forward"),
                new WorkItemLink(new WorkItemReference(2), new WorkItemReference(4), "forward"),
                new WorkItemLink(new WorkItemReference(5), new WorkItemReference(6), "forward"),
            };
            var queryResult = new HierarchicalWorkItemsQueryResult { QueryType = QueryType.Tree, WorkItemRelations = workitemLinks };
            HttpClientMock.Setup(c => c.ExecuteGet<JObject>(It.IsAny<string>(), CancellationToken))
                .ReturnsAsync(JObject.FromObject(queryResult));

            var workitems = workitemLinks.SelectMany(l => new[] { l.Source, l.Target })
                .Distinct()
                .Select(r => new WorkItem { Id = r.Id });
            SetupGetCollectionOf<WorkItem>(url => url.Contains("$expand=All"))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems });

            var result = await Client.GetWorkItemsAsync(Guid.NewGuid(), true, CancellationToken);

            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(workitems);

            HttpClientMock.VerifyAll();
        }

        [Test]
        public async Task RequestsColumnsFromQueryIfNoExpand()
        {
            var workitemReferences = new[] { new WorkItemReference(1), new WorkItemReference(2), new WorkItemReference(3) };
            var columns = new[] { new ColumnReference("C1", "System.C1"), new ColumnReference("C2", "System.C2") };
            var queryResult = new FlatWorkItemsQueryResult { QueryType = QueryType.Flat, WorkItems = workitemReferences, Columns = columns };
            HttpClientMock.Setup(c => c.ExecuteGet<JObject>(It.IsAny<string>(), CancellationToken))
                .ReturnsAsync(JObject.FromObject(queryResult));

            var workitems = workitemReferences.Select(r => new WorkItem { Id = r.Id });
            SetupGetCollectionOf<WorkItem>(url => url.Contains(string.Join(',', columns.Select(c => c.ReferenceName))) && !url.Contains("$expand=All"))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems });

            var result = await Client.GetWorkItemsAsync(Guid.NewGuid(), expand: false, cancellationToken: CancellationToken);

            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(workitems);

            HttpClientMock.VerifyAll();
        }

        [Test]
        public void ThrowsIfUnknownQueryType()
        {
            var queryResult = new FlatWorkItemsQueryResult { QueryType = QueryType.Unknown };
            HttpClientMock.Setup(c => c.ExecuteGet<JObject>(It.IsAny<string>(), CancellationToken))
                .ReturnsAsync(JObject.FromObject(queryResult));
            Client.Awaiting(c => c.GetWorkItemsAsync(Guid.NewGuid(), false, CancellationToken))
                .Should().Throw<UnknownWorkItemQueryTypeException>();
        }

        private bool VerifyUrlWithIds(string url, int[] ids)
        {
            var idsString = string.Join(",", ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&$expand=All&api-version";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlWithAllParameters(string url, int[] ids, DateTime asOf, string[] fields)
        {
            var fieldsString = fields != null ? string.Join(',', fields) : string.Empty;
            var asOfString = asOf.ToString("u");
            var idsString = string.Join(",", ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&fields={fieldsString}&asOf={asOfString}&api-version";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlWithOnlyAsOf(string url, int[] ids, DateTime asOf)
        {
            var asOfString = asOf.ToString("u");
            var idsString = string.Join(",", ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&$expand=All&asOf={asOfString}&api-version";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlWithOnlyFields(string url, int[] ids, string[] fields)
        {
            var fieldsString = fields != null ? string.Join(',', fields) : string.Empty;
            var idsString = string.Join(",", ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&fields={fieldsString}&api-version";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyBatchUrl(string url)
        {
            var batch1Ids = $"ids={string.Join(",", Enumerable.Range(1, 10))}&";
            var batch2Ids = $"ids={string.Join(",", Enumerable.Range(11, 5))}&";

            return url.Contains(batch1Ids) || url.Contains(batch2Ids);
        }
    }
}
