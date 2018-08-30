using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VSTS.Net.Models.Response;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.WorkItems.CRUD
{
    [TestFixture]
    public class DeleteWorkItemTests : BaseHttpClientTests
    {
        [Test]
        public async Task SendDeleteRequest([Values(true, false)]bool destroy)
        {
            const int workitemId = 124;
            HttpClientMock.Setup(c => c.ExecuteDelete<WorkItemDeleteResponse>(It.Is<string>(u => VerifyUrl(u, workitemId, destroy)), CancellationToken))
                .ReturnsAsync(new WorkItemDeleteResponse { Id = workitemId, Code = 200, DeletedBy = "Foo", DeletedDate = DateTime.UtcNow })
                .Verifiable();

            var result = await Client.DeleteWorkItemAsync(workitemId, destroy, CancellationToken);

            result.Should().BeTrue();
            HttpClientMock.Verify();
        }

        [Test]
        public void DoesNotCatchExceptions()
        {
            HttpClientMock.Setup(c => c.ExecuteDelete<WorkItemDeleteResponse>(It.IsAny<string>(), CancellationToken))
                .Throws<Exception>()
                .Verifiable();

            Client.Awaiting(c => c.DeleteWorkItemAsync(124, false, CancellationToken))
                .Should().Throw<Exception>();
        }

        private bool VerifyUrl(string url, int workitemId, bool destroy)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems/{workitemId}?";
            if (destroy)
            {
                expectedUrl += "destroy=true&";
            }

            expectedUrl += "api-version=";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
