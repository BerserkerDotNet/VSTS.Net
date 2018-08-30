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
    public class CreateWorkItemTests : BaseHttpClientTests
    {
        [Test]
        [Sequential]
        public void ThrowsArgumentExceptionIfArgumentsNull([Values(null, "", ProjectName, ProjectName)]string project, [Values("Task", "Task", null, "")]string type)
        {
            Client.Awaiting(c => c.CreateWorkItemAsync(project, type, new WorkItem(), CancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ThrowsArgumentExceptionIfWorkitemIsNull()
        {
            Client.Awaiting(c => c.CreateWorkItemAsync(ProjectName, "Task", null, CancellationToken))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task TranslatesWorkitemIntoUpdateRequest()
        {
            var workitem = new WorkItem();
            workitem.Fields = new System.Collections.Generic.Dictionary<string, string>();
            workitem.Fields.Add("System.Title", "Foo");
            workitem.Fields.Add("System.Tags", "Bla; Foo");

            HttpClientMock.Setup(c => c.ExecutePost<WorkItem>(It.IsAny<string>(), It.Is<UpdateWorkitemRequest.Update[]>(r => ValidateRequest(r, workitem)), Constants.JsonPatchMimeType, CancellationToken))
                .ReturnsAsync(workitem)
                .Verifiable();

            var result = await Client.CreateWorkItemAsync(ProjectName, "Task", workitem, CancellationToken);

            HttpClientMock.Verify();
            result.Should().Be(workitem);
        }

        [Test]
        public async Task SendsRequestToCorrectUrl()
        {
            const string type = "Bug";
            var workitem = new WorkItem();
            workitem.Fields = new System.Collections.Generic.Dictionary<string, string>();

            HttpClientMock.Setup(c => c.ExecutePost<WorkItem>(It.Is<string>(u => VerifyUrl(u, type)), It.IsAny<UpdateWorkitemRequest.Update[]>(), Constants.JsonPatchMimeType, CancellationToken))
                .ReturnsAsync(workitem)
                .Verifiable();

            var result = await Client.CreateWorkItemAsync(ProjectName, type, workitem, CancellationToken);

            HttpClientMock.Verify();
            result.Should().Be(workitem);
        }

        [Test]
        public void DoesNotCatchExceptions()
        {
            HttpClientMock.Setup(c => c.ExecutePost<WorkItem>(It.IsAny<string>(), It.IsAny<UpdateWorkitemRequest.Update[]>(), Constants.JsonPatchMimeType, CancellationToken))
                .Throws<Exception>()
                .Verifiable();

            Client.Awaiting(c => c.CreateWorkItemAsync(ProjectName, "Bug", new WorkItem(), CancellationToken));
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
