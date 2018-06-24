using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.WorkItems.CRUD
{
    [TestFixture]
    public class GetWorkItemTests : BaseHttpClientTests
    {
        [Test]
        public void ThrowsIfEmptyArguments([Values("", null)] string project)
        {
            _client.Awaiting(c => c.GetWorkItemAsync(project, 0))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ReturnsWorkItemById()
        {
            const int expectedWorkitemId = 2;
            var workItem = new WorkItem { Id = expectedWorkitemId };
            SetupSingle(workItem);

            var result = await _client.GetWorkItemAsync(ProjectName, expectedWorkitemId, cancellationToken: _cancellationToken);

            result.Should().NotBeNull();
            result.Id.Should().Be(expectedWorkitemId);
        }

        [Test]
        public async Task VerifyUrlHasAllParameters() 
        {
            const int expectedWorkitemId = 2;
            var dt = DateTime.UtcNow.AddDays(-1);
            var fields = new[] { "System.Title", "System.Foo", "System.Bla" };
            var workItem = new WorkItem { Id = expectedWorkitemId };
            SetupSingle(workItem, u => VerifyUrlWithAll(u, dt, fields, expectedWorkitemId));

            var result = await _client.GetWorkItemAsync(ProjectName, expectedWorkitemId, dt, fields, cancellationToken: _cancellationToken);

            _httpClientMock.Verify();
        }

        [Test]
        public async Task VerifyUrlWithOptionalParameters()
        {
            const int expectedWorkitemId = 2;
            var dt = DateTime.UtcNow.AddDays(-1);
            var fields = new[] { "System.Title", "System.Foo", "System.Bla" };
            var workItem = new WorkItem { Id = expectedWorkitemId };
            SetupSingle(workItem, u => VerifyUrlWithoutFields(u, dt, expectedWorkitemId));
            SetupSingle(workItem, u => VerifyUrlWithoutAsOf(u, fields, expectedWorkitemId));
            SetupSingle(workItem, u => VerifyUrlBasic(u, expectedWorkitemId));

            await _client.GetWorkItemAsync(ProjectName, expectedWorkitemId, null, fields, cancellationToken: _cancellationToken);
            await _client.GetWorkItemAsync(ProjectName, expectedWorkitemId, dt, cancellationToken: _cancellationToken);
            await _client.GetWorkItemAsync(ProjectName, expectedWorkitemId, cancellationToken: _cancellationToken);

            _httpClientMock.Verify();
        }

        [Test]
        public void PassThroughExceptions()
        {
            SetupSingle<WorkItem>().Throws<Exception>();

            _client.Awaiting(c => c.GetWorkItemAsync(ProjectName, 1, cancellationToken: _cancellationToken))
                .Should().Throw<Exception>();
        }

        private bool VerifyUrlWithAll(string url, DateTime? asOf, string[] fields, int expectedWorkitemId)
        {
            var fieldsString = fields != null ? string.Join(',', fields) : string.Empty;
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/{ProjectName}/_apis/wit/workitems/{expectedWorkitemId}?fields={fieldsString}&asOf={asOfString}";

            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlWithoutFields(string url, DateTime? asOf, int expectedWorkitemId)
        {
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/{ProjectName}/_apis/wit/workitems/{expectedWorkitemId}?asOf={asOfString}&api-version";

            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlWithoutAsOf(string url, string[] fields, int expectedWorkitemId)
        {
            var fieldsString = fields != null ? string.Join(',', fields) : string.Empty;
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/{ProjectName}/_apis/wit/workitems/{expectedWorkitemId}?fields={fieldsString}&api-version";

            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlBasic(string url, int expectedWorkitemId)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/{ProjectName}/_apis/wit/workitems/{expectedWorkitemId}?api-version";

            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
