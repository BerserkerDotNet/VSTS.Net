using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using NUnit.Framework;
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

        protected Mock<IHttpClient> HttpClientMock { get; set; }

        protected VstsClient Client { get; set; }

        protected CancellationToken CancellationToken { get; set; }

        protected Random Random { get; set; } = new Random();

        [SetUp]
        public void SetUp()
        {
            HttpClientMock = new Mock<IHttpClient>();
            var factory = new OnlineUrlBuilderFactory(InstanceName);
            Client = new VstsClient(factory, HttpClientMock.Object, VstsClientConfiguration.Default, Mock.Of<ILogger<VstsClient>>());
            var source = new CancellationTokenSource();
            CancellationToken = source.Token;
        }

        protected virtual void Initialize()
        {
        }

        protected ISetup<IHttpClient, Task<CollectionResponse<T>>> SetupGetCollectionOf<T>(Expression<Func<string, bool>> urlPredicate = null)
        {
            if (urlPredicate == null)
            {
                urlPredicate = _ => true;
            }

            return HttpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<T>>(It.Is(urlPredicate), CancellationToken));
        }

        protected ISetupSequentialResult<Task<CollectionResponse<T>>> SetupPagedGetCollectionOf<T>(Expression<Func<string, bool>> urlPredicate = null)
        {
            if (urlPredicate == null)
            {
                urlPredicate = _ => true;
            }

            return HttpClientMock.SetupSequence(c => c.ExecuteGet<CollectionResponse<T>>(It.Is(urlPredicate), CancellationToken));
        }

        protected ISetupSequentialResult<Task<CollectionResponse<T>>> SetupOnePageOf<T>(IEnumerable<T> page, Expression<Func<string, bool>> urlPredicate = null)
        {
            if (urlPredicate == null)
            {
                urlPredicate = _ => true;
            }

            return HttpClientMock.SetupSequence(c => c.ExecuteGet<CollectionResponse<T>>(It.Is(urlPredicate), CancellationToken))
                .ReturnsAsync(new CollectionResponse<T> { Value = page })
                .ReturnsAsync(new CollectionResponse<T> { Value = Enumerable.Empty<T>() });
        }

        protected void SetupSingle<T>(T item, Expression<Func<string, bool>> urlPredicate = null)
        {
            MakeSureUrlPredicateExists(ref urlPredicate);

            HttpClientMock.Setup(c => c.ExecuteGet<T>(It.Is(urlPredicate), CancellationToken))
                .ReturnsAsync(item)
                .Verifiable();
        }

        protected ISetup<IHttpClient, Task<T>> SetupSingle<T>(Expression<Func<string, bool>> urlPredicate = null)
        {
            MakeSureUrlPredicateExists(ref urlPredicate);

            return HttpClientMock.Setup(c => c.ExecuteGet<T>(It.Is(urlPredicate), CancellationToken));
        }

        protected void VerifyPagedRequests<T>(Times times)
        {
            HttpClientMock.Verify(c => c.ExecuteGet<CollectionResponse<T>>(It.IsAny<string>(), CancellationToken), times);
        }

        private void MakeSureUrlPredicateExists(ref Expression<Func<string, bool>> urlPredicate)
        {
            if (urlPredicate == null)
            {
                urlPredicate = _ => true;
            }
        }
    }
}
