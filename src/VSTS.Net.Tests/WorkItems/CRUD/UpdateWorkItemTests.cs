using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Types;
using VSTS.Net.Tests.Types;
using System.Linq;

namespace VSTS.Net.Tests.WorkItems.CRUD
{
    [TestFixture]
    public class UpdateWorkItemTests : BaseHttpClientTests
    {
        [Test]
        public void ThrowsExceptionIfProjectIsNull([Values("", null)]string project)
        {
            _client.Awaiting(c => c.UpdateWorkItemAsync(project, new UpdateWorkitemRequest(1), _cancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ThrowsExceptionIfRequestIsNull()
        {
            _client.Awaiting(c => c.UpdateWorkItemAsync(ProjectName, null, _cancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ThrowsExceptionIfWorkitemIdIsNull()
        {
            _client.Awaiting(c => c.UpdateWorkItemAsync(ProjectName, new UpdateWorkitemRequest(), _cancellationToken))
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
            _httpClientMock.Setup(c => c.ExecutePatch<WorkItem>(It.Is<string>(u=>VerifyUrl(u, expectedId)), It.Is<UpdateWorkitemRequest.Update[]>(r=>ValidateRequest(r, updateRequest.Updates.ToArray())), Constants.JsonPatchMimeType, _cancellationToken))
                .ReturnsAsync(workitem)
                .Verifiable();

            var result = await _client.UpdateWorkItemAsync(ProjectName, updateRequest, _cancellationToken);

            _httpClientMock.Verify();
            result.Should().Be(workitem);
        }

        private bool VerifyUrl(string url, int id)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/{ProjectName}/_apis/wit/workitems/{id}";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool ValidateRequest(UpdateWorkitemRequest.Update[] original, UpdateWorkitemRequest.Update[] expected)
        {
            return original.Count() == expected.Count() &&
                expected.All(e => original.Any(u => u.Operation == UpdateWorkitemRequest.OperationAdd && u.Path == e.Path && u.Value == e.Value));
        }
    }
}
