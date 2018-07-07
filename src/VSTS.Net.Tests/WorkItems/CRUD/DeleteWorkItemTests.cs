using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
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
            _httpClientMock.Setup(c => c.ExecuteDelete<WorkItemDeleteResponse>(It.Is<string>(u => VerifyUrl(u, workitemId, destroy)), _cancellationToken))
                .ReturnsAsync(new WorkItemDeleteResponse { Id = workitemId, Code = 200, DeletedBy = "Foo", DeletedDate = DateTime.UtcNow })
                .Verifiable();

            var result = await _client.DeleteWorkItemAsync(workitemId, destroy, _cancellationToken);

            result.Should().BeTrue();
            _httpClientMock.Verify();
        }

        [Test]
        public void DoesNotCatchExceptions()
        {
            _httpClientMock.Setup(c => c.ExecuteDelete<WorkItemDeleteResponse>(It.IsAny<string>(), _cancellationToken))
                .Throws<Exception>()
                .Verifiable();

            _client.Awaiting(c => c.DeleteWorkItemAsync(124, false, _cancellationToken))
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
