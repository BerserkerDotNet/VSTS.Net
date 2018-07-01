using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using VSTS.Net.Models.Request;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Types;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.WorkItems.CRUD
{
    [TestFixture]
    public class CreateWorkItemTests : BaseHttpClientTests
    {
        [Test, Sequential]
        public void ThrowsArgumentExceptionIfArgumentsNull([Values(null, "", ProjectName, ProjectName)]string project, [Values("Task", "Task", null, "")]string type)
        {
            _client.Awaiting(c => c.CreateWorkItemAsync(project, type, new WorkItem(), _cancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ThrowsArgumentExceptionIfWorkitemIsNull()
        {
            _client.Awaiting(c => c.CreateWorkItemAsync(ProjectName, "Task", null, _cancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task TranslatesWorkitemIntoUpdateRequest()
        {
            var workitem = new WorkItem();
            workitem.Fields = new System.Collections.Generic.Dictionary<string, string>();
            workitem.Fields.Add("System.Title", "Foo");
            workitem.Fields.Add("System.Tags", "Bla; Foo");

            _httpClientMock.Setup(c => c.ExecutePost<WorkItem>(It.IsAny<string>(), It.Is<UpdateWorkitemRequest.Update[]>(r => ValidateRequest(r, workitem)), Constants.JsonPatchMimeType, _cancellationToken))
                .ReturnsAsync(workitem)
                .Verifiable();

            var result = await _client.CreateWorkItemAsync(ProjectName, "Task", workitem, _cancellationToken);

            _httpClientMock.Verify();
            result.Should().Be(workitem);
        }

        [Test]
        public async Task SendsRequestToCorrectUrl()
        {
            const string type = "Bug";
            var workitem = new WorkItem();
            workitem.Fields = new System.Collections.Generic.Dictionary<string, string>();

            _httpClientMock.Setup(c => c.ExecutePost<WorkItem>(It.Is<string>(u => VerifyUrl(u, type)), It.IsAny<UpdateWorkitemRequest.Update[]>(), Constants.JsonPatchMimeType, _cancellationToken))
                .ReturnsAsync(workitem)
                .Verifiable();

            var result = await _client.CreateWorkItemAsync(ProjectName, type, workitem, _cancellationToken);

            _httpClientMock.Verify();
            result.Should().Be(workitem);
        }

        [Test]
        public void DoesNotCatchExceptions()
        {
            _httpClientMock.Setup(c => c.ExecutePost<WorkItem>(It.IsAny<string>(), It.IsAny<UpdateWorkitemRequest.Update[]>(), Constants.JsonPatchMimeType, _cancellationToken))
                .Throws<Exception>()
                .Verifiable();

            _client.Awaiting(c => c.CreateWorkItemAsync(ProjectName, "Bug", new WorkItem(), _cancellationToken));
        }

        private bool VerifyUrl(string url, string type)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/{ProjectName}/_apis/wit/workitems/${type}";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool ValidateRequest(UpdateWorkitemRequest.Update[] updates, WorkItem wi)
        {
            return updates.Count() == wi.Fields.Count &&
                wi.Fields.All(f => updates.Any(u => u.Operation == UpdateWorkitemRequest.OperationAdd && u.Path == $"/fields/{f.Key}" && u.Value == f.Value));
        }
    }
}
