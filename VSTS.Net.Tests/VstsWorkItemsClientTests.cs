using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.Response.WorkItems;

namespace VSTS.Net.Tests
{
    [TestFixture]
    public class VstsWorkItemsClientTests
    {
        const string _instanceName = "Foo";
        Mock<IHttpClient> _httpClientMock;
        VstsClient _client;

        [SetUp]
        public void SetUp()
        {
            _httpClientMock = new Mock<IHttpClient>();
            _client = new VstsClient(_instanceName, _httpClientMock.Object);
        }

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
            _httpClientMock.Setup(c => c.ExecutePost<HierarchicalWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => q.IsHierarchical), CancellationToken.None))
                .Returns<string, WorkItemsQuery, CancellationToken>((_, __, ___) => Task.FromResult(new HierarchicalWorkItemsQueryResult()));
            _httpClientMock.Setup(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken.None))
                .Returns<string, WorkItemsQuery, CancellationToken>((_, __, ___) => Task.FromResult(new FlatWorkItemsQueryResult()));

            var result = await _client.ExecuteQueryAsync(query);

            result.Should().BeOfType(expectedReturnType);
            _httpClientMock.Verify(c => c.ExecutePost<HierarchicalWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => q.IsHierarchical), CancellationToken.None),
                Times.Exactly(isHierarchical ? 1 : 0));
            _httpClientMock.Verify(c => c.ExecutePost<FlatWorkItemsQueryResult>(It.IsAny<string>(), It.Is<WorkItemsQuery>(q => !q.IsHierarchical), CancellationToken.None),
                Times.Exactly(!isHierarchical ? 1 : 0));
        }
    }
}
