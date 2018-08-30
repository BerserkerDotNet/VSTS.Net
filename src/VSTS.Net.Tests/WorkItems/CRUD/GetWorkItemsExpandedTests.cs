using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VSTS.Net.Models.Response;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.WorkItems.CRUD
{
    [TestFixture]
    public class GetWorkItemsExpandedTests : BaseHttpClientTests
    {
        [Test]
        public async Task VerifyUrlContainsExpandAndDoesNotContainFields()
        {
            var ids = new[] { 1, 2 };
            var asOf = DateTime.UtcNow.AddDays(-1);
            var workitems = new[] { new WorkItem { Id = 1 }, new WorkItem { Id = 2 } };
            SetupGetCollectionOf<WorkItem>(u => VerifyUrl(u, ids, asOf))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems })
                .Verifiable();

            await Client.GetWorkItemsExpandedAsync(ids, asOf, cancellationToken: CancellationToken);

            HttpClientMock.Verify();
        }

        private bool VerifyUrl(string url, int[] ids, DateTime asOf)
        {
            var asOfString = asOf.ToString("u");
            var idsString = string.Join(",", ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&$expand=All&asOf={asOfString}&api-version";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
