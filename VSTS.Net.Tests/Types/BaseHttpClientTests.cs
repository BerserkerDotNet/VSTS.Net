using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.Response;

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

        [SetUp]
        public void SetUp()
        {
            _httpClientMock = new Mock<IHttpClient>();
            _client = new VstsClient(InstanceName, _httpClientMock.Object, Mock.Of<ILogger<VstsClient>>());
        }

        protected virtual void Initialize()
        {
        }

        protected ISetup<IHttpClient, Task<CollectionResponse<T>>> SetupGetCollectionOf<T>(Expression<Func<string, bool>> urlPredicate = null)
        {
            if (urlPredicate == null)
                urlPredicate = _ => true;

            return _httpClientMock.Setup(c => c.ExecuteGet<CollectionResponse<T>>(It.Is(urlPredicate), CancellationToken.None));
        }

        protected ISetupSequentialResult<Task<CollectionResponse<T>>> SetupPagedGetCollectionOf<T>(Expression<Func<string, bool>> urlPredicate = null)
        {
            if (urlPredicate == null)
                urlPredicate = _ => true;

            return _httpClientMock.SetupSequence(c => c.ExecuteGet<CollectionResponse<T>>(It.Is(urlPredicate), CancellationToken.None));
        }
    }
}
