using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.WorkItems.CRUD
{
    [TestFixture]
    public class GetWorkItemTests : BaseHttpClientTests
    {
        [Test]
        public async Task ReturnsWorkItemById()
        {
            const int expectedWorkitemId = 2;
            var workItem = new WorkItem { Id = expectedWorkitemId };
            SetupSingle(workItem);

            var result = await Client.GetWorkItemAsync(expectedWorkitemId, cancellationToken: CancellationToken);

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

            var result = await Client.GetWorkItemAsync(expectedWorkitemId, dt, fields, cancellationToken: CancellationToken);

            HttpClientMock.Verify();
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

            await Client.GetWorkItemAsync(expectedWorkitemId, null, fields, cancellationToken: CancellationToken);
            await Client.GetWorkItemAsync(expectedWorkitemId, dt, cancellationToken: CancellationToken);
            await Client.GetWorkItemAsync(expectedWorkitemId, cancellationToken: CancellationToken);

            HttpClientMock.Verify();
        }

        [Test]
        public async Task VerifyAllFieldsExdpandedIfNoFieldsREquests()
        {
            const int expectedWorkitemId = 2;
            var dt = DateTime.UtcNow.AddDays(-1);
            var workItem = new WorkItem { Id = expectedWorkitemId };
            SetupSingle(workItem, u => VerifyUrlWithExpand(u, dt, expectedWorkitemId));

            var result = await Client.GetWorkItemAsync(expectedWorkitemId, dt, fields: null, cancellationToken: CancellationToken);

            HttpClientMock.Verify();
        }

        [Test]
        public void PassThroughExceptions()
        {
            SetupSingle<WorkItem>().Throws<Exception>();

            Client.Awaiting(c => c.GetWorkItemAsync(1, cancellationToken: CancellationToken))
                .Should().Throw<Exception>();
        }

        private bool VerifyUrlWithAll(string url, DateTime? asOf, string[] fields, int expectedWorkitemId)
        {
            var fieldsString = fields != null ? string.Join(',', fields) : string.Empty;
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems/{expectedWorkitemId}?fields={fieldsString}&asOf={asOfString}";

            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlWithoutFields(string url, DateTime? asOf, int expectedWorkitemId)
        {
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems/{expectedWorkitemId}?$expand=All&asOf={asOfString}&api-version";

            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlWithoutAsOf(string url, string[] fields, int expectedWorkitemId)
        {
            var fieldsString = fields != null ? string.Join(',', fields) : string.Empty;
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems/{expectedWorkitemId}?fields={fieldsString}&api-version";

            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlBasic(string url, int expectedWorkitemId)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems/{expectedWorkitemId}?$expand=All&api-version";

            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlWithExpand(string url, DateTime? asOf, int expectedWorkitemId)
        {
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems/{expectedWorkitemId}?$expand=All&asOf={asOfString}&api-version";

            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
