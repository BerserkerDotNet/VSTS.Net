using System;
using System.Threading.Tasks;
using NUnit.Framework;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.WorkItems.CRUD
{
    [TestFixture]
    public class GetWorkItemExpandedTests : BaseHttpClientTests
    {
        [Test]
        public async Task VerifyExpandParameterInUrlAndNoFieldsParameterInUrl()
        {
            const int expectedWorkitemId = 2;
            var dt = DateTime.UtcNow.AddDays(-1);
            var workItem = new WorkItem { Id = expectedWorkitemId };
            SetupSingle(workItem, u => VerifyUrl(u, dt, expectedWorkitemId));

            var result = await Client.GetWorkItemExpandedAsync(expectedWorkitemId, dt, cancellationToken: CancellationToken);

            HttpClientMock.Verify();
        }

        private bool VerifyUrl(string url, DateTime? asOf, int expectedWorkitemId)
        {
            var asOfString = asOf.HasValue ? asOf.Value.ToString("u") : string.Empty;
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems/{expectedWorkitemId}?$expand=All&asOf={asOfString}&api-version";

            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
