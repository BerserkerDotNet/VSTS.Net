using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Tests.Types;
using VSTS.Net.Types;

namespace VSTS.Net.Tests.WorkItems
{
    [TestFixture]
    public class ExecuteQueryAndExpandTests : BaseHttpClientTests
    {
        [TestCase(null)]
        [TestCase("")]
        public void ThrowsArgumentExceptionIfIncorrectParameters(string query)
        {
            Client.Awaiting(c => c.ExecuteQueryAndExpandAsync(query, false, CancellationToken))
                .Should().Throw<ArgumentNullException>(nameof(query));
        }

        [Test]
        public void ThrowsArgumentExceptionIfIncorrectParameters()
        {
            Client.Awaiting(c => c.ExecuteQueryAndExpandAsync(Guid.Empty, false, CancellationToken))
                .Should().Throw<ArgumentNullException>("queryId");
        }

        [Test]
        public async Task ReturnsFlatQueryResultWithWorkitemsByQueryText()
        {
            await RunFlatQuery(async () => await Client.ExecuteQueryAndExpandAsync("Fake query", false, CancellationToken));
        }

        [Test]
        public async Task ReturnsHeirarchicalQueryResultWithWorkitemsByQueryText()
        {
            await RunHierarchicalQuery(async () => await Client.ExecuteQueryAndExpandAsync("Fake query", false, CancellationToken));
        }

        [Test]
        public async Task ReturnsFlatQueryResultWithWorkitemsByQueryId()
        {
            await RunFlatQuery(async () => await Client.ExecuteQueryAndExpandAsync(Guid.NewGuid(), false, CancellationToken));
        }

        [Test]
        public async Task ReturnsHeirarchicalQueryResultWithWorkitemsByQueryId()
        {
            await RunHierarchicalQuery(async () => await Client.ExecuteQueryAndExpandAsync(Guid.NewGuid(), false, CancellationToken));
        }

        private async Task RunFlatQuery(Func<Task<WorkItemsQueryResult>> queryExecutor)
        {
            var workitems = new[] { new WorkItem { Id = 1 }, new WorkItem() { Id = 2 }, new WorkItem { Id = 3 } };
            var workitemReferences = workitems.Select(w => new WorkItemReference(w.Id)).ToArray();
            var expectedQueryResult = new FlatWorkItemsQueryResult
            {
                QueryType = QueryType.Flat,
                WorkItems = workitemReferences,
                Columns = new[] { new ColumnReference("Id", "System.Id"), new ColumnReference("Title", "System.Title") },
                SortColumns = new[] { new SortColumn { Field = new ColumnReference("Title", "System.Title") } }
            };
            HttpClientMock.Setup(c => c.ExecuteGet<JObject>(It.IsAny<string>(), CancellationToken))
                .ReturnsAsync(JObject.FromObject(expectedQueryResult));
            HttpClientMock.Setup(c => c.ExecutePost<JObject>(It.IsAny<string>(), It.IsAny<WorkItemsQuery>(), CancellationToken))
                .ReturnsAsync(JObject.FromObject(expectedQueryResult));
            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.IsAny<string>(), CancellationToken))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems });

            var result = await queryExecutor();

            result.Should().BeOfType<FlatWorkItemsQueryResultWithWorkItems>();
            var flatQueryResult = result.As<FlatWorkItemsQueryResultWithWorkItems>();
            flatQueryResult.WorkItems.Should().BeEquivalentTo(workitems);
            flatQueryResult.QueryType.Should().Be(expectedQueryResult.QueryType);
            flatQueryResult.Columns.Should().BeEquivalentTo(expectedQueryResult.Columns);
            flatQueryResult.SortColumns.Should().BeEquivalentTo(expectedQueryResult.SortColumns);
        }

        private async Task RunHierarchicalQuery(Func<Task<WorkItemsQueryResult>> queryExecutor)
        {
            var workitems = new[] { new WorkItem { Id = 1 }, new WorkItem() { Id = 2 }, new WorkItem { Id = 3 } };
            var workitemLinks = new[]
            {
                new WorkItemLink(new WorkItemReference(1), new WorkItemReference(2), "Parent"),
                new WorkItemLink(new WorkItemReference(2), new WorkItemReference(3), "Parent"),
                new WorkItemLink(new WorkItemReference(1), new WorkItemReference(3), "Related")
            };
            var expectedQueryResult = new HierarchicalWorkItemsQueryResult
            {
                QueryType = QueryType.Tree,
                WorkItemRelations = workitemLinks,
                Columns = new[] { new ColumnReference("Id", "System.Id"), new ColumnReference("Title", "System.Title") },
                SortColumns = new[] { new SortColumn { Field = new ColumnReference("Title", "System.Title") } }
            };

            HttpClientMock.Setup(c => c.ExecutePost<JObject>(It.IsAny<string>(), It.IsAny<WorkItemsQuery>(), CancellationToken))
                .ReturnsAsync(JObject.FromObject(expectedQueryResult));
            HttpClientMock.Setup(c => c.ExecuteGet<JObject>(It.IsAny<string>(), CancellationToken))
                .ReturnsAsync(JObject.FromObject(expectedQueryResult));
            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.IsAny<string>(), CancellationToken))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems });

            var result = await queryExecutor();

            result.Should().BeOfType<HierarchicalWorkItemsQueryResultWithWorkItems>();
            var hierarchicalQueryResult = result.As<HierarchicalWorkItemsQueryResultWithWorkItems>();
            hierarchicalQueryResult.WorkItems.Should().BeEquivalentTo(workitems);
            hierarchicalQueryResult.WorkItemRelations.Should().BeEquivalentTo(workitemLinks);
            hierarchicalQueryResult.QueryType.Should().Be(expectedQueryResult.QueryType);
            hierarchicalQueryResult.Columns.Should().BeEquivalentTo(expectedQueryResult.Columns);
            hierarchicalQueryResult.SortColumns.Should().BeEquivalentTo(expectedQueryResult.SortColumns);
        }
    }
}
