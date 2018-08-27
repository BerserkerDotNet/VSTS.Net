using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.Response;
using VSTS.Net.Types;

namespace VSTS.Net.Tests.Types
{
    [TestFixture]
    public abstract class BaseHttpClientTests
    {
        protected const string ProjectName = "FooProject";
        protected const string RepositoryName = "BarRepository";
        protected const string InstanceName = "Foo";

        protected Mock<IHttpClient> _httpClientMock;
        protected VstsClient _client;
        protected CancellationToken _cancellationToken;
        protected Random _random = new Random();

        [SetUp]
        public void SetUp()
        {
            _httpClientMock = new Mock<IHttpClient>();
            var factory = new OnlineUrlBuilderFactory(InstanceName);
            _client = new VstsClient(factory, _httpClientMock.Object, VstsClientConfiguration.Default, Mock.Of<ILogger<VstsClient>>());
            var source = new CancellationTokenSource();
            _cancellationToken = source.Token;
        }

        protected virtual void Initialize()
        {
        }

        protected ISetup<IHttpClient, Task<CollectionResponse<T>>> SetupGetCollectionOf<T>(Expression<Func<string, bool>> urlPredicate = null)
        {
            if (urlPredicate == null)
                urlPredicate = _ => true;

            return _httpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<T>>(It.Is(urlPredicate), _cancellationToken));
        }

        protected ISetupSequentialResult<Task<CollectionResponse<T>>> SetupPagedGetCollectionOf<T>(Expression<Func<string, bool>> urlPredicate = null)
        {
            if (urlPredicate == null)
                urlPredicate = _ => true;

            return _httpClientMock.SetupSequence(c => c.ExecuteGet<CollectionResponse<T>>(It.Is(urlPredicate), _cancellationToken));
        }

        protected ISetupSequentialResult<Task<CollectionResponse<T>>> SetupOnePageOf<T>(IEnumerable<T> page, Expression<Func<string, bool>> urlPredicate = null)
        {
            if (urlPredicate == null)
                urlPredicate = _ => true;

            return _httpClientMock.SetupSequence(c => c.ExecuteGet<CollectionResponse<T>>(It.Is(urlPredicate), _cancellationToken))
                .ReturnsAsync(new CollectionResponse<T> { Value = page })
                .ReturnsAsync(new CollectionResponse<T> { Value = Enumerable.Empty<T>() });
        }

        protected void SetupSingle<T>(T item, Expression<Func<string, bool>> urlPredicate = null)
        {
            MakeSureUrlPredicateExists(ref urlPredicate);

            _httpClientMock.Setup(c => c.ExecuteGet<T>(It.Is(urlPredicate), _cancellationToken))
                .ReturnsAsync(item)
                .Verifiable();
        }

        protected ISetup<IHttpClient, Task<T>> SetupSingle<T>(Expression<Func<string, bool>> urlPredicate = null)
        {
            MakeSureUrlPredicateExists(ref urlPredicate);

            return _httpClientMock.Setup(c => c.ExecuteGet<T>(It.Is(urlPredicate), _cancellationToken));
        }

        protected void VerifyPagedRequests<T>(Times times)
        {
            _httpClientMock.Verify(c => c.ExecuteGet<CollectionResponse<T>>(It.IsAny<string>(), _cancellationToken), times);
        }

        private void MakeSureUrlPredicateExists(ref Expression<Func<string, bool>> urlPredicate)
        {
            if (urlPredicate == null)
                urlPredicate = _ => true;
        }
    }
}
