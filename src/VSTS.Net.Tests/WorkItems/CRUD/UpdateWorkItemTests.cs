using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Tests.Types;
using VSTS.Net.Types;

namespace VSTS.Net.Tests.WorkItems.CRUD
{
    [TestFixture]
    public class UpdateWorkItemTests : BaseHttpClientTests
    {
        [Test]
        public void ThrowsExceptionIfRequestIsNull()
        {
            Client.Awaiting(c => c.UpdateWorkItemAsync(null, CancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ThrowsExceptionIfWorkitemIdIsNull()
        {
            Client.Awaiting(c => c.UpdateWorkItemAsync(new UpdateWorkitemRequest(), CancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task SendUpdateRequest()
        {
            const int expectedId = 87094;
            var workitem = new WorkItem();
            workitem.Fields = new System.Collections.Generic.Dictionary<string, string>();
            var updateRequest = new UpdateWorkitemRequest(expectedId);
            updateRequest.AddFieldValue("System.Title", "Foo");
            updateRequest.AddFieldValue("System.Tags", "Bla");
            HttpClientMock.Setup(c => c.ExecutePatch<WorkItem>(It.Is<string>(u => VerifyUrl(u, expectedId)), It.Is<UpdateWorkitemRequest.Update[]>(r => ValidateRequest(r, updateRequest.Updates.ToArray())), Constants.JsonPatchMimeType, CancellationToken))
                .ReturnsAsync(workitem)
                .Verifiable();

            var result = await Client.UpdateWorkItemAsync(updateRequest, CancellationToken);

            HttpClientMock.Verify();
            result.Should().Be(workitem);
        }

        private bool VerifyUrl(string url, int id)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems/{id}";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool ValidateRequest(UpdateWorkitemRequest.Update[] original, UpdateWorkitemRequest.Update[] expected)
        {
            return original.Count() == expected.Count() &&
                expected.All(e => original.Any(u => u.Operation == UpdateWorkitemRequest.OperationAdd && u.Path == e.Path && u.Value == e.Value));
        }
    }
}
