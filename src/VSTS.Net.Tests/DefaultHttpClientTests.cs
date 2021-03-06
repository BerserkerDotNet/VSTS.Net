﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using VSTS.Net.Types;

namespace VSTS.Net.Tests
{
    [TestFixture]
    public class DefaultHttpClientTests
    {
        private const string SamplePostContent = "post content";
        private const string SamplePatchContent = "patch content";

        private DefaultHttpClient _client;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private string _sampleSerializedContent;

        [SetUp]
        public void SetUp()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _client = new DefaultHttpClient(new HttpClient(_mockHttpMessageHandler.Object), Mock.Of<ILogger<DefaultHttpClient>>());
            _sampleSerializedContent = JsonConvert.SerializeObject(new SampleResponse { IsSample = true });
        }

        [Test]
        public void ShouldThrowIfClientIsNull()
        {
            _client.Invoking(_ => new DefaultHttpClient(null, null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldSendGetRequest()
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => VerifyGetRequest(r)), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => GetMockResponse(request, cancellationToken));

            var result = await _client.ExecuteGet<SampleResponse>("http://foo.com/sample");

            result.Should().NotBeNull();
            result.IsSample.Should().BeTrue();
        }

        [Test]
        public async Task ShouldSendPostRequest()
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => VerifyPostRequest(r, SamplePostContent, Constants.JsonMimeType)), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => GetMockResponse(request, cancellationToken));

            var result = await _client.ExecutePost<SampleResponse>("http://foo.com/sample", SamplePostContent, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSample.Should().BeTrue();
        }

        [Test]
        public async Task ShouldSendPostRequestWithSpecificMimeType()
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => VerifyPostRequest(r, SamplePostContent, Constants.JsonPatchMimeType)), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => GetMockResponse(request, cancellationToken));

            var result = await _client.ExecutePost<SampleResponse>("http://foo.com/sample", SamplePostContent, Constants.JsonPatchMimeType, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSample.Should().BeTrue();
        }

        [Test]
        public async Task ShouldSendPatchRequest()
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => VerifyPatchRequest(r, SamplePatchContent)), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => GetMockResponse(request, cancellationToken))
                .Verifiable();

            var result = await _client.ExecutePatch<SampleResponse>("http://foo.com/sample", SamplePatchContent, Constants.JsonPatchMimeType, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSample.Should().BeTrue();

            _mockHttpMessageHandler.Verify();
        }

        [Test]
        public async Task ShouldSerializePayloadForPostRequest()
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => VerifyPostRequest(r, _sampleSerializedContent, Constants.JsonMimeType)), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => GetMockResponse(request, cancellationToken));

            var result = await _client.ExecutePost<SampleResponse>("http://foo.com/sample", new SampleResponse() { IsSample = true }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSample.Should().BeTrue();
        }

        [Test]
        public void ShouldThrowExceptionIfNotSuccess()
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => GetMockResponse(request, cancellationToken));

            _client.Awaiting(async c => await c.ExecuteGet<SampleResponse>("http://foo.com/error"))
                .Should().Throw<HttpRequestException>();

            _client.Awaiting(async c => await c.ExecutePost<SampleResponse>("http://foo.com/error", SamplePostContent, CancellationToken.None))
                .Should().Throw<HttpRequestException>();

            _client.Awaiting(async c => await c.ExecutePost<SampleResponse>("http://foo.com/error", new SampleResponse() { IsSample = true }, CancellationToken.None))
                .Should().Throw<HttpRequestException>();
        }

        private bool VerifyGetRequest(HttpRequestMessage request)
        {
            return request.Method == HttpMethod.Get && request.RequestUri.LocalPath == "/sample";
        }

        private bool VerifyPostRequest(HttpRequestMessage request, string expectedContent, string expectedMimeType)
        {
            var content = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return request.Method == HttpMethod.Post &&
                request.RequestUri.LocalPath == "/sample" &&
                content == expectedContent &&
                request.Content.Headers.ContentType.MediaType == expectedMimeType;
        }

        private bool VerifyPatchRequest(HttpRequestMessage request, string expectedContent)
        {
            var content = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return request.Method.Method == Constants.HttpMethodPatch &&
                request.RequestUri.LocalPath == "/sample" &&
                content == expectedContent &&
                request.Content.Headers.ContentType.MediaType == Constants.JsonPatchMimeType;
        }

        private Task<HttpResponseMessage> GetMockResponse(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.LocalPath == "/sample")
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.Content = new StringContent(_sampleSerializedContent, Encoding.UTF8, Constants.JsonMimeType);
                return Task.FromResult(response);
            }
            else if (request.RequestUri.LocalPath == "/error")
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                return Task.FromResult(response);
            }

            throw new NotImplementedException();
        }

        private class SampleResponse
        {
            public bool IsSample { get; set; }
        }
    }
}
