using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using VSTS.Net.Models.Response;
using VSTS.Net.Models.WorkItems;
using VSTS.Net.Tests.Types;

namespace VSTS.Net.Tests.WorkItems.CRUD
{
    [TestFixture]
    public class GetWorkItemsTests: BaseHttpClientTests
    {
        [Test]
        public void ThrowsIfEmptyListOfIds()
        {
            _client.Awaiting(c => c.GetWorkItemsAsync((int[])null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ReturnsEmptyCollectionIfEmptyListOfIds()
        {
            var result = await _client.GetWorkItemsAsync(new int[0]);

            result.Should().BeEmpty();
        }

        [Test]
        public async Task QueryWorkItems()
        {
            var workitems = new[] { new WorkItem { Id = 1 }, new WorkItem { Id = 2 } };
            SetupGetCollectionOf<WorkItem>()
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems });

            var result = await _client.GetWorkItemsAsync(new[] { 1, 2 }, cancellationToken: _cancellationToken);

            result.Should().BeEquivalentTo(workitems);
        }

        [Test]
        public async Task ReturnsEmptyListIfResponseIsNull()
        {
            SetupGetCollectionOf<WorkItem>()
                .ReturnsAsync((CollectionResponse<WorkItem>)null);

            var result = await _client.GetWorkItemsAsync(new[] { 1, 2 });

            result.Should().BeEmpty();
        }

        [Test]
        public async Task VerifyUrl()
        {
            var ids = new[] { 1, 2 };
            var fields = new[] { "System.Title", "System.Tags" };
            var asOf = DateTime.UtcNow.AddDays(-1);
            var workitems = new[] { new WorkItem { Id = 1 }, new WorkItem { Id = 2 } };
            SetupGetCollectionOf<WorkItem>(u => VerifyUrlWithIds(u, ids))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems })
                .Verifiable();
            SetupGetCollectionOf<WorkItem>(u => VerifyUrlWithAllParameters(u, ids, asOf, fields))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems })
                .Verifiable();
            SetupGetCollectionOf<WorkItem>(u => VerifyUrlWithOnlyAsOf(u, ids, asOf))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems })
                .Verifiable();
            SetupGetCollectionOf<WorkItem>(u => VerifyUrlWithOnlyFields(u, ids, fields))
                .ReturnsAsync(new CollectionResponse<WorkItem> { Value = workitems })
                .Verifiable();

            await _client.GetWorkItemsAsync(ids, cancellationToken: _cancellationToken);
            await _client.GetWorkItemsAsync(ids, asOf, fields, cancellationToken: _cancellationToken);
            await _client.GetWorkItemsAsync(ids, fields: fields, cancellationToken: _cancellationToken);
            await _client.GetWorkItemsAsync(ids, asOf, cancellationToken: _cancellationToken);

            _httpClientMock.Verify();
        }

        private bool VerifyUrlWithIds(string url, int[] ids)
        {
            var idsString = string.Join(",", ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&api-version";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlWithAllParameters(string url, int[] ids, DateTime asOf, string[] fields)
        {
            var fieldsString = fields != null ? string.Join(',', fields) : string.Empty;
            var asOfString = asOf.ToString("u");
            var idsString = string.Join(",", ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&fields={fieldsString}&asOf={asOfString}&api-version";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlWithOnlyAsOf(string url, int[] ids, DateTime asOf)
        {
            var asOfString = asOf.ToString("u");
            var idsString = string.Join(",", ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&asOf={asOfString}&api-version";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifyUrlWithOnlyFields(string url, int[] ids, string[] fields)
        {
            var fieldsString = fields != null ? string.Join(',', fields) : string.Empty;
            var idsString = string.Join(",", ids);
            var expectedUrl = $"https://{InstanceName}.visualstudio.com/_apis/wit/workitems?ids={idsString}&fields={fieldsString}&api-version";
            return url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
