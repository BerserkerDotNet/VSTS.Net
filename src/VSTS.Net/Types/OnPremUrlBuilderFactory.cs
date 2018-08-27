using System;
using VSTS.Net.Interfaces;

namespace VSTS.Net.Types
{
    public class OnPremUrlBuilderFactory : IVstsUrlBuilderFactory
    {
        private readonly Uri _baseUri;

        public OnPremUrlBuilderFactory(Uri baseUri)
        {
            _baseUri = baseUri;
        }

        public VstsUrlBuilder Create()
        {
            return VstsUrlBuilder.Create(_baseUri);
        }
    }
}
