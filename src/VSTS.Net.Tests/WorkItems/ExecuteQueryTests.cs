using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Tests.Types;
using VSTS.Net.Types;

namespace VSTS.Net.Tests.WorkItems
{
    [TestFixture]
    public class ExecuteQueryTests : BaseHttpClientTests
    {
        [Test]
        public void ExecuteQueryShouldThrowArgumentNullExceptionIfQueryIsNull()
        {
            Client.Awaiting(async c => await c.ExecuteQueryAsync(null))
                .Should()
                .Throw<ArgumentNullException>("query");
        }

        [Test]
        public void ExecuteQueryShouldThrowArgumentExceptionIfQueryIsEmpty()
        {
            var query = WorkItemsQuery.Get(string.Empty);
            Client.Awaiting(async c => await c.ExecuteQueryAsync(query))
                .Should()
                .Throw<ArgumentException>();
        }

        [TestCase(false, typeof(FlatWorkItemsQueryResult))]
        [TestCase(true, typeof(HierarchicalWorkItemsQueryResult))]
        public async Task ExecuteQueryShouldReturnCorrectResultType(bool isHierarchical, Type expectedReturnType)
        {
            var query = WorkItemsQuery.Get("Dummy query", isHierarchical);
            HttpClientMock.Setup(c => c.ExecutePost<HierarchicalWorkItemsQueryResult>(It.Is<string>(u => VerifyWiqlQueryUrl(u)), It.Is<WorkItemsQuery>(q => q.IsHierarchical), CancellationToken))
                .ReturnsAsync(new HierarchicalWorkItemsQueryResult());
            HttpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.Is<string>(u => VerifyWiqlQueryUrl(u)), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken))
                .ReturnsAsync(new FlatWorkItemsQueryResult());

            var result = await Client.ExecuteQueryAsync(query, CancellationToken);

            result.Should().BeOfType(expectedReturnType);
            HttpClientMock.Verify(
                c => c.ExecutePost<HierarchicalWorkItemsQueryResult>(It.Is<string>(u => VerifyWiqlQueryUrl(u)), It.Is<WorkItemsQuery>(q => q.IsHierarchical), CancellationToken),
                Times.Exactly(isHierarchical ? 1 : 0));
            HttpClientMock.Verify(
                c => c.ExecutePost<FlatWorkItemsQueryResult>(It.Is<string>(u => VerifyWiqlQueryUrl(u)), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken),
                Times.Exactly(!isHierarchical ? 1 : 0));
        }

        [Test]
        public async Task ExecuteFlatQueryReturnsFlatQueryResults()
        {
            HttpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.Is<string>(u => VerifyWiqlQueryUrl(u)), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken))
                .ReturnsAsync(new FlatWorkItemsQueryResult())
                .Verifiable();

            var result = await Client.ExecuteFlatQueryAsync("Dummy query", CancellationToken);

            result.Should().BeOfType<FlatWorkItemsQueryResult>();
            HttpClientMock.VerifyAll();
        }

        [Test]
        public async Task ExecuteHierarchicalQueryReturnsHierarchicalQueryResults()
        {
            HttpClientMock.Setup(c => c.ExecutePost<HierarchicalWorkItemsQueryResult>(It.Is<string>(u => VerifyWiqlQueryUrl(u)), It.Is<WorkItemsQuery>(q => q.IsHierarchical), CancellationToken))
                .ReturnsAsync(new HierarchicalWorkItemsQueryResult())
                .Verifiable();

            var result = await Client.ExecuteHierarchicalQueryAsync("Dummy query", CancellationToken);

            result.Should().BeOfType<HierarchicalWorkItemsQueryResult>();
            HttpClientMock.VerifyAll();
        }

        private bool VerifyWiqlQueryUrl(string url)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/wiql?api-version={Constants.CurrentWorkItemsApiVersion}";
            return string.Equals(url, expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
