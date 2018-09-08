using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VSTS.Net.Models.Response;
using VSTS.Net.Models.WorkItemsMetadata;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.WorkItems
{
    [TestFixture]
    public class GetWorkItemFieldsTests : BaseHttpClientTests
    {
        [Test]
        public async Task ReturnsListOfWorkItemFields()
        {
            var fields = new[] { new WorkItemField { Name = "Field 1" }, new WorkItemField { Name = "Field 2" } };

            SetupGetCollectionOf<WorkItemField>(url => VerifyUrl(url))
                .ReturnsAsync(new CollectionResponse<WorkItemField>() { Value = fields })
                .Verifiable();

            var result = await Client.GetWorkItemFieldsAsync(CancellationToken);

            result.Should().HaveCount(fields.Length);
            result.Should().BeEquivalentTo(fields);

            HttpClientMock.Verify();
        }

        [Test]
        public void DoesNotCatchExceptions()
        {
            SetupGetCollectionOf<WorkItemField>()
                .Throws<UriFormatException>();

            Client.Awaiting(c => c.GetWorkItemFieldsAsync(CancellationToken))
                .Should().Throw<UriFormatException>();
        }

        private bool VerifyUrl(string url)
        {
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/fields?api-version";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
