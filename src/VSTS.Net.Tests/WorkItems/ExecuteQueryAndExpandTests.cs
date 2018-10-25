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
            var workitems = new[] { new WorkItem { Id = 1 }, new WorkItem() { Id = 2 }, new WorkItem { Id = 3 } };
            var workitemReferences = workitems.Select(w => new WorkItemReference(w.Id)).ToArray();
            var expectedQueryResult = new FlatWorkItemsQueryResult
            {
                QueryType = QueryType.Flat,
                WorkItems = workitemReferences,
                Columns = new[] { new ColumnReference("Id", "System.Id"), new ColumnReference("Title", "System.Title") },
                SortColumns = new[] { new SortColumn { Field = new ColumnReference("Title", "System.Title") } }
            };
            HttpClientMock.Setup(c => c.ExecutePost<JObject>(It.IsAny<string>(), It.IsAny<WorkItemsQuery>(), CancellationToken))
                .ReturnsAsync(JObject.FromObject(expectedQueryResult));
            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.IsAny<string>(), CancellationToken))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems });

            var result = await Client.ExecuteQueryAndExpandAsync("Fake query", false, CancellationToken);

            result.Should().BeOfType<FlatWorkItemsQueryResultWithWorkItems>();
            var flatQueryResult = result.As<FlatWorkItemsQueryResultWithWorkItems>();
            flatQueryResult.WorkItems.Should().BeEquivalentTo(workitems);
            flatQueryResult.QueryType.Should().Be(expectedQueryResult.QueryType);
            flatQueryResult.Columns.Should().BeEquivalentTo(expectedQueryResult.Columns);
            flatQueryResult.SortColumns.Should().BeEquivalentTo(expectedQueryResult.SortColumns);
        }

        [Test]
        public void ReturnsHeirarchicalQueryResultWithWorkitemsByQueryText()
        {
        }

        [Test]
        public async Task ReturnsFlatQueryResultWithWorkitemsByQueryId()
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
            HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<WorkItem>>(It.IsAny<string>(), CancellationToken))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems });

            var result = await Client.ExecuteQueryAndExpandAsync(Guid.NewGuid(), false, CancellationToken);

            result.Should().BeOfType<FlatWorkItemsQueryResultWithWorkItems>();
            var flatQueryResult = result.As<FlatWorkItemsQueryResultWithWorkItems>();
            flatQueryResult.WorkItems.Should().BeEquivalentTo(workitems);
            flatQueryResult.QueryType.Should().Be(expectedQueryResult.QueryType);
            flatQueryResult.Columns.Should().BeEquivalentTo(expectedQueryResult.Columns);
            flatQueryResult.SortColumns.Should().BeEquivalentTo(expectedQueryResult.SortColumns);
        }

        [Test]
        public void ReturnsHeirarchicalQueryResultWithWorkitemsByQueryId()
        {
        }
    }
}
